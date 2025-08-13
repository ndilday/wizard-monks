using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Core;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Ideas;

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
            // Inventing a spell. The idea is for a related spell.
            if (activity is InventSpellActivity invent)
            {
                return invent.Spell.Base.ArtPair;
            }

            // Studying vis. The idea is inspired by the Art being studied.
            if (activity is StudyVisActivity study)
            {
                Ability technique, form;

                // Determine if the studied Art is a Technique or a Form.
                if (MagicArts.IsTechnique(study.Art))
                {
                    // The studied Art is the Technique. We must select a random Form.
                    technique = study.Art;
                    var forms = MagicArts.GetEnumerator().Where(MagicArts.IsForm).ToList();
                    int randomIndex = (int)(Die.Instance.RollDouble() * forms.Count);
                    form = forms[randomIndex];
                }
                else // The studied Art must be a Form.
                {
                    // The studied Art is the Form. We must select a random Technique.
                    form = study.Art;
                    var techniques = MagicArts.GetEnumerator().Where(MagicArts.IsTechnique).ToList();
                    int randomIndex = (int)(Die.Instance.RollDouble() * techniques.Count);
                    technique = techniques[randomIndex];
                }

                return new ArtPair(technique, form);
            }

            // Add more contexts for other activities...

            // Fallback: base it on the magus's highest Arts. This logic is sound.
            var topTech = magus.Arts.OrderByDescending(a => a.Value).First(a => MagicArts.IsTechnique(a.Ability));
            var topForm = magus.Arts.OrderByDescending(a => a.Value).First(a => MagicArts.IsForm(a.Ability));
            return new ArtPair(topTech.Ability, topForm.Ability);
        }
    }
}