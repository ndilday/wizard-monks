using System;
using System.Linq;
using WizardMonks.Core;
using WizardMonks.Instances;

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
        GauntletApprentice,
        FindAura,
		FindVisSource,
		Sundry,
        Practice
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
