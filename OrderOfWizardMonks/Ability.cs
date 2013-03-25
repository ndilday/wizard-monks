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

    public class AbilityValue : Ability
    {
        public int AbilityChange { get; set; }
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

        [DataMember(IsRequired = true)]
		public bool IsPuissant { get; set; }
        [DataMember(IsRequired = true)]
		public bool IsAffinity { get; set; }
        [DataMember(IsRequired = true)]
		public uint Experience { get; set; }

	    public Ability Ability
	    {
            get { return ImmutableMultiton<int, Ability>.GetInstance(_abilityId); }
	    }

		public abstract byte GetValue();
	}

    [DataContract]
    public class CharacterAbility : CharacterAbilityBase
    {
        public CharacterAbility(Ability newAbility)
            : base(newAbility)
        {
        }

        public override byte GetValue()
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
			x = Math.Floor(x);
			if (this.IsPuissant)
			{
				x += 2;
			}

			return Convert.ToByte(x);
		}
    }

    [DataContract]
    public class AcceleratedAbility : CharacterAbilityBase
    {
        public AcceleratedAbility(Ability newAbility)
            : base(newAbility)
        {
        }

        public override byte GetValue()
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
            x = Math.Floor(x);
            if (this.IsPuissant)
            {
                x += 2;
            }

            return Convert.ToByte(x);
        }
    }
}
