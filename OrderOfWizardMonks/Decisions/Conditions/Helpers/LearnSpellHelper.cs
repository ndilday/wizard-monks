using System;
using System.Collections.Generic;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class LearnSpellHelper : AHelper
    {
        private SpellBase _spellBase;
        public LearnSpellHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, SpellBase spellBase, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _spellBase = spellBase;
        }
        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            if(Mage.Laboratory == null)
            {
                if (AgeToCompleteBy - 1 > Mage.SeasonalAge)
                {
                    HasLabCondition labCondition = 
                        new(Mage, AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1));
                    labCondition.AddActionPreferencesToList(alreadyConsidered, log);
                }
            }
            else if (AgeToCompleteBy > Mage.SeasonalAge)
            {
                double minLevel = 0;
                // see if the mage already knows a spell
                Spell bestSpell = Mage.GetBestSpell(_spellBase);
                if(bestSpell != null)
                {
                    minLevel = bestSpell.Level;
                }
                else
                {
                    minLevel = Mage.GetSpontaneousCastingTotal(_spellBase.ArtPair);
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
                //double newSpellMagnitude = singleSeasonSpellLevel / 5;
                
                // TODO: we're going to have to put a lot of design thought into making this flexible
                if(singleSeasonSpellLevel > minLevel && singleSeasonSpellLevel >= _spellBase.Level)
                {
                    Spell newSpell;
                    switch(singleSeasonSpellLevel)
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
                    double desire = _desireFunc((singleSeasonSpellLevel - minLevel), ConditionDepth);
                    log.Add($"Inventing {newSpell.Name} {newSpell.Level} worth {desire:0.000}");
                    alreadyConsidered.Add(new InventSpell(newSpell, Abilities.MagicTheory, desire));
                }
                // TODO: incorporate lab text library

                // increase Lab Total
                if (AgeToCompleteBy > Mage.SeasonalAge && ConditionDepth < 10)
                {
                    LabTotalIncreaseHelper labTotalIncreaseHelper =
                        new(Mage, AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1), _spellBase.ArtPair, CalculateScoreGainDesire);
                    labTotalIncreaseHelper.AddActionPreferencesToList(alreadyConsidered, log);
                }
            }
        }

        private double CalculateScoreGainDesire(double gain, ushort conditionDepth)
        {
            return _desireFunc(gain / 2, conditionDepth);
        }
    }
}
