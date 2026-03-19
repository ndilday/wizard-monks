using System;
using System.Linq;
using WizardMonks.Core;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities
{
    [Serializable]
    public enum Activity
    {
        ReadBook,
        InventSpells,
        InventItem,
        WriteLabText,
        CopyLabText,
        LongevityRitual,
        EnchantFamiliar,
        OriginalResearch,
        InitiateOther,
        InitiateSelf,
        Adventure,
        DistillVis,
        StudyVis,
        Teach,
        Train,
        TranslateLabText,
        Learn,
        WriteBook,
        CopyBook,
        BuildLaboratory,
        RefineLaboratory,
        InstallLaboratoryFeature,
        FindApprentice,
        FindHedgeMage,
        RecruitHedgeMage,
        GauntletApprentice,
        FindAura,
        FindVisSource,
        Sundry,
        Practice,
        /// <summary>
        /// Opening the Arts: the season-long ritual in which a master opens an
        /// apprentice's Gift to a magical tradition. The master's InVi Lab Total
        /// (for Hermetic magi) is used as the Opening Total. This activity
        /// occupies both master and apprentice for the full season.
        /// </summary>
        OpenArts
    }

    public interface IActivity
    {
        Activity Action { get; }
        double Desire { get; set; }
        void Act(Character character);
        bool Matches(IActivity action);
        string Log();
    }
}