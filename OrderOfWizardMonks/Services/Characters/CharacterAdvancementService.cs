using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Decisions;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;
using WizardMonks.Models;
using WizardMonks.Models.Characters;

namespace WizardMonks.Services.Characters
{
    public static class CharacterAdvancementService
    {
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
            if (character is Magus mage)
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
                double studySeasons = gain / mage.VisStudyRate;
                double visNeeded = studySeasons * visUsedPerStudySeason;
                // compare to the number of seasons we would need to extract the vis
                // plus the number of seasons we would need to study the extracted vis
                double extractTime = visNeeded / distillVisRate;
                double totalVisEquivalent = (extractTime + studySeasons) * baseDistillVisRate;

                // credit back the value of the exposure gained in the process of distilling
                double exposureGained = 2.0 * extractTime;
                double exposureSeasonsOfVis = exposureGained / mage.VisStudyRate;
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
            else
            {
                ConsideredActions actions = new();
                foreach (IGoal goal in character.ActiveGoals)
                {
                    if (!goal.IsComplete())
                    {
                        List<string> dummy = new List<string>();
                        goal.AddActionPreferencesToList(actions, character.Desires, dummy);
                    }
                }
                character.Log.AddRange(actions.Log());
                return actions.GetBestAction();
            }
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
            }
            character.IsCollaborating = false;
            character.ReprioritizeGoals();
            return activity;
        }

        public static IActivity MageAdvance(this Magus mage)
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
