using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Instances;
using WizardMonks.Models;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class LabImprovementHelper : AHelper
    {
        private readonly ArtPair _arts;
        private readonly Activity _activity; // Can be null if not relevant

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

            // --- Find the single most desirable feature we could install ---
            LabFeature bestFeature = null;
            double bestBonus = 0;

            // Get all candidate features for the required Arts and Activity
            var candidates = new List<LabFeature>();
            if (LabFeatures.FeaturesByArt.TryGetValue(_arts.Technique, out var techFeatures)) candidates.AddRange(techFeatures);
            if (LabFeatures.FeaturesByArt.TryGetValue(_arts.Form, out var formFeatures)) candidates.AddRange(formFeatures);
            if (_activity != null && LabFeatures.FeaturesByActivity.TryGetValue(_activity, out var activityFeatures)) candidates.AddRange(activityFeatures);

            foreach (var featureDef in candidates.Distinct())
            {
                if (_mage.Laboratory.HasFeature(featureDef)) continue;

                double bonus = featureDef.ArtModifiers.Sum(kvp =>
                    (kvp.Key == _arts.Technique || kvp.Key == _arts.Form) ? kvp.Value : 0);

                if (bonus > bestBonus)
                {
                    bestBonus = bonus;
                    bestFeature = featureDef;
                }
            }

            // --- Decide what to do based on the best feature found ---
            if (bestFeature == null)
            {
                // No known features provide the bonus we need.
                return;
            }

            double availableSpace = _mage.Laboratory.GetAvailableSpace();

            if (availableSpace >= bestFeature.Size)
            {
                // We have space! Consider installing it.
                double desire = _desireFunc(bestBonus, (ushort)(_conditionDepth + 1));
                log.Add($"Considering installing '{bestFeature.Name}' for a +{bestBonus} bonus, worth {desire:0.000}");
                alreadyConsidered.Add(new InstallLabFeatureActivity(bestFeature, Abilities.MagicTheory, desire));
            }
            else
            {
                // Not enough space. Your point #2: only consider refinement now.
                log.Add($"Want to install '{bestFeature.Name}', but need {bestFeature.Size - availableSpace} more points of space.");

                if (_mage.GetAbility(Abilities.MagicTheory).Value >= _mage.Laboratory.Refinement + 1)
                {
                    // A season of refinement adds 1 point of space.
                    // The value is the proportional gain towards our goal.
                    double valueOfRefinement = bestBonus / (bestFeature.Size - availableSpace);
                    double desire = _desireFunc(valueOfRefinement, (ushort)(_conditionDepth + 1));
                    log.Add($"Considering refining the lab to create space, worth {desire:0.000}");
                    alreadyConsidered.Add(new RefineLaboratoryActivity(Abilities.MagicTheory, desire));
                }
            }
        }
    }
}