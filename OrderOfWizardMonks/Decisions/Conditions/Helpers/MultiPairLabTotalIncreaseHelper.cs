using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    // Like LabTotalIncreaseHelper, but for activities where multiple TeFo combinations
    // all contribute to the same outcome (e.g., breakthrough research, spell-tag improvement).
    // Shared improvements — Magic Theory, lab quality, aura — are added once regardless of
    // how many art pairs are in scope, avoiding artificial inflation of their desire values.
    // Art-specific improvements (vis study, summae) are added once per unique Art, so Re
    // vis study appears once even when Re features in ten different pairs.
    public class MultiPairLabTotalIncreaseHelper : AHelper
    {
        private readonly IReadOnlyList<ArtPair> _artPairs;
        private readonly Activity _activity;

        public MultiPairLabTotalIncreaseHelper(
            HermeticMagus mage,
            uint ageToCompleteBy,
            ushort conditionDepth,
            IReadOnlyList<ArtPair> artPairs,
            Activity activity,
            CalculateDesireFunc desireFunc)
            : base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _artPairs = artPairs;
            _activity = activity;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_ageToCompleteBy <= _mage.SeasonalAge) return;

            var uniqueArts = _artPairs
                .SelectMany(p => new[] { p.Technique, p.Form })
                .Distinct()
                .ToList();

            foreach (var art in uniqueArts)
            {
                AddVisStudyToActionList(art, alreadyConsidered);
                var readingHelper = new ReadingHelper(art, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
                readingHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }

            var mtPracticeHelper = new PracticeHelper(Abilities.MagicTheory, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
            mtPracticeHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);

            var mtReadingHelper = new ReadingHelper(Abilities.MagicTheory, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
            mtReadingHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);

            var bestPair = _artPairs
                .OrderByDescending(p => _mage.GetLabTotal(p, _activity))
                .First();

            var labHelper = new LabImprovementHelper(Abilities.MagicTheory, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), bestPair, _activity, _desireFunc);
            labHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);

            if (_ageToCompleteBy - 1 > _mage.SeasonalAge)
            {
                var auraHelper = new FindNewAuraHelper(_mage, _ageToCompleteBy - 2, (ushort)(_conditionDepth + 2), _desireFunc);
                auraHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }

            if (_ageToCompleteBy - 2 > _mage.SeasonalAge && _mage.Apprentice == null)
            {
                var apprenticeHelper = new FindApprenticeHelper(_mage, _ageToCompleteBy - 3, (ushort)(_conditionDepth + 3), _desireFunc);
                apprenticeHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }
        }

        private void AddVisStudyToActionList(Ability art, ConsideredActions alreadyConsidered)
        {
            CharacterAbilityBase magicArt = _mage.GetAbility(art);
            double stockpile = _mage.GetVisCount(art);
            double visNeed = 0.5 + (magicArt.Value / 10.0);

            if (stockpile > visNeed)
            {
                double gain = magicArt.GetValueGain(_mage.GetVisStudyAuraBonus());
                alreadyConsidered.Add(new StudyVisActivity(magicArt.Ability, _desireFunc(gain, _conditionDepth)));
            }
        }
    }
}
