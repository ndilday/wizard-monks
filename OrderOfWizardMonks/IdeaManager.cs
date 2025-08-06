using System;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Core;
using WizardMonks.Instances;
using WizardMonks.Models;

namespace WizardMonks
{
    public static class IdeaManager
    {
        private const double BASE_IDEA_CHANCE = 0.05; // 5% chance per season

        public static void CheckForIdea(Magus magus, IActivity activity)
        {
            // 1. Check for embedded Idea from reading
            if (activity is ReadActivity readActivity && readActivity.Book.EmbeddedIdea != null)
            {
                magus.AddIdea(readActivity.Book.EmbeddedIdea);
                return; // An embedded idea precludes a random one for the season
            }

            // 2. Roll for a random Idea
            double chance = BASE_IDEA_CHANCE * magus.Personality.GetDesireMultiplier(HexacoFacet.Inquisitiveness) * magus.Personality.GetDesireMultiplier(HexacoFacet.Creativity);
            if (Die.Instance.RollDouble() < chance)
            {
                GenerateRandomIdea(magus, activity);
            }
        }

        private static void GenerateRandomIdea(Magus magus, IActivity activity)
        {
            // This is where the "mixed system" logic lives.
            // For now, we will create a simple contextual Idea.
            // This can be expanded with personality-driven logic later.

            ArtPair inspiredArts = GetContextualArts(magus, activity);
            if (inspiredArts != null)
            {
                string desc = $"An idea for a {inspiredArts.Technique.AbilityName} {inspiredArts.Form.AbilityName} spell.";
                var Idea = new SpellIdea(inspiredArts, desc);
                magus.AddIdea(Idea);
            }
        }

        private static ArtPair GetContextualArts(Magus magus, IActivity activity)
        {
            // Example of contextual logic
            if (activity is InventSpellActivity invent)
            {
                return invent.Spell.Base.ArtPair;
            }
            if (activity is StudyVisActivity study)
            {
                // Idea is more likely to use the Art being studied as either Tech or Form
                Ability otherArt = MagicArts.GetEnumerator().ElementAt(Die.Instance.RollSimpleDie() - 1 + Die.Instance.RollSimpleDie() - 1);
                if (Die.Instance.RollDouble() < 0.5)
                {
                    return new ArtPair(study.Art, otherArt);
                }
                else
                {
                    return new ArtPair(otherArt, study.Art);
                }
            }
            // Add more contexts for other activities...

            // Fallback: base it on the magus's highest Arts
            var topTech = magus.Arts.OrderByDescending(a => a.Value).First(a => MagicArts.IsTechnique(a.Ability));
            var topForm = magus.Arts.OrderByDescending(a => a.Value).First(a => MagicArts.IsForm(a.Ability));
            return new ArtPair(topTech.Ability, topForm.Ability);
        }
    }
}