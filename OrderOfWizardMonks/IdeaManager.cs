using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Core;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Ideas;
using WizardMonks.Models.Projects; // Add this using statement

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
            // Pass the magus object down to get context
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
                // Look up the project using the ID from the activity
                var project = magus.ActiveProjects
                    .OfType<SpellInventionProject>()
                    .FirstOrDefault(p => p.ProjectId == invent.ProjectId);

                // If the project is found, get the ArtPair from the spell being invented
                if (project != null)
                {
                    return project.SpellToInvent.Base.ArtPair;
                }
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
                    var forms = MagicArts.GetEnumerator().Where(a => MagicArts.IsForm(a)).ToList();
                    int randomIndex = (int)(Die.Instance.RollDouble() * forms.Count);
                    form = forms[randomIndex];
                }
                else // The studied Art must be a Form.
                {
                    // The studied Art is the Form. We must select a random Technique.
                    form = study.Art;
                    var techniques = MagicArts.GetEnumerator().Where(a => MagicArts.IsTechnique(a)).ToList();
                    int randomIndex = (int)(Die.Instance.RollDouble() * techniques.Count);
                    technique = techniques[randomIndex];
                }

                return new ArtPair(technique, form);
            }

            // Add more contexts for other activities...

            // Fallback: base it on the magus's highest Arts.
            var topTech = magus.Arts.OrderByDescending(a => a.Value).First(a => MagicArts.IsTechnique(a.Ability));
            var topForm = magus.Arts.OrderByDescending(a => a.Value).First(a => MagicArts.IsForm(a.Ability));
            return new ArtPair(topTech.Ability, topForm.Ability);
        }
    }
}