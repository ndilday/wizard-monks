using System.Collections.Generic;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class LabImprovementHelper : AHelper
    {
        private readonly ArtPair _arts;
        private readonly Activity _activity;

        public LabImprovementHelper(Ability exposureAbility, Magus mage, uint ageToCompleteBy, ushort conditionDepth, ArtPair arts, Activity activity, CalculateDesireFunc desireFunc)
            : base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _arts = arts;
            _activity = activity;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_mage.Laboratory == null || _ageToCompleteBy <= _mage.SeasonalAge)
            {
                return;
            }

            // organized and perfectionistic magi are most likely to want to keep tweaking their lab
            double desire = _mage.Personality.GetDesireMultiplier(HexacoFacet.Organization) * _mage.Personality.GetDesireMultiplier(HexacoFacet.Perfectionism);

            if (_mage.Laboratory.Specialization != null)
            {
                var prereqs = _mage.Laboratory.Specialization.GetPrerequisitesForNextStage();
                
                if(prereqs.MagicTheory > _mage.GetAbility(Abilities.MagicTheory).Value)
                {
                    // need to improve magic theory first
                    // increase Magic Theory via practice
                    PracticeHelper practiceHelper = new(Abilities.MagicTheory, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
                    practiceHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
                    // increase Magic Theory via reading
                    ReadingHelper readingHelper = new(Abilities.MagicTheory, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
                    readingHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
                }
                // lab already specialized, see if this lab activity aligns with that specialization
                else if (_mage.Laboratory.Specialization.ArtTopic != null)
                {
                    if (_arts.Technique == _mage.Laboratory.Specialization.ArtTopic)
                    {
                        alreadyConsidered.Add(new SpecializeLabActivity(_arts.Technique, Abilities.MagicTheory, _desireFunc(desire, _conditionDepth)));
                    }
                    else if (_arts.Technique == _mage.Laboratory.Specialization.ArtTopic)
                    {
                        alreadyConsidered.Add(new SpecializeLabActivity(_arts.Form, Abilities.MagicTheory, _desireFunc(desire, _conditionDepth)));
                    }

                }
                else if (_mage.Laboratory.Specialization.ActivityTopic == _activity)
                {
                    alreadyConsidered.Add(new SpecializeLabActivity(_activity, Abilities.MagicTheory, _desireFunc(desire, _conditionDepth)));
                }   
                else if(_mage.Laboratory.Specialization.GetCurrentBonuses().Quality < 0)
                {
                    // specializing will get rid of the negative quality of the current specialization
                    if (_mage.Laboratory.Specialization.ArtTopic != null)
                    {
                        alreadyConsidered.Add(new SpecializeLabActivity(_mage.Laboratory.Specialization.ArtTopic, Abilities.MagicTheory, _desireFunc(desire, _conditionDepth)));
                    }
                    else
                    {
                        alreadyConsidered.Add(new SpecializeLabActivity(_mage.Laboratory.Specialization.ActivityTopic, Abilities.MagicTheory, _desireFunc(desire, _conditionDepth)));
                    }
                }
            }
            else
            {
                // consider all three types of specialization
                alreadyConsidered.Add(new SpecializeLabActivity(_arts.Technique, Abilities.MagicTheory, _desireFunc(desire, _conditionDepth)));
                alreadyConsidered.Add(new SpecializeLabActivity(_arts.Form, Abilities.MagicTheory, _desireFunc(desire, _conditionDepth)));
                alreadyConsidered.Add(new SpecializeLabActivity(_activity, Abilities.MagicTheory, _desireFunc(desire, _conditionDepth)));
            }
        }
    }
}