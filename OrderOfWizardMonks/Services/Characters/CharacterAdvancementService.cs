using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Decisions;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;
using WizardMonks.Models;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Events;
using WizardMonks.Models.Ideas;

namespace WizardMonks.Services.Characters
{
    public static class CharacterAdvancementService
    {
        private static readonly AppraisalEngine _appraisalEngine = new();
        private static readonly ReflectionEngine _reflectionEngine = new();
        private static readonly IReflectionPolicy _reflectionPolicy = new TickSynchronizedReflectionPolicy();
        private static readonly IGoalGenerator _goalGenerator = new GoalGenerator();
        /// <summary>
        /// Determines the value of an experience gain in terms of practice seasons
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="gain"></param>
        /// <returns>the season equivalence of this gain</returns>
        public static double RateSeasonalExperienceGain(this Character character, Ability ability, double gain)
        {
            if (!MagicArts.IsArt(ability))
            {
                return gain / 4;
            }
            if (character is HermeticMagus mage)
            {
                double baseDistillVisRate = mage.GetVisDistillationRate();
                double distillVisRate = baseDistillVisRate;
                if (MagicArts.IsTechnique(ability))
                {
                    distillVisRate /= 4.0;
                }
                else if (ability != MagicArts.Vim)
                {
                    distillVisRate /= 2.0;
                }

                CharacterAbilityBase charAbility = mage.GetAbility(ability);
                double visUsedPerStudySeason = 0.5 + (charAbility.Value + charAbility.GetValueGain(gain) / 2) / 10.0;
                // the gain per season depends on how the character views vis
                double studySeasons = gain / mage.GetVisStudyAuraBonus();
                double visNeeded = studySeasons * visUsedPerStudySeason;
                // compare to the number of seasons we would need to extract the vis
                // plus the number of seasons we would need to study the extracted vis
                double extractTime = visNeeded / distillVisRate;
                double totalVisEquivalent = (extractTime + studySeasons) * baseDistillVisRate;

                // credit back the value of the exposure gained in the process of distilling
                double exposureGained = 2.0 * extractTime;
                double exposureSeasonsOfVis = exposureGained / mage.GetVisStudyAuraBonus();
                CharacterAbilityBase vim = mage.GetAbility(MagicArts.Vim);
                CharacterAbilityBase creo = mage.GetAbility(MagicArts.Creo);
                CharacterAbilityBase exposureAbility = creo.Value < vim.Value ? creo : vim;
                double visValueOfExposure = 0.5 + (exposureAbility.Value + exposureAbility.GetValueGain(exposureGained) / 2) / 10.0 * exposureSeasonsOfVis;
                return totalVisEquivalent - visValueOfExposure;
            }
            return 0;
        }

        public static IActivity DecideSeasonalActivity(this Character character)
        {
            character.Desires = new Desires();
            if (character.IsCollaborating)
            {
                return character.MandatoryAction;
            }

            ConsideredActions actions = new();

            // Legacy goals (bootstrap and any not yet migrated to the intention system).
            foreach (IGoal goal in character.ActiveGoals)
            {
                if (!goal.IsComplete())
                {
                    List<string> dummy = new List<string>();
                    goal.AddActionPreferencesToList(actions, character.Desires, dummy);
                }
            }

            // Intention-wrapped goals from the cognitive architecture.
            foreach (Intention intention in character.ActiveIntentions)
            {
                if (!intention.UnderlyingGoal.IsComplete())
                {
                    List<string> dummy = new List<string>();
                    intention.UnderlyingGoal.AddActionPreferencesToList(actions, character.Desires, dummy);
                }
            }

            character.Log.AddRange(actions.Log());
            return actions.GetBestAction();
        }

        public static void ReprioritizeGoals(this Character character)
        {
            foreach (IGoal goal in character.ActiveGoals.ToList())
            {
                if (!goal.IsComplete())
                {
                    if (goal.AgeToCompleteBy < character.SeasonalAge)
                    {
                        character.Log.Add("Failed to achieve a goal");
                        character.ActiveGoals.Remove(goal);
                    }
                }
            }

            // Prune completed intentions from the new system.
            character.ActiveIntentions.RemoveAll(i => i.UnderlyingGoal.IsComplete());
        }

        public static void CommitAction(this Character character, IActivity action)
        {
            character.AddSeasonActivity(action);
            action.Act(character);
            if (character.SeasonalAge >= 140)
            {
                character.Age(character.LongevityRitual);
            }
        }

        public static IActivity Advance(this Character character)
        {
            // Decay emotion tokens at the start of each tick before activity selection.
            character.Emotions.Tick();

            IActivity activity = null;
            if (!character.IsCollaborating)
            {
                character.Log.Add("");
                character.Log.Add(character.CurrentSeason.ToString() + " " + character.GetSeasonActivityLength() / 4);
                activity = character.DecideSeasonalActivity();
                character.AddSeasonActivity(activity);
                activity.Act(character);
                if (character.SeasonalAge >= 140)
                {
                    character.Age(character.LongevityRitual);
                }
                switch (character.CurrentSeason)
                {
                    case Season.Spring:
                        character.CurrentSeason = Season.Summer;
                        break;
                    case Season.Summer:
                        character.CurrentSeason = Season.Autumn;
                        break;
                    case Season.Autumn:
                        character.CurrentSeason = Season.Winter;
                        break;
                    case Season.Winter:
                        character.CurrentSeason = Season.Spring;
                        break;
                }

                RunCognitiveStack(character, activity);
            }
            character.IsCollaborating = false;
            character.ReprioritizeGoals();
            return activity;
        }

        /// <summary>
        /// Runs the post-activity cognitive pipeline: appraise events, reflect,
        /// generate new intentions, prune reconsidered ones.
        /// </summary>
        private static void RunCognitiveStack(Character character, IActivity activity)
        {
            int currentTick = (int)character.SeasonalAge;
            float maxEventImportance = 0f;

            // Appraise the activity outcome event if one was emitted.
            if (activity is AMageActivity mageActivity && mageActivity.EmittedEvent != null)
                maxEventImportance = AppraiseEvent(character, mageActivity.EmittedEvent, currentTick);

            // Appraise aging event if one occurred this tick.
            if (character.LastAgingEvent != null)
            {
                maxEventImportance = Math.Max(maxEventImportance,
                    AppraiseEvent(character, character.LastAgingEvent, currentTick));
                character.LastAgingEvent = null;
            }

            // Reflect: synthesize beliefs from accumulated memory entries.
            bool beliefRevised = false;
            if (_reflectionPolicy.ShouldReflect(character, character.Memory, currentTick))
            {
                _reflectionEngine.Reflect(character, character.Memory, character.CognitiveBeliefs, currentTick);
                character.Memory.ExpireStaleEntries(currentTick);
                beliefRevised = true;
            }

            // Generate new intentions from current emotional and belief state.
            var newIntentions = _goalGenerator.GenerateIntentions(
                character, character.Emotions, character.CognitiveBeliefs,
                character.ActiveIntentions, currentTick);
            foreach (var intention in newIntentions)
            {
                character.ActiveIntentions.Add(intention);
                character.Log.Add($"[Cognition] New intention formed: {intention.UnderlyingGoal.GetType().Name}");
            }

            // Context-change goal generation: certain completions trigger immediate
            // re-evaluation regardless of emotional pressure thresholds.
            if (activity is AMageActivity completedMageActivity && completedMageActivity.EmittedEvent != null)
                TryGenerateContextChangeIntentions(character, completedMageActivity.EmittedEvent, currentTick);

            // Prune intentions that should be reconsidered.
            // Use the most conflicting emotion (Fear or Distress) as the signal.
            float conflictingEmotion = Math.Max(
                character.Emotions.GetIntensity(EmotionType.Fear),
                character.Emotions.GetIntensity(EmotionType.Distress));

            var pruned = character.ActiveIntentions
                .Where(i => i.ShouldReconsider(maxEventImportance, beliefRevised, conflictingEmotion, currentTick))
                .ToList();
            foreach (var intention in pruned)
            {
                character.ActiveIntentions.Remove(intention);
                character.Log.Add($"[Cognition] Intention reconsidered: {intention.UnderlyingGoal.GetType().Name}");
            }
        }

        /// <summary>
        /// Appraises a single WorldEvent: pushes resulting tokens into the emotion ledger
        /// and appends the memory entry. Returns the importance weight of the entry, or 0.
        /// </summary>
        private static float AppraiseEvent(Character character, WorldEvent worldEvent, int currentTick)
        {
            var entry = _appraisalEngine.Appraise(
                character, worldEvent, character.ActiveIntentions, character.Emotions);
            if (entry == null) return 0f;

            foreach (var token in entry.EmotionSnapshot)
                character.Emotions.Add(token);

            character.Memory.Append(entry);
            return entry.ImportanceWeight;
        }

        /// <summary>
        /// Handles goal generation triggered by specific event completions rather than
        /// accumulated emotional pressure. A mage who finishes a spell or achieves a
        /// breakthrough immediately re-evaluates what to work on next — no threshold needed.
        /// </summary>
        private static void TryGenerateContextChangeIntentions(
            Character character, WorldEvent triggeredEvent, int currentTick)
        {
            if (character is not HermeticMagus magus) return;

            bool isCompletion = triggeredEvent.Category is
                WorldEventCategory.SpellInvented or
                WorldEventCategory.BreakthroughMade;

            if (!isCompletion) return;

            float inquisitiveness = (float)character.Personality.GetFacet(HexacoFacet.Inquisitiveness);
            float commitment = Math.Clamp(inquisitiveness * 0.5f, 0.1f, 0.9f);
            float desire = Math.Clamp(inquisitiveness * 0.5f, 0.1f, 1.0f);
            int stagnation = (int)Math.Clamp(
                8 * character.Personality.GetFacet(HexacoFacet.Prudence), 2, 24);

            var pursuedIds = character.ActiveIntentions
                .Select(i => i.UnderlyingGoal)
                .OfType<PursueIdeaGoal>()
                .Select(g => g.Idea.Id)
                .ToHashSet();

            // Pick the next unintenioned idea, if any.
            float envyIntensity = character.Emotions.GetIntensity(EmotionType.Envy);
            var nextIdea = magus.GetInspirations()
                .Where(idea => !pursuedIds.Contains(idea.Id))
                .Select(idea => (idea, score: ScoreIdeaForContextChange(idea, magus, envyIntensity)))
                .OrderByDescending(x => x.score)
                .Select(x => x.idea)
                .FirstOrDefault();

            if (nextIdea != null)
            {
                character.ActiveIntentions.Add(
                    new Intention(new PursueIdeaGoal(magus, nextIdea), commitment, desire, currentTick, stagnation));
                character.Log.Add(
                    $"[Cognition] Context change ({triggeredEvent.Category}): pursuing '{nextIdea.Description}'");
            }
        }

        private static float ScoreIdeaForContextChange(AIdea idea, HermeticMagus magus, float envyIntensity)
        {
            if (idea is SpellIdea spellIdea)
            {
                return (float)(magus.Arts.GetAbility(spellIdea.Arts.Technique).Value
                             + magus.Arts.GetAbility(spellIdea.Arts.Form).Value);
            }
            if (idea is BreakthroughIdea)
                return 5f + envyIntensity * 10f;
            return 0f;
        }

        public static IActivity MageAdvance(this HermeticMagus mage)
        {
            mage.IsBestBookCacheClean = false;
            // harvest vis
            foreach (Aura aura in mage.GetOwnedAuras())
            {
                foreach (VisSource source in aura.VisSources)
                {
                    if ((mage.CurrentSeason & source.Seasons) == mage.CurrentSeason)
                    {
                        mage.VisStock[source.Art] += source.Amount;
                    }
                }
            }
            mage.Log.Add("VIS STOCK");
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                if (mage.VisStock[art] > 0)
                {
                    mage.Log.Add(art.AbilityName + ": " + mage.VisStock[art].ToString("0.00"));
                }
            }
            IActivity activity = mage.Advance();
            IdeaManager.CheckForIdea(mage, activity);
            return activity;
        }

        public static void Advance(this Character character, IActivity activity)
        {
            character.Log.Add("");
            character.Log.Add(character.CurrentSeason.ToString() + " " + character.GetSeasonActivityLength() / 4);
            character.AddSeasonActivity(activity);
            activity.Act(character);
            if (character.SeasonalAge >= 140)
            {
                character.Age(character.LongevityRitual);
            }
            switch (character.CurrentSeason)
            {
                case Season.Spring:
                    character.CurrentSeason = Season.Summer;
                    break;
                case Season.Summer:
                    character.CurrentSeason = Season.Autumn;
                    break;
                case Season.Autumn:
                    character.CurrentSeason = Season.Winter;
                    break;
                case Season.Winter:
                    character.CurrentSeason = Season.Spring;
                    break;
            }
            character.IsCollaborating = true;
        }

        public static void PlanToBeTaught(this Character character)
        {
            character.IsCollaborating = true;
            //_mandatoryAction =
        }
    }
}
