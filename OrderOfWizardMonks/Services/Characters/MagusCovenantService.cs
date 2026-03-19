using WizardMonks.Models;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Covenants;

namespace WizardMonks.Services.Characters
{
    public static class MagusCovenantService
    {
        public static void JoinCovenant(this HermeticMagus mage, Covenant covenant, CovenantRole role = CovenantRole.Visitor)
        {
            if (mage.Covenant != null)
            {
                mage.Covenant.RemoveMagus(mage);
            }
            mage.Covenant = covenant;
            covenant.AddMagus(mage);
        }

        public static void LeaveCovenant(this HermeticMagus mage)
        {
            if (mage.Covenant != null)
            {
                mage.Covenant.RemoveMagus(mage);
                mage.Covenant = null;
            }

        }

        public static Covenant FoundCovenant(this HermeticMagus mage, Aura aura)
        {
            Covenant coventant = new(aura);
            mage.JoinCovenant(coventant, CovenantRole.Founder);
            var auraBeliefs = mage.GetBeliefProfile(aura);
            // the mage now believes the aura is owned by the covenant
            auraBeliefs.AddOrUpdateBelief(new(BeliefTopics.Owner, coventant.Id.GetHashCode()));
            return coventant;
        }
    }
}
