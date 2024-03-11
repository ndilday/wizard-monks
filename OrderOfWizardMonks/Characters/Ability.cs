using System;
using System.Runtime.Serialization;
using WizardMonks.Core;
using WizardMonks.Instances;

namespace WizardMonks.Characters
{
    [DataContract]
    public enum AbilityType
    {
        [DataMember]
        Academic,
        [DataMember]
        Arcane,
        [DataMember]
        Decrepitude,
        [DataMember]
        General,
        [DataMember]
        Martial,
        [DataMember]
        Mystery,
        [DataMember]
        Social,
        [DataMember]
        Supernatural,
        [DataMember]
        Warping,
        [DataMember]
        Art,
        [DataMember]
        Language
    }

    public delegate void AbilityValueChangedEventHandler(object sender, EventArgs e);

    [DataContract]
    public class Ability : IKeyed<int>
    {
        [DataMember(Name = "AbilityId", IsRequired = true)]
        private int _abilityId;
        [DataMember(IsRequired = true)]
        public AbilityType AbilityType { get; set; }
        [DataMember(IsRequired = true)]
        public string AbilityName { get; set; }

        public Ability()
        {
        }

        public Ability(int abilityId, AbilityType type, string name)
        {
            _abilityId = abilityId;
            AbilityType = type;
            AbilityName = name;
        }

        public int AbilityId
        {
            get { return _abilityId; }
        }

        public int GetKey()
        {
            return _abilityId;
        }

        public override string ToString()
        {
            return AbilityName;
        }
    }

    [DataContract]
    public class AbilityList
    {
        [DataMember(IsRequired = true)]
        public Ability[] Abilities { get; set; }
    }

    public class AbilityValue
    {
        public double AbilityChange { get; set; }
        public Ability Ability { get; set; }
    }

    [DataContract]
    [KnownType(typeof(CharacterAbility))]
    [KnownType(typeof(AcceleratedAbility))]
    public abstract class CharacterAbilityBase
    {
        [DataMember(IsRequired = true)]
        private int _abilityId;
        protected double _value;
        protected bool _cached;
        private double _experience;

        public event AbilityValueChangedEventHandler Changed;

        public virtual void OnChanged(EventArgs e)
        {
            Changed?.Invoke(this, e);
        }

        protected CharacterAbilityBase(Ability ability)
        {
            _abilityId = ability.AbilityId;

            IsPuissant = IsAffinity = false;
            Experience = 0;
        }

        protected CharacterAbilityBase(int abilityId)
        {
            _abilityId = abilityId;

            IsPuissant = IsAffinity = false;
            Experience = 0;
        }

        public abstract CharacterAbilityBase MakeCopy();

        public abstract double GetExperienceUntilLevel(double level);

        [DataMember(IsRequired = true)]
        public bool IsPuissant { get; set; }
        [DataMember(IsRequired = true)]
        public bool IsAffinity { get; set; }
        [DataMember(IsRequired = true)]
        public double Experience
        {
            get
            {
                return _experience;
            }
            set
            {
                _experience = value;
                _cached = false;
            }
        }

        public string Name
        {
            get
            {
                return Ability.AbilityName;
            }
        }

        public Ability Ability
        {
            get { return ImmutableMultiton<int, Ability>.GetInstance(_abilityId); }
        }

        public double Value
        {
            get
            {
                if (_cached)
                {
                    return _value;
                }

                double x = GetValueHelper();

                _value = x;
                _cached = true;

                return x;
            }
        }

        public double GetValueGain(double experience, double levelLimit = 0)
        {
            CharacterAbilityBase copy = MakeCopy();
            copy.AddExperience(experience, levelLimit);
            return copy.Value - Value;
        }

        protected abstract double GetValueHelper();

        public virtual void AddExperience(double amount)
        {
            Experience += amount;
        }

        public virtual void AddExperience(double amount, double levelLimit = 0)
        {
            double prevExperience = Experience;
            if (levelLimit == 0)
            {
                Experience += amount;
            }
            else
            {
                double experienceToLevel = GetExperienceUntilLevel(levelLimit);
                Experience += experienceToLevel < amount ? experienceToLevel : amount;
            }
            if (prevExperience != Experience)
            {
                _cached = false;
                OnChanged(new EventArgs());
            }
        }

        public abstract int GetTractatiiLimit();
    }

    [DataContract]
    public class CharacterAbility(Ability newAbility) : CharacterAbilityBase(newAbility)
    {
        public override CharacterAbilityBase MakeCopy()
        {
            CharacterAbility retVal = new(Ability)
            {
                Experience = Experience,
                IsAffinity = IsAffinity,
                IsPuissant = IsPuissant
            };
            return retVal;
        }

        protected override double GetValueHelper()
        {
            if (_cached)
            {
                return _value;
            }
            double x = Experience;
            if (IsAffinity)
            {
                x *= 1.5;
            }

            x *= .4;
            x += .25;
            x = Math.Sqrt(x);
            x -= .5;
            if (IsPuissant)
            {
                x += 2;
            }

            return x;
        }

        public override double GetExperienceUntilLevel(double level)
        {
            double totalExperience = level * (level + 1) * 5 / 2;
            return totalExperience - Experience;
        }

        public override int GetTractatiiLimit()
        {
            if (Value < 2) return 0;
            return (int)Math.Floor(Value);
        }
    }

    [DataContract]
    public class AcceleratedAbility(Ability newAbility) : CharacterAbilityBase(newAbility)
    {
        public override CharacterAbilityBase MakeCopy()
        {
            AcceleratedAbility retVal = new(Ability)
            {
                Experience = Experience,
                IsAffinity = IsAffinity,
                IsPuissant = IsPuissant
            };
            return retVal;
        }

        protected override double GetValueHelper()
        {
            double x = Experience;
            if (IsAffinity)
            {
                x *= 1.5;
            }

            x *= 2;
            x += .25;
            x = Math.Sqrt(x);
            x -= .5;
            if (IsPuissant)
            {
                x += 2;
            }

            return x;
        }

        public override double GetExperienceUntilLevel(double level)
        {
            double totalExperience = level * (level + 1) / 2;
            return totalExperience - Experience;
        }

        public override int GetTractatiiLimit()
        {
            return (int)Math.Floor(Value / 5.0);
        }
    }

    [DataContract]
    public class ArtPair
    {
        public Ability Technique { get; private set; }
        public Ability Form { get; private set; }

        public ArtPair(Ability technique, Ability form)
        {
            if (!MagicArts.IsTechnique(technique) || !MagicArts.IsForm(form))
            {
                throw new ArgumentException("one of the arts used to initialize this pair is invalid!");
            }
            Technique = technique;
            Form = form;
        }
    }
}
