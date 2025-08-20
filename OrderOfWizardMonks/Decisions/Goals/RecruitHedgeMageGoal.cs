// In WizardMonks/Decisions/Goals/ (New File)
using System.Collections.Generic;
using WizardMonks.Decisions;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

public class RecruitHedgeMageGoal : AGoal
{
    public Magus Master { get; private set; }

    public RecruitHedgeMageGoal(Magus recruiter, Magus master, double desire)
        : base(recruiter, null, desire)
    {
        Master = master;
        // The desire is intrinsic to Trianoma's character.
        Desire *= recruiter.Personality.GetDesireMultiplier(HexacoFacet.Fairness) * recruiter.Personality.GetDesireMultiplier(HexacoFacet.Diligence);
    }

    // This goal is "complete" for now once one wizard is found, 
    // but the GoalGenerator could create a new one later.
    public override bool IsComplete() => _completed;

    public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
    {
        if (IsComplete()) return;

        // This goal's only action is to search.
        // We can reuse the FindApprenticeHelper's logic but direct it to a new activity.
        log.Add($"[Goal] Recruiting a hedge wizard for {Master.Name}. Desire: {Desire:F2}");
        var findActivity = new FindHedgeMageActivity(Master, Abilities.AreaLore, Desire);
        alreadyConsidered.Add(findActivity);
    }
}