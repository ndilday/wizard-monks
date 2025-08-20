using System;
using WizardMonks.Models.Characters;


namespace WizardMonks.Models.Research
{
    public class BreakthroughDefinition
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ushort BreakthroughPointsRequired { get; private set; }

        // The delegate defines what this breakthrough actually *does* to a HermeticTheory object.
        public Action<HermeticTheory> ApplyEffect { get; private set; }

        public BreakthroughDefinition(string name, string desc, ushort points, Action<HermeticTheory> effect)
        {
            Name = name;
            Description = desc;
            BreakthroughPointsRequired = points;
            ApplyEffect = effect;
        }
    }
}
