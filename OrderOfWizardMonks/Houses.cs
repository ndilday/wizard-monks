using System;
using System.Collections.Generic;
using WizardMonks.Beliefs;

namespace WizardMonks
{
    [Serializable]
    public enum HousesEnum
    {
        Apprentice,
        Bjornaer,
        Bonisagus,
        Criamon,
        Diedne,
        ExMiscellanea,
        Flambeau,
        Guernicus,
        Jerbiton,
        Mercere,
        Merinita,
        Tremere,
        Tytalus,
        Verditius
    }

    public static class Houses
    {
        private static readonly Dictionary<HousesEnum, HouseSubject> _houseSubjects = new();

        static Houses()
        {
            foreach (HousesEnum houseEnum in Enum.GetValues(typeof(HousesEnum)))
            {
                _houseSubjects.Add(houseEnum, new HouseSubject(houseEnum));
            }
        }

        public static IBeliefSubject GetSubject(HousesEnum house)
        {
            return _houseSubjects[house];
        }

        private class HouseSubject : IBeliefSubject
        {
            public Guid Id { get; private set; }
            public string Name { get; private set; }

            public HouseSubject(HousesEnum house)
            {
                // Use a deterministic GUID for enums so it's consistent across runs
                Id = new Guid(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)house });
                Name = house.ToString();
            }
        }
    }
}
