using System;
using System.Collections.Generic;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class LearnSpellHelper : AHelper
    {
        private SpellBase _spellBase;
        public LearnSpellHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, 
                                ushort conditionDepth, SpellBase spellBase, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth, desireFunc)
        {
            _spellBase = spellBase;
        }
        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            if(Mage.Laboratory == null)
            {
                if (AgeToCompleteBy - Mage.SeasonalAge > 1)
                {
                    HasLabCondition labCondition = 
                        new(Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1));
                    labCondition.AddActionPreferencesToList(alreadyConsidered, log);
                }
            }
            else if (AgeToCompleteBy - Mage.SeasonalAge > 0)
            {
                double minMagnitude = 0;
                // see if the mage already knows a spell
                Spell bestSpell = Mage.GetBestSpell(_spellBase);
                if(bestSpell != null)
                {
                    minMagnitude = bestSpell.Level / 5;
                }
                else
                {
                    minMagnitude = Mage.GetSpontaneousCastingTotal(_spellBase.ArtPair) / 5;
                }

                double labTotal = Mage.GetLabTotal(_spellBase.ArtPair, Activity.InventSpells);
                double singleSeasonSpellLevel = labTotal / 2.0;
                if (singleSeasonSpellLevel > 5)
                {
                    // round off to a multiple of 5
                    singleSeasonSpellLevel = Math.Floor(singleSeasonSpellLevel / 5) * 5;
                }
                else
                {
                    singleSeasonSpellLevel = Math.Floor(singleSeasonSpellLevel);
                }
                double newSpellMagnitude = singleSeasonSpellLevel / 5;
                
                // TODO: we're going to have to put a lot of design thought into making this flexible
                if(newSpellMagnitude > minMagnitude && newSpellMagnitude >= _spellBase.Level)
                {
                    Spell newSpell;
                    switch(newSpellMagnitude)
                    {
                        case 1:
                            newSpell =
                                new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Taste, _spellBase, 0, false, _spellBase.Name);
                            break;
                        case 2:
                            newSpell =
                                new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Touch, _spellBase, 0, false, _spellBase.Name);
                            break;
                        case 3:
                            newSpell =
                                new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Smell, _spellBase, 0, false, _spellBase.Name);
                            break;
                        case 4:
                            newSpell =
                                new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Hearing, _spellBase, 0, false, _spellBase.Name);
                            break;
                        case 5:
                            newSpell =
                                new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Sight, _spellBase, 0, false, _spellBase.Name);
                            break;
                        case 10:
                            newSpell =
                                new Spell(EffectRanges.Personal, EffectDurations.Diameter, EffectTargets.Sight, _spellBase, 0, false, _spellBase.Name);
                            break;
                        case 15:
                            newSpell =
                                new Spell(EffectRanges.Personal, EffectDurations.Sun, EffectTargets.Sight, _spellBase, 0, false, _spellBase.Name);
                            break;
                        default:
                            newSpell =
                                new Spell(EffectRanges.Personal, EffectDurations.Moon, EffectTargets.Sight, _spellBase, 0, false, _spellBase.Name);
                            break;
                    }
                    double desire = _desireFunc(newSpellMagnitude - minMagnitude, ConditionDepth);
                    if (desire > 0.00001)
                    {
                        log.Add("Inventing a spell worth " + desire.ToString("0.000"));
                        alreadyConsidered.Add(new InventSpell(newSpell, Abilities.MagicTheory, desire));
                    }
                }
                // TODO: incorporate lab text library

                // increase Lab Total
                LabTotalIncreaseHelper labTotalIncreaseHelper =
                    new(Mage, AgeToCompleteBy - 1, Desire / 2, (ushort)(ConditionDepth + 1), _spellBase.ArtPair, _desireFunc);
                labTotalIncreaseHelper.AddActionPreferencesToList(alreadyConsidered, log);
            }
        }
    }
}
