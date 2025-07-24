using System.Collections.Generic;
using WizardMonks.Economy;
using WizardMonks.Instances;

namespace WizardMonks.Decisions
{
    public class Desires
    {
        public IList<BookDesire> BookDesires { get; private set; }
        public VisDesire[] VisDesires { get; private set; }
        public IList<LabTextDesire> LabTextDesires { get; private set; }

        public Desires()
        {
            BookDesires = [];
            VisDesires = new VisDesire[MagicArts.Count];
            VisDesires[0] = new VisDesire(MagicArts.Creo,0);
            VisDesires[1] = new VisDesire(MagicArts.Intellego, 0);
            VisDesires[2] = new VisDesire(MagicArts.Muto, 0);
            VisDesires[3] = new VisDesire(MagicArts.Perdo, 0);
            VisDesires[4] = new VisDesire(MagicArts.Rego, 0);
            VisDesires[5] = new VisDesire(MagicArts.Animal, 0);
            VisDesires[6] = new VisDesire(MagicArts.Aquam, 0);
            VisDesires[7] = new VisDesire(MagicArts.Auram, 0    );
            VisDesires[8] = new VisDesire(MagicArts.Corpus, 0);
            VisDesires[9] = new VisDesire(MagicArts.Herbam, 0);
            VisDesires[10] = new VisDesire(MagicArts.Ignem, 0);
            VisDesires[11] = new VisDesire(MagicArts.Imaginem, 0);
            VisDesires[12] = new VisDesire(MagicArts.Mentem, 0);
            VisDesires[13] = new VisDesire(MagicArts.Terram, 0);
            VisDesires[14] = new VisDesire(MagicArts.Vim, 0);
            LabTextDesires = [];
        }
    }
}
