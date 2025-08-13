using System;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Activities;
using WizardMonks.Decisions;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities.MageActivities
{
    public class TranslateShorthandActivity : AExposingMageActivity
    {
        public LabText TargetText { get; private set; }

        public TranslateShorthandActivity(LabText textToTranslate, Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.TranslateLabText;
            TargetText = textToTranslate;
        }

        protected override void DoMageAction(Magus mage)
        {
            mage.Log.Add($"Working to decipher the shorthand for '{TargetText.SpellContained.Name}' by {TargetText.Author.Name}.");

            // Get existing progress, if any.
            double? previousProgress = mage.GetLabTextTranslationProgress(TargetText);

            // Add this season's progress.
            double currentProgress = mage.GetLabTotal(TargetText.SpellContained.Base.ArtPair, Activity.TranslateLabText);
            if(previousProgress != null )
            {
                currentProgress += (double)previousProgress;
            }
            ushort? previousTranslation = mage.GetDeciperedLabTextLevel(TargetText.Author);
            if(previousTranslation != null )
            {
                currentProgress += (ushort)previousTranslation;
            }

            // Check for completion.
            if (currentProgress >= TargetText.SpellContained.Level)
            {
                mage.Log.Add("Success! The shorthand has been deciphered.");

                // Update the deciphered level for this author.
                mage.AddDecipheredLabTextLevel(TargetText.Author, TargetText.SpellContained.Level);
            }
            else
            {
                // Update progress.
                mage.SetLabTextTranslationProgress(TargetText, currentProgress);
                mage.Log.Add($"Accumulated {currentProgress:F1} of {TargetText.SpellContained.Level} points needed.");
            }
        }

        public override bool Matches(IActivity action)
        {
            return action is TranslateShorthandActivity ta && ta.TargetText == this.TargetText;
        }

        public override string Log()
        {
            return $"Translating '{TargetText.SpellContained.Name}' worth {Desire:0.000}";
        }
    }
}