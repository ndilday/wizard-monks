using System;
using System.Runtime.Serialization;

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
		Social,
        [DataMember]
		Supernatural,
        [DataMember]
        Warping,
        [DataMember]
        Art
	}

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
		public double Experience { get; set; }

	    public Ability Ability
	    {
            get { return ImmutableMultiton<int, Ability>.GetInstance(_abilityId); }
	    }

		public abstract double GetValue();

        public virtual void AddExperience(double amount)
        {
            this.Experience += amount;
        }

        public virtual void AddExperience(double amount, double levelLimit = 0)
        {
            if (levelLimit == 0)
            {
                this.Experience += amount;
            }
            else
            {
                double experienceToLevel = this.GetExperienceUntilLevel(levelLimit);
                this.Experience += experienceToLevel < amount ? experienceToLevel : amount;
            }
        }
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

        public override double GetValue()
		{
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

        public override double GetValue()
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
    }
}
