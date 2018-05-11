using System;
using System.Runtime.Serialization;

using WizardMonks.Core;
using WizardMonks.Instances;

namespace WizardMonks
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
        Language,
        [DataMember]
        AreaLore,
        [DataMember]
        Profession
	}

    public delegate void AbilityValueChangedEventHandler(object sender, EventArgs e);

	[DataContract]
	public class Ability: IKeyed<int>
	{
        [DataMember(Name="AbilityId", IsRequired = true)]
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
            if (Changed != null)
            {
                Changed(this, e);
            }
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
            this.Experience += amount;
        }

        public virtual void AddExperience(double amount, double levelLimit = 0)
        {
            double prevExperience = this.Experience;
            if (levelLimit == 0)
            {
                this.Experience += amount;
            }
            else
            {
                double experienceToLevel = this.GetExperienceUntilLevel(levelLimit);
                this.Experience += experienceToLevel < amount ? experienceToLevel : amount;
            }
            if (prevExperience != this.Experience)
            {
                _cached = false;
                OnChanged(new EventArgs());
            }
        }

        public abstract int GetTractatiiLimit();
	}

    [DataContract]
    public class CharacterAbility : CharacterAbilityBase
    {
        public CharacterAbility(Ability newAbility)
            : base(newAbility)
        {
        }

        public override CharacterAbilityBase MakeCopy()
        {
            CharacterAbility retVal = new CharacterAbility(this.Ability);
            retVal.Experience = this.Experience;
            retVal.IsAffinity = this.IsAffinity;
            retVal.IsPuissant = this.IsPuissant;
            return retVal;
        }

        protected override double GetValueHelper()
		{
            if (_cached)
            {
                return _value;
            }
			double x = this.Experience;
			if (this.IsAffinity)
			{
				x *= 1.5;
			}

			x *= .4;
			x += .25;
			x = Math.Sqrt(x);
			x -= .5;
			if (this.IsPuissant)
			{
				x += 2;
			}

			return x;
		}

        public override double GetExperienceUntilLevel(double level)
        {
            double totalExperience = level * (level + 1) * 5 / 2;
            return totalExperience - this.Experience;
        }

        public override int GetTractatiiLimit()
        {
            if (Value < 2) return 0;
            return (int)Math.Floor(Value);
        }
    }

    [DataContract]
    public class AcceleratedAbility : CharacterAbilityBase
    {
        public AcceleratedAbility(Ability newAbility)
            : base(newAbility)
        {
        }

        public override CharacterAbilityBase MakeCopy()
        {
            AcceleratedAbility retVal = new AcceleratedAbility(this.Ability);
            retVal.Experience = this.Experience;
            retVal.IsAffinity = this.IsAffinity;
            retVal.IsPuissant = this.IsPuissant;
            return retVal;
        }

        protected override double GetValueHelper()
        {
            double x = this.Experience;
            if (this.IsAffinity)
            {
                x *= 1.5;
            }

            x *= 2;
            x += .25;
            x = Math.Sqrt(x);
            x -= .5;
            if (this.IsPuissant)
            {
                x += 2;
            }

            return x;
        }

        public override double GetExperienceUntilLevel(double level)
        {
            double totalExperience = level * (level + 1) / 2;
            return totalExperience - this.Experience;
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
