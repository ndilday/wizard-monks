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
                foreach (IGoal goal in character.Goals)
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
            foreach (IGoal goal in character.Goals.ToList())
            {
                if (!goal.IsComplete())
                {
                    if (goal.AgeToCompleteBy < character.SeasonalAge)
                    {
                        character.Log.Add("Failed to achieve a goal");
                        character.Goals.Remove(goal);
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
            foreach (Aura aura in mage.KnownAuras)
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
