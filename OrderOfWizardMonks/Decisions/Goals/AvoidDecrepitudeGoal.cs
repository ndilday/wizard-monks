// In WizardMonks/Decisions/Goals/
using System;
using System.Collections.Generic;
using WizardMonks;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Decisions;
using WizardMonks.Decisions.Conditions;
using WizardMonks.Decisions.Conditions.Helpers;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;

public class AvoidDecrepitudeGoal : AGoal
{
    private Magus _mage;
    private const ushort FIRST_AGING_SEASON = 140; // Age 35
    private readonly List<Ability> visTypes = [MagicArts.Creo, MagicArts.Vim];

    public AvoidDecrepitudeGoal(Magus magus, double desire)
        : base(magus, null, desire) // Due date is dynamic, so we set it to null here.
    {
        _mage = magus;
    }

    // The core logic will reside in the AddActionPreferencesToList method.
    public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
    {
        // Step 1: Determine the deadline for the next aging roll.
        uint deadlineSeason;
        if (_mage.LongevityRitual == 0)
        {
            deadlineSeason = FIRST_AGING_SEASON;
        }
        else
        {
            // since the aging math is a stress die plus age/10 minus longevity ritual strength, let's aim for protecting from the 1 standard deviation up case, which is a result of ~11
            // but we only worry about results of 10+, so longevity ritual should aim for a strength of 2 + age/40 (where age is measured in seasons)
            // solving for age leads to the below equation
            deadlineSeason = (ushort)(40 * _mage.LongevityRitual - 80);
        }

        // If the deadline has passed, the goal has failed for this cycle. The magus must face an aging roll.
        if (_mage.SeasonalAge >= deadlineSeason)
        {
            log.Add("Missed the window for a new Longevity Ritual; an aging roll is imminent.");
            deadlineSeason = _mage.SeasonalAge + 1;
        }

        // Step 2: Calculate the best ritual the magus could perform this season.
        double potentialStrength = Math.Floor(_mage.GetLabTotal(MagicArtPairs.CrVi, Activity.LongevityRitual) / 5);

        SetupConditions(deadlineSeason);
        base.AddActionPreferencesToList(alreadyConsidered, desires, log);

        // if the mage has a lab, has the vis necessary, can improve their longevity ritual, and is near or at the deadline, invent the longevity ritual
        if(base.IsComplete() && potentialStrength > _mage.LongevityRitual && _mage.SeasonalAge >= deadlineSeason - 1)
        {
            alreadyConsidered.Add(new InventLongevityRitualActivity(Abilities.MagicTheory, this.Desire));
        }
        else
        {
            // if we're not inventing the longevity ritual now, we should consider improving the lab total for a better ritual later
            ConsiderIncreasingLabTotal(alreadyConsidered, desires, log, deadlineSeason);
        }
    }

    private void SetupConditions(uint deadlineAge)
    {
        this.Conditions.Clear();
        double visNeed = _mage.SeasonalAge / 20;
        this.Conditions.Add(new HasLabCondition(_mage, deadlineAge, this.Desire));
        this.Conditions.Add(new VisCondition(_mage, deadlineAge, this.Desire, visTypes, visNeed, 1));
    }

    private void ConsiderIncreasingLabTotal(ConsideredActions alreadyConsidered, Desires desires, IList<string> log, uint deadlineSeason)
    {
        // The value of increasing the lab total is the number of extra seasons of life it will provide
        // effectively, each five points of lab total increases the longevity ritual by 1 point
        // which is equal to a decade of life
        // so, five points of lab total equals 40 seasons, meaning each point of lab total is worth 8 seasons
        // there's some loss due to the additional vis cost as one ages, and the warping, so let's call it 7 seasons
        double distillRate = _mage.GetVisDistillationRate();
        CalculateDesireFunc desireFunc = (gain, depth) =>
        {
            double seasonsRemaining = deadlineSeason - _mage.SeasonalAge;
            if (seasonsRemaining <= 0) seasonsRemaining = 1;
            // A gain of 1 in lab total is worth 1 season of life.
            return (this.Desire / seasonsRemaining) * gain * distillRate * 7;
        };

        var labTotalHelper = new LabTotalIncreaseHelper(
            _mage,
            deadlineSeason - 1, // Must improve *before* the deadline
            1,
            MagicArtPairs.CrVi,
            Activity.LongevityRitual,
            desireFunc
        );
        labTotalHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
    }

    // This goal is never truly "complete" in the traditional sense.
    // It's only quiescent when the next aging roll is safely in the future.
    public override bool IsComplete()
    {
        return false;
    }
}