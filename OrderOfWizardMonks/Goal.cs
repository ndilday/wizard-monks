using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks
{
    public partial class Character
    {
        protected List<IGoal> _goals;
        IAction DecideSeasonalActivity()
        {
            ConsideredActions actions = new ConsideredActions();
            foreach (IGoal goal in _goals)
            {
                if (!goal.IsComplete(this))
                {
                    // TODO: it should probably be an error case for a goal to still be here
                    // for now, ignore
                    //List<string> dummy = new List<string>();
                    goal.ModifyActionList(this, actions, Log);
                }
            }
            Log.AddRange(actions.Log());
            return actions.GetBestAction();
        }

        public virtual void ReprioritizeGoals()
        {
            foreach (IGoal goal in _goals)
            {
                if (!goal.IsComplete(this))
                {
                    if (!goal.DecrementDueDate())
                    {
                        Log.Add("Failed to achieve a goal");
                        _goals.Remove(goal);
                    }
                }
            }
        }
    }

    public class ConsideredActions
    {
        Dictionary<Activity, IList<IAction>> ActionTypeMap = new Dictionary<Activity,IList<IAction>>();
        
        public void Add(IAction action)
        {
            if (!ActionTypeMap.ContainsKey(action.Action))
            {
                ActionTypeMap[action.Action] = new List<IAction>();
                ActionTypeMap[action.Action].Add(action);
            }
            else
            {
                var match = ActionTypeMap[action.Action].Where(a => a.Matches(action)).FirstOrDefault();
                if (match != null)
                {
                    match.Desire += action.Desire;
                }
                else
                {
                    ActionTypeMap[action.Action].Add(action);
                }
            }
        }

        public IList<string> Log()
        {
            List<string> log = new List<string>();
            log.Add("----------");
            log.AddRange(ActionTypeMap.SelectMany(a => a.Value).Select(a => a.Log()));
            log.Add("----------");
            return log;
        }
        
        public IAction GetBestAction()
        {
            return ActionTypeMap.SelectMany(a => a.Value).OrderByDescending(a => a.Desire).FirstOrDefault();
        }
    }

    public interface IGoal
    {
        void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log);
        bool IsComplete(Character character);
        uint? DueDate { get; }
        double Desire { get; }
        bool DecrementDueDate();
    }

    #region Basic (1-tier) Goals
    public abstract class BaseGoal : IGoal
    {
        public uint? DueDate {get; private set;}
        public double Desire {get; private set;}
        public virtual bool DecrementDueDate()
        {
            if(DueDate != null)
            {
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }
        public abstract bool IsComplete(Character character);
        public abstract void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log);
        public BaseGoal(double desire, uint? dueDate = null)
        {
            DueDate = dueDate;
            Desire = desire;
        }
    }

    class AbilityScoreCondition : BaseGoal
    {
        #region Protected fields
        protected List<Ability> _abilities;
        protected double _total;
        #endregion

        public AbilityScoreCondition(List<Ability> abilities, double total, double desire, uint? dueDate = null) : base(desire, dueDate)
        {
            _abilities = abilities;
            _total = total;
        }

        public AbilityScoreCondition(Ability ability, double total, double desire, uint? dueDate = null) : base(desire, dueDate)
        {
            _abilities = new List<Ability>();
            _abilities.Add(ability);
            _total = total;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double remainingTotal = GetRemainingTotal(character);
            double dueDateDesire = Desire;
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Ability Condition failed!");
                    return;
                }
                dueDateDesire /= (double)DueDate;
            }
            IEnumerable<IBook> readableBooks = character.ReadableBooks;
            foreach (Ability ability in _abilities)
            {
                // Handle Reading
                CharacterAbilityBase charAbility = character.GetAbility(ability);
                var topicalBooks = readableBooks.Where(b => b.Topic == ability);
                AddReading(character, alreadyConsidered, topicalBooks, remainingTotal, dueDateDesire);

                // Handle Practice
                // For now, assume 4pt practice on everything
                double desire = dueDateDesire * charAbility.GetValueGain(4) / remainingTotal;
                log.Add("Practicing " + ability.AbilityName + " worth " + desire.ToString("0.00"));
                Practice practiceAction = new Practice(ability, desire);
                alreadyConsidered.Add(practiceAction);

                if (character.GetType() == typeof(Magus))
                {
                    Magus mage = (Magus)character;
                    if (MagicArts.IsArt(charAbility.Ability))
                    {
                        HandleVisUse(mage, charAbility, remainingTotal, dueDateDesire, alreadyConsidered, log);
                    }
                }

                // TODO: Learning By Training
                // TODO: Learning by Teaching
            }
        }

        private void HandleVisUse(Magus mage, CharacterAbilityBase charAbility, double remainingTotal, double desire, ConsideredActions alreadyConsidered, IList<string> log )
        {
            // see if the mage has enough vis of this type
            double stockpile = mage.GetVisCount(charAbility.Ability);
            double visNeed = 0.5 + (charAbility.Value / 10.0);
            double baseDesire = desire * charAbility.GetValueGain(6) / remainingTotal;
            // if so, assume vis will return an average of 6XP
            if (stockpile > visNeed)
            {
                log.Add("Studying vis for " + charAbility.Ability.AbilityName + " worth " + baseDesire.ToString("0.00"));
                VisStudying visStudy = new VisStudying(charAbility.Ability, baseDesire);
                alreadyConsidered.Add(visStudy);
                // TODO: how do we decrement the cost of the vis?
            }
            else if(baseDesire > 0.1 && (DueDate == null || DueDate > 1))
            {
                // only try to extract the vis now if there's sufficient time to do so
                List<Ability> visType = new List<Ability>();
                visType.Add(charAbility.Ability);
                VisCondition visCondition = new VisCondition(visType, visNeed - stockpile, baseDesire / 2, DueDate == null ? null : DueDate - 1);
                visCondition.ModifyActionList(mage, alreadyConsidered, log);
            }
            
                // TODO: consider the value of looking for another aura
                // TODO: consider the value of looking for vis sites
        }

        public override bool IsComplete(Character character)
        {
            return GetRemainingTotal(character) <= 0;
        }

        protected virtual double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in _abilities)
            {
                currentTotal += character.GetAbility(ability).Value;
            }
            return _total - currentTotal;
        }

        private void AddReading(Character character, ConsideredActions alreadyConsidered, IEnumerable<IBook> topicalBooks, double remainingTotal, double desire)
        {
            if (topicalBooks.Any())
            {
                var bestBook =
                    (from book in topicalBooks
                     orderby character.GetBookLevelGain(book),
                             book.Level ascending
                     select new EvaluatedBook
                     {
                         Book = book,
                         PerceivedValue = character.GetBookLevelGain(book) * desire / remainingTotal
                     }).First();
                // check to see if reading this book is already in the action list
                Reading readingAction = new Reading(bestBook.Book, bestBook.PerceivedValue);
                alreadyConsidered.Add(readingAction);
            }
        }
    }

    class CharacteristicAbilityScoreCondition : AbilityScoreCondition
    {
        protected List<AttributeType> _attributes;

        public CharacteristicAbilityScoreCondition(List<Ability> abilities, List<AttributeType> attributes, double total, double desire, uint? dueDate = null) :
            base(abilities, total, desire, dueDate)
        {
            _attributes = attributes;
        }

        protected override double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in _abilities)
            {
                currentTotal += character.GetAbility(ability).Value;
            }
            foreach(AttributeType attribute in _attributes)
            {
                currentTotal += character.GetAttribute(attribute).Value;
            }
            return _total - currentTotal;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            base.ModifyActionList(character, alreadyConsidered, log);
            // TODO: consider CrVi spells that increase statistics
        }
    }
    #endregion

    #region Complex Goals
    class LabScoreGoal : CharacteristicAbilityScoreCondition
    {
        private HasLabCondition _hasLabCondition;

        public LabScoreGoal(List<Ability> abilities, List<AttributeType> attributes, double total, double desire, uint? dueDate = null)
            : base(abilities, attributes, total, desire, dueDate)
        {
            _hasLabCondition = new HasLabCondition(desire, dueDate);
        }

        public override bool DecrementDueDate()
        {
            if (DueDate != null )
            {
                if (!_hasLabCondition.DecrementDueDate()) return false;
                return base.DecrementDueDate();
            }
            return true;
        }

        protected override double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in _abilities)
            {
                currentTotal += character.GetAbility(ability).Value;
            }
            foreach (AttributeType attribute in _attributes)
            {
                currentTotal += character.GetAttribute(attribute).Value;
            }
            if(character.GetType() == typeof(Magus))
            {
                Magus mage = (Magus)character;
                currentTotal += mage.Laboratory.Quality;
                foreach (Ability ability in _abilities)
                {
                    if (mage.Laboratory.ArtModifiers.ContainsKey(ability))
                    {
                        currentTotal += mage.Laboratory.ArtModifiers[ability];
                    }
                }
                // TODO: we probably need to take activity type into account, 
                // or that needs to be taken into account in setting the total of this object
            }
            return _total - currentTotal;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (!_hasLabCondition.IsComplete(character))
            {
                _hasLabCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            else
            {
                base.ModifyActionList(character, alreadyConsidered, log);

                // TODO: consider aura search
                // TODO: consider laboratory refinements
                // TODO: consider taking an apprentice
                // TODO: consider finding a familiar
            }
        }

        public override bool IsComplete(Character character)
        {
            return _hasLabCondition.IsComplete(character) && GetRemainingTotal(character) <= 0;
        }
    }

    // TODO: consider a base class for goals with pre-reqs

    class HasCovenantCondition : IGoal
    {
        private AbilityScoreCondition _minScore;

        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if (!_minScore.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public HasCovenantCondition(double value, uint? dueDate = null)
        {
            Desire = value;
            DueDate = dueDate;
            List<Ability> abilities = new List<Ability>();
            abilities.Add(Abilities.AreaLore);
            abilities.Add(MagicArts.Intellego);
            abilities.Add(MagicArts.Vim);
            _minScore = new AbilityScoreCondition(abilities, 2, value, dueDate == null ? null : dueDate - 1);
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double desire = Desire;
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Has Covenant Condition failed");
                    return;
                }
                desire /= (double)DueDate;
            }

            if (!_minScore.IsComplete(character))
            {
                _minScore.ModifyActionList(character, alreadyConsidered, log);
            }
            else
            {
                log.Add("Looking for an aura worth " + (desire).ToString("0.00"));
                alreadyConsidered.Add(new FindAura(Abilities.AreaLore, desire));
                // consider the incremental improvement of increasing skills
                if (desire > 0.1)
                {
                    double artTotal = character.GetAbility(MagicArts.Intellego).Value + character.GetAbility(MagicArts.Vim).Value;
                    List<Ability> artHelper = new List<Ability>();
                    artHelper.Add(MagicArts.Intellego);
                    artHelper.Add(MagicArts.Vim);
                    IncreaseAbilitiesHelper helper = new IncreaseAbilitiesHelper(artHelper, Desire / 10, artTotal, DueDate == null ? null : DueDate - 1);
                    helper.ModifyActionList(character, alreadyConsidered, log);
                    List<Ability> al = new List<Ability>();
                    al.Add(Abilities.AreaLore);
                    IncreaseAbilitiesHelper helper2 =
                        new IncreaseAbilitiesHelper(al, Desire / 2, character.GetAbility(Abilities.AreaLore).Value, DueDate == null ? null : DueDate - 1);
                }
            }
        }

        public bool IsComplete(Character character)
        {
            return ((Magus)character).Covenant != null;
        }
    }

    class HasLabCondition : IGoal
    {
        private HasCovenantCondition _hasCovenant;
        private AbilityScoreCondition _mtCondition;

        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if(!_hasCovenant.DecrementDueDate()) return false;
                if(!_mtCondition.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public HasLabCondition(double value, uint? dueDate = null)
        {
            Desire = value;
            DueDate = dueDate;
            _hasCovenant = new HasCovenantCondition(value, dueDate == null ? null : dueDate - 2);
            _mtCondition = new AbilityScoreCondition(Abilities.MagicTheory, 3, value, dueDate == null ? null : dueDate - 1);
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double desire = Desire;
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Not enough time to build lab");
                    return;
                }
                desire /= (double)DueDate;
            }
            bool hasCovenant = _hasCovenant.IsComplete(character);
            bool hasMT = _mtCondition.IsComplete(character);
            if (!hasCovenant)
            {
                _hasCovenant.ModifyActionList(character, alreadyConsidered, log);
            }
            if (!hasMT)
            {
                _mtCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            if(hasCovenant && hasMT)
            {
                log.Add("Setting up a lab worth " + (desire).ToString("0.00"));
                alreadyConsidered.Add(new BuildLaboratory(Abilities.MagicTheory, desire));
            }
        }

        public bool IsComplete(Character character)
        {
            return ((Magus)character).Laboratory != null;
        }
    }

    class VisCondition : IGoal
    {
        HasLabCondition _hasLab;

        List<Ability> _visTypes;
        List<Ability> _extractArts;
        double _total;
        bool _isVimSufficient;

        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public VisCondition(List<Ability> visTypes, double total, double desire, uint? dueDate = null)
        {
            DueDate = dueDate;
            Desire = desire;

            _extractArts = new List<Ability>();
            _extractArts.Add(MagicArts.Creo);
            _extractArts.Add(MagicArts.Vim);
            _extractArts.Add(Abilities.MagicTheory);
            _visTypes = visTypes;
            _total = total;
            _isVimSufficient = visTypes.Where(v => v == MagicArts.Vim).Any();

            _hasLab = new HasLabCondition(desire, dueDate == null || dueDate < 2 ? null : dueDate - 1);
        }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if(!_hasLab.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double dueDateDesire = Desire;
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Vis Goal Failed");
                    return;
                }
                dueDateDesire /= (double)DueDate;
            }
            Magus mage = (Magus)character;
            bool hasLab = _hasLab.IsComplete(character);

            if (_isVimSufficient)
            {
                // determine value of extracting vis from aura

                if (!hasLab)
                {
                    // if we don't have a lab, we need a lab
                    _hasLab.ModifyActionList(character, alreadyConsidered, log);
                }
                else
                {
                    // Now we need to consider both the value of starting to distill vis now
                    // and the value of instead learning more before doing so.
                    double labTotal = mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis);
                    double currentRate = labTotal / 10;
                    if (currentRate >= _total)
                    {
                        // we can get what we want in one season, go ahead and do it
                        log.Add("Extracting vis worth " + (dueDateDesire).ToString("0.00"));
                        alreadyConsidered.Add(new VisExtracting(Abilities.MagicTheory, dueDateDesire));
                    }
                    else
                    {
                        // we are in the multi-season-to-fulfill scenario
                        double desire = currentRate * dueDateDesire / _total;
                        log.Add("Extracting vis worth " + (desire).ToString("0.00"));
                        alreadyConsidered.Add(new VisExtracting(Abilities.MagicTheory, desire));

                        // the difference between the desire of starting now
                        // and the desire of starting after gaining experience
                        // is the effective value of practicing here
                        IncreaseAbilitiesHelper helper = new IncreaseAbilitiesHelper(_extractArts, Desire, labTotal, DueDate - 1);
                        helper.ModifyActionList(character, alreadyConsidered, log);
                    }
                }
            }
            // determine current rate of automatic accrual of vis of the desired type
            double annualGain = 
                mage.KnownAuras.SelectMany( a => a.VisSources).Where(v => _visTypes.Contains(v.Art)).Select(v => v.AnnualAmount).Sum();
            if (annualGain == 0 || (DueDate != null && (annualGain * DueDate / 4) < _total))
            {
                // we're not getting vis fast enough, so we need to find a new source
                // consider the value of searching for new vis sites in current auras
                // determine average vis source found
                double magicLore = mage.GetAbility(Abilities.MagicLore).Value;
                magicLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 5;
                if (magicLore > 0)
                {
                    foreach (Aura aura in mage.KnownAuras)
                    {
                        double visSourceFound = Math.Sqrt(2.5 * magicLore * aura.Strength);
                        visSourceFound -= aura.VisSources.Select(v => v.AnnualAmount).Sum();

                        // modify by chance vis will be of the proper type
                        visSourceFound = visSourceFound * _visTypes.Count() / 15;

                        // TODO: modify by lifelong value of source?
                        log.Add("Looking for vis source worth " + (visSourceFound * dueDateDesire).ToString("0.00"));
                        alreadyConsidered.Add(new FindVisSource(aura, Abilities.MagicLore, visSourceFound * dueDateDesire));

                        // TODO: consider the value of getting better at the vis search skills first?
                    }
                }
                // consider the value of looking for a new aura
                double areaLore = mage.GetAbility(Abilities.AreaLore).Value;
                areaLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 10;

                double auraFound = Math.Sqrt(2.5 * areaLore / (mage.KnownAuras.Count() + 1));
                double visFromAura = Math.Sqrt(2.5 * magicLore * auraFound) * _visTypes.Count() / 15;
                dueDateDesire = dueDateDesire * visFromAura / 2;
                log.Add("Looking for aura (to find a vis source in) worth " + (dueDateDesire).ToString("0.00"));
                alreadyConsidered.Add(new FindAura(Abilities.AreaLore, dueDateDesire));
            }
        }

        public bool IsComplete(Character character)
        {
            if (character.GetType() != typeof(Magus))
            {
                throw new ArgumentException("Only magi can have Vis conditions");
            }
            Magus mage = (Magus)character;
            double total = 0;
            foreach (Ability visType in _visTypes)
            {
                total += mage.GetVisCount(visType);
            }
            return total >= _total;
        }
    }

    class InventSpellGoal : IGoal
    {
        private LabScoreGoal _labScore;
        private List<Ability> _abilitiesRequired;
        private Spell _spell;

        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if(!_labScore.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public InventSpellGoal(Spell spell, double desire, uint? dueDate = null)
        {
            DueDate = dueDate;
            Desire = desire;
            _spell = spell;
            _abilitiesRequired = new List<Ability>();
            _abilitiesRequired.Add(spell.BaseArts.Technique);
            _abilitiesRequired.Add(spell.BaseArts.Form);
            _abilitiesRequired.Add(Abilities.MagicTheory);
            List<AttributeType> attributes = new List<AttributeType>();
            attributes.Add(AttributeType.Intelligence);

            // we need at least a lab score above the level of the spell to think about inventing it
            uint? labScoreDueDate = null;
            if (dueDate != null && dueDate > spell.Level)
            {
                labScoreDueDate = dueDate - (uint)spell.Level;
            }
            else
            {
                // TODO: throw error?
            }
            _labScore = new LabScoreGoal(_abilitiesRequired, attributes, spell.Level, desire, labScoreDueDate);
        }
 
        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double desire = Desire;
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Not enough time to invent spell");
                    return;
                }
                desire /= (double)DueDate;
            }

            if (!_labScore.IsComplete(character))
            {
                _labScore.ModifyActionList(character, alreadyConsidered, log);
            }
            else
            {
                double level = _spell.Level;
                // Now we need to consider both the value of starting to invent the spell now
                // and the value of instead learning more before doing so.
                // As a first pass, value the portion of the spell inventable this season
                Magus mage = (Magus)character;
                double extraTotal = mage.GetLabTotal(_spell.BaseArts, Activity.InventSpells) - level;
                if (extraTotal <= 0)
                {
                    // This should not happen
                    _labScore.ModifyActionList(character, alreadyConsidered, log);
                }
                if (extraTotal >= level)
                {
                    // there's no reason to consider adding to abilities for this spell
                    // TODO: eventually, we should consider the fact that the extra learning
                    // could allow one to learn more spells in a season
                    log.Add("Inventing this spell worth " + desire.ToString("0.00"));
                    alreadyConsidered.Add(new InventSpell(_spell, Abilities.MagicTheory, desire));
                }
                else
                {
                    // we are in the multi-season-to-invent scenario
                    desire = extraTotal * desire / level;
                    alreadyConsidered.Add(new InventSpell(_spell, Abilities.MagicTheory, desire));

                    // the difference between the desire of starting now
                    // and the desire of starting after practice
                    // is the effective value of practicing here
                    IncreaseAbilitiesVersusLevelHelper helper = new IncreaseAbilitiesVersusLevelHelper(_abilitiesRequired, desire, extraTotal, level);
                    helper.ModifyActionList(character, alreadyConsidered, log);
                }
            }
        }

        public bool IsComplete(Character character)
        {
            return ((Magus)character).SpellList.Where(s => s.Name == _spell.Name).Any();
        }
    }

    class LongevityRitualGoal : IGoal
    {
        private HasLabCondition _hasLabCondition;
        private List<Ability> _abilitiesRequired;
        private List<Ability> _artsRequired;

        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if(!_hasLabCondition.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public LongevityRitualGoal(double desire, uint? dueDate = null)
        {
            DueDate = dueDate;
            Desire = desire;
            _abilitiesRequired = new List<Ability>();
            _abilitiesRequired.Add(MagicArts.Creo);
            _abilitiesRequired.Add(MagicArts.Vim);
            _abilitiesRequired.Add(Abilities.MagicTheory);
            _artsRequired = new List<Ability>();
            _artsRequired.Add(MagicArts.Creo);
            _artsRequired.Add(MagicArts.Vim);
            List<AttributeType> attributes = new List<AttributeType>();
            attributes.Add(AttributeType.Intelligence);

            // we need a lab to create a longevity ritual
            _hasLabCondition = new HasLabCondition(desire, dueDate == null || dueDate <= 3 ? null : dueDate - 3);
        }
 
        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (character.GetType() == typeof(Magus))
            {
                double visNeed = character.SeasonalAge / 20.0;
                VisCondition visCondition = new VisCondition(_artsRequired, visNeed, Desire, DueDate == null ? null : DueDate - 2);

                bool visComplete = visCondition.IsComplete(character);
                bool labComplete = _hasLabCondition.IsComplete(character);

                if (!visComplete)
                {
                    visCondition.ModifyActionList(character, alreadyConsidered, log);
                }
                if (!labComplete)
                {
                    _hasLabCondition.ModifyActionList(character, alreadyConsidered, log);
                }
                if (visComplete && labComplete)
                {
                    //effectively, every five points of lab total is worth a decade of effectiveness
                    double dueDateDesire = Desire;
                    if (DueDate != null)
                    {
                        if (DueDate == 0)
                        {
                            log.Add("Not enough time to perform longevity ritual");
                            return;
                        }
                        dueDateDesire /= (double)DueDate;
                    }
                    log.Add("Performing longevity ritual worth " + dueDateDesire.ToString("0.00"));
                    alreadyConsidered.Add(new LongevityRitual(Abilities.MagicTheory, dueDateDesire));
                    double labTotal = ((Magus)character).GetLabTotal(MagicArtPairs.CrVi, Activity.LongevityRitual);
                    IncreaseAbilitiesHelper helper = 
                        new IncreaseAbilitiesHelper(_abilitiesRequired, Desire, labTotal, DueDate == null ? null : DueDate - 1);
                    helper.ModifyActionList(character, alreadyConsidered, log);
                }
            }
        }

        public bool IsComplete(Character character)
        {
            return character.LongevityRitual > 0;
        }
    }

    class HasApprenticeCondition : IGoal
    {
        #region Private Fields
        AbilityScoreCondition[] _artRequirements;
        #endregion
        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                foreach (AbilityScoreCondition condition in _artRequirements)
                {
                    if (!condition.DecrementDueDate())
                    {
                        return false;
                    }
                }
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public HasApprenticeCondition(double desire, uint? dueDate = null)
        {
            DueDate = dueDate;
            Desire = desire;

            // TODO: remove the magic number
            _artRequirements = new AbilityScoreCondition[15];
            uint? modifiedDueDate = dueDate == null || dueDate < 3 ? null : dueDate - 2;
            _artRequirements[0] = new AbilityScoreCondition(MagicArts.Creo, 5, Desire, modifiedDueDate);
            _artRequirements[1] = new AbilityScoreCondition(MagicArts.Intellego, 5, Desire, modifiedDueDate);
            _artRequirements[2] = new AbilityScoreCondition(MagicArts.Muto, 5, Desire, modifiedDueDate);
            _artRequirements[3] = new AbilityScoreCondition(MagicArts.Perdo, 5, Desire, modifiedDueDate);
            _artRequirements[4] = new AbilityScoreCondition(MagicArts.Rego, 5, Desire, modifiedDueDate);
            _artRequirements[5] = new AbilityScoreCondition(MagicArts.Animal, 5, Desire, modifiedDueDate);
            _artRequirements[6] = new AbilityScoreCondition(MagicArts.Aquam, 5, Desire, modifiedDueDate);
            _artRequirements[7] = new AbilityScoreCondition(MagicArts.Auram, 5, Desire, modifiedDueDate);
            _artRequirements[8] = new AbilityScoreCondition(MagicArts.Corpus, 5, Desire, modifiedDueDate);
            _artRequirements[9] = new AbilityScoreCondition(MagicArts.Herbam, 5, Desire, modifiedDueDate);
            _artRequirements[10] = new AbilityScoreCondition(MagicArts.Ignem, 5, Desire, modifiedDueDate);
            _artRequirements[11] = new AbilityScoreCondition(MagicArts.Imaginem, 5, Desire, modifiedDueDate);
            _artRequirements[12] = new AbilityScoreCondition(MagicArts.Mentem, 5, Desire, modifiedDueDate);
            _artRequirements[13] = new AbilityScoreCondition(MagicArts.Terram, 5, Desire, modifiedDueDate);
            _artRequirements[14] = new AbilityScoreCondition(MagicArts.Vim, 5, Desire, modifiedDueDate);
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            bool isReady = true;
            foreach (AbilityScoreCondition condition in _artRequirements)
            {
                if (!condition.IsComplete(character))
                {
                    isReady = false;
                    condition.ModifyActionList(character, alreadyConsidered, log);
                }
            }
            if (isReady)
            {
                double desire = Desire;
                if(DueDate != null)
                {
                    if (DueDate == 0)
                    {
                        log.Add("Not enough time to find an apprentice");
                        return;
                    }
                    desire /= (double)DueDate;
                }
                alreadyConsidered.Add(new FindApprentice(Abilities.Etiquette, desire));
            }
        }

        public bool IsComplete(Character character)
        {
            // TODO: there's nothing preventing a character from having multiple apprentices
            return character.GetType() == typeof(Magus) && ((Magus)character).Apprentice != null;
        }
    }
    #endregion

    #region Goal Helpers
    public abstract class BaseHelper
    {
        public double BaseDesire { get; private set; }
        public uint? BaseDueDate { get; private set; }
        public List<Ability> Abilities { get; private set; }

        public BaseHelper(List<Ability> abilities, double baseDesire, uint? baseDueDate = null)
        {
            Abilities = abilities;
            BaseDesire = baseDesire;
            BaseDueDate = baseDueDate;
        }

        protected abstract double CalculateDesire(double increase, byte requiredTime = 1);

        public virtual void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (BaseDueDate != null && BaseDueDate == 0)
            {
                return;
            }
            IEnumerable<IBook> readableBooks = character.ReadableBooks;
            foreach (Ability ability in Abilities)
            {
                // Handle Reading
                CharacterAbilityBase charAbility = character.GetAbility(ability);
                var topicalBooks = readableBooks.Where(b => b.Topic == ability);
                HandleReading(character, alreadyConsidered, topicalBooks);

                // Handle Practice
                HandlePractice(character, alreadyConsidered, log, ability);


                // Handle Vis
                if (character.GetType() == typeof(Magus))
                {
                    Magus mage = (Magus)character;
                    if (MagicArts.IsArt(charAbility.Ability))
                    {
                        HandleVisUse(mage, charAbility, alreadyConsidered, log);
                    }
                }

                // TODO: Learning By Training
                // TODO: Learning by Teaching
            }
        }

        private void HandlePractice(Character character, ConsideredActions alreadyConsidered, IList<string> log, Ability ability)
        {
            // Handle Practice
            // For now, assume 4pt practice on everything
            // after lots of math, the right equation is:
            // Desire * (labTotal + increase)/(labTotal + increase + level)
            double increase = character.GetAbility(ability).GetValueGain(4);
            double desire = CalculateDesire(increase);
            if (BaseDueDate != null)
            {
                desire /= (double)BaseDueDate;
            }
            Practice practiceAction = new Practice(ability, desire);
            log.Add("Practicing " + ability.AbilityName + " worth " + (desire).ToString("0.00"));
            alreadyConsidered.Add(practiceAction);
        }

        private void HandleReading(Character character, ConsideredActions alreadyConsidered, IEnumerable<IBook> topicalBooks)
        {
            if (topicalBooks.Any())
            {
                var bestBook =
                    (from book in topicalBooks
                     orderby character.GetBookLevelGain(book),
                             book.Level ascending
                     select book).First();
                double increase = character.GetBookLevelGain(bestBook);
                double desire = CalculateDesire(increase);
                if (BaseDueDate != null)
                {
                    desire /= (double)BaseDueDate;
                }

                Reading readingAction = new Reading(bestBook, desire);
                alreadyConsidered.Add(readingAction);
            }
        }
    
        private void HandleVisUse(Magus mage, CharacterAbilityBase magicArt, ConsideredActions alreadyConsidered, IList<string> log)
        {
            // see if the mage has enough vis of this type
            double stockpile = mage.GetVisCount(magicArt.Ability);
            double visNeed = 0.5 + (magicArt.Value / 10.0);
            double increase = magicArt.GetValueGain(6);
            double baseDesire = CalculateDesire(increase);
            if (BaseDueDate != null)
            {
                baseDesire /= (double)BaseDueDate;
            }
            // if so, assume vis will return an average of 6XP
            if (stockpile > visNeed)
            {
                log.Add("Studying vis for " + magicArt.Ability.AbilityName + " worth " + baseDesire.ToString("0.00"));
                VisStudying visStudy = new VisStudying(magicArt.Ability, baseDesire);
                alreadyConsidered.Add(visStudy);
                // TODO: how do we decrement the cost of the vis?
            }
            // putting a limit here to how far the circular loop will go
            else if (baseDesire >= 0.1)
            {
                List<Ability> visType = new List<Ability>();
                visType.Add(magicArt.Ability);
                VisCondition visCondition = new VisCondition(visType, visNeed - stockpile, baseDesire / 2, BaseDueDate == null ? null : BaseDueDate - 1);
                visCondition.ModifyActionList(mage, alreadyConsidered, log);
            }
        }
    }
    
    /// <summary>
    /// This is a specialized goal-like object used to figure out 
    /// the value of increasing abilities relative to a certain lab spell/effect.
    /// </summary>
    class IncreaseAbilitiesVersusLevelHelper : BaseHelper
    {
        private double _currentGain;
        private double _effectLevel;
        public IncreaseAbilitiesVersusLevelHelper(List<Ability> abilities, double desire, double currentRate, double level, uint? dueDate = null) 
            : base(abilities, desire, dueDate)
        {
            _currentGain = currentRate;
            _effectLevel = level;
        }

        protected override double CalculateDesire(double increase, byte requiredTime = 1)
        {
            return (_currentGain + increase) * BaseDesire / (((_currentGain + increase) * requiredTime) + _effectLevel); 
        }
    }

    /// <summary>
    /// This is a specialized goal-like object used to figure out
    /// the value of increasing abilities relative to a certain lab total
    /// </summary>
    class IncreaseAbilitiesHelper : BaseHelper
    {
        private double _currentTotal;
        public IncreaseAbilitiesHelper(List<Ability> abilities, double desire, double currentTotal, uint? dueDate = null) 
            : base(abilities, desire, dueDate)
        {
            _currentTotal = currentTotal;
        }

        protected override double CalculateDesire(double increase, byte requiredTime = 1)
        {
            if (_currentTotal == 0) return BaseDesire / (requiredTime + 1);
            return (increase + _currentTotal) * BaseDesire / (_currentTotal * (requiredTime + 1));
        }
    }

    #endregion
}
