using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks
{
    // TODO: create config option to switch between this attribute and an implementation based upon the strict AM rules

    public interface IAttribute
    {
        double Value { get; }
        double BaseValue { get; set; }
        double Decrepitude { get; }
        void AddDecrepitude(byte additionalDecrepitude);
        //float AddDecrepitudeToNextLevel();
    }

    public enum AttributeType
    {
        Strength,
        Stamina,
        Dexterity,
        Quickness,
        Intelligence,
        Communication,
        Perception,
        Presence
    }

    [Serializable]
    public class Attribute : IAttribute
    {
        private double _baseValue;
        private double _decrepitude;

        public double Value { get; private set; }

        public Attribute(double startingValue, double startingDecrepitude = 0)
        {
            _baseValue = startingValue;
            _decrepitude = startingDecrepitude;
            RecalculateValue();
        }

        public double BaseValue
        {
            get { return _baseValue; }
            set 
            { 
                _baseValue = value;
                RecalculateValue();
            }
        }

        public double Decrepitude
        {
            get { return _decrepitude; }
        }

        public void AddDecrepitude(byte additionalDecrepitude)
        {
            _decrepitude += additionalDecrepitude;
            RecalculateValue();
        }

        private void RecalculateValue()
        {
            double tempValue = _baseValue;
            double decrepitude = _decrepitude;
            double absValue = Math.Abs(tempValue);

            while (decrepitude >= absValue + 1)
            {
                // each abs + 1 reduces the stat by a point
                decrepitude = (byte)(decrepitude - absValue - 1);
                tempValue--;
                absValue = Math.Abs(tempValue);
            }

            Value = tempValue;
        }
    }
}
