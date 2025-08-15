using System.Collections.Generic;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class AbilityIncreaseHelper : AHelper
    {
        private readonly Ability _ability;

        public AbilityIncreaseHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, Ability ability, CalculateDesireFunc desireFunc)
            : base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _ability = ability;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_ageToCompleteBy <= _mage.SeasonalAge) return;

            // Option 1: Reading a book on the topic
            var readingHelper = new ReadingHelper(_ability, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
            readingHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);

            // Option 2: Practice (for non-Arts)
            if (!MagicArts.IsArt(_ability))
            {
                var practiceHelper = new PracticeHelper(_ability, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
                practiceHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }
            else
            {
                CharacterAbilityBase magicArt = _mage.GetAbility(_ability);
                double stockpile = _mage.GetVisCount(_ability);
                double visNeed = 0.5 + (magicArt.Value / 10.0);

                // if so, assume vis will return an average of 6XP + aura
                if (stockpile > visNeed)
                {
                    double gain = magicArt.GetValueGain(_mage.VisStudyRate);
                    double effectiveDesire = _desireFunc(gain, _conditionDepth);
                    StudyVisActivity visStudy = new(magicArt.Ability, effectiveDesire);
                    alreadyConsidered.Add(visStudy);
                    // consider the value of finding a better aura to study vis in
                    FindNewAuraHelper auraHelper = new(_mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);

                    // TODO: how do we decrement the cost of the vis?
                }
            }
        }
    }
}