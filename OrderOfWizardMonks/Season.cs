using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks
{
	[Serializable]
	public enum SeasonAction
	{
		ReadAbilityBook,
		ReadArtBook,
		InventSpells,
		InventItem,
		LongevityRitual,
		EnchantFamiliar,
		OriginalResearch,
		InitiateOther,
		InitiateSelf,
		Adventure,
		DistillVis,
		StudyVis,
		WriteBook,
		CopyBook,
		BuildLaboratory,
		RefineLaboratory,
		FindApprentice,
		FindVisSource,
		Sundry,
        Practice
	}

	public interface ISeason
	{
		ushort? SeasonId { get; }
        SeasonAction Action { get; }
        List<AbilityValue> AbilityChanges { get;} 
	}

    [Serializable]
    public class ReadingSeason : ISeason
    {
        public ushort? SeasonId { get; private set; }
        public SeasonAction Action { get; private set; }
        public List<AbilityValue> AbilityChanges { get; private set; }
    }

    [Serializable]
    public class SundrySeason: ISeason
    {
        public ushort? SeasonId { get; private set; }
        public SeasonAction Action
        {
            get { return SeasonAction.Sundry; }
        }
        public List<AbilityValue> AbilityChanges { get; private set; }
    }
}
