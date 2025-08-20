using WizardMonks.Activities;
using WizardMonks.Activities.ExposingActivities;
using WizardMonks.Core;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Covenants;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

public class FindHedgeMageActivity : AExposingActivity
{
    private const double EASE_FACTOR = 9.0;

    public Magus TargetMaster { get; private set; }

    public FindHedgeMageActivity(Magus targetMaster, Ability exposure, double desire) : base(exposure, desire)
    {
        TargetMaster = targetMaster;
        Action = Activity.FindHedgeMage; // New Activity enum value needed
    }

    protected override void DoAction(Character character)
    {
        var recruiter = (Magus)character;
        double searchTotal = CalculateSearchTotal(recruiter);

        // Step 3: Make the Roll and Determine Outcome
        double roll = Die.Instance.RollStressDie(0, out _); // Botch has no special effect for now.

        if (roll + searchTotal > EASE_FACTOR)
        {

            // ON SUCCESS:
            // 1. for now, just pick one of the unmet founders
            var hedgeWizard = CharacterFactory.GenerateNewHedgeMage();
            var profile = new BeliefProfile(SubjectType.Character, 1.0);
            profile.AddOrUpdateBelief(new(BeliefTopics.HedgeMage, 1.0));
        }
        else
        {
            recruiter.Log.Add("Met no hedge magi this season.");
            return;
        }
    }

    private static double CalculateSearchTotal(Magus recruiter)
    {
        // Step 1: Calculate the Search Total
        double searchTotal = 0;
        searchTotal += recruiter.GetAbility(Abilities.FolkKen).Value;
        searchTotal += recruiter.GetAttributeValue(AttributeType.Perception);
        // Area Lore and Etiquette provide a smaller, supporting bonus.
        searchTotal += recruiter.GetAbility(Abilities.AreaLore).Value / 2.0;
        searchTotal += recruiter.GetAbility(Abilities.Etiquette).Value / 2.0;

        // Step 2: Add Magic Bonus
        SpellBase giftFindingBase = SpellBases.GetSpellBaseForEffect(TechniqueEffects.Detect, FormEffects.Gift);
        Spell bestGiftFindingSpell = recruiter.GetBestSpell(giftFindingBase);
        double giftFindingBonus = 0;

        if (bestGiftFindingSpell != null)
        {
            giftFindingBonus = (bestGiftFindingSpell.Level / 5.0) - 5;
            recruiter.Log.Add($"Using '{bestGiftFindingSpell.Name}' to aid the search.");
        }
        else
        {
            // If no spell is known, use spontaneous magic potential.
            giftFindingBonus = (recruiter.GetSpontaneousCastingTotal(MagicArtPairs.InVi) / 5.0) - 5;
        }
        searchTotal += giftFindingBonus;
        return searchTotal;
    }

    public override bool Matches(IActivity action)
    {
        return action.Action == Activity.FindHedgeMage;
    }

    public override string Log()
    {
        return "Searching for Hedge Magi worth " + Desire.ToString("0.000");
    }
}