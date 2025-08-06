using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Models;

namespace WizardMonks.Economy
{
    public class LabTextForTrade
    {
        public LabText LabText { get; private set; }
        public double MinimumPrice { get; private set; }
        public LabTextForTrade(LabText labText, double minPrice)
        {
            LabText = labText;
            MinimumPrice = minPrice;
        }
    }
}
