using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Models;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Covenants;
using static System.Net.Mime.MediaTypeNames;

namespace WizardMonks.Services.Characters
{
    public static class MagusCovenantService
    {
        public static void JoinCovenant(this Magus mage, Covenant covenant, CovenantRole role = CovenantRole.Visitor)
        {
            if (mage.Covenant != null)
            {
                mage.Covenant.RemoveMagus(mage);
            }
            mage.Covenant = covenant;
            covenant.AddMagus(mage);
            mage.VisStudyRate = 6.75 + covenant.Aura.Strength;
        }

        public static void LeaveCovenant(this Magus mage)
        {
            if (mage.Covenant != null)
            {
                // basically, need to leave the covenant and forget all known Auras (until we come up with a notion of Aura ownership)
                mage.Covenant.RemoveMagus(mage);
                mage.Covenant = null;
                mage.KnownAuras.Clear();
            }

        }

        public static Covenant FoundCovenant(this Magus mage, Aura aura)
        {
            Covenant coventant = new(aura);
            mage.JoinCovenant(coventant, CovenantRole.Founder);
            return coventant;
        }
    }
}
