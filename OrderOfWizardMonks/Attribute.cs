using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderOfHermes
{
    // TODO: create config option to switch between this attribute and an implementation based upon the strict AM rules

    public interface IAttribute
    {
        sbyte Value { get; }
        sbyte BaseValue { get; set; }
        byte Decrepitude { get; }
        void AddDecrepitude(byte additionalDecrepitude);
        byte AddDecrepitudeToNextLevel();
    }

    [Serializable]
    public class Attribute : IAttribute
    {
        private sbyte _baseValue;
        private byte _decrepitude;

        public sbyte Value { get; private set; }

        public Attribute(sbyte startingValue, byte startingDecrepitude = (byte)(0))
        {
            _baseValue = startingValue;
            _decrepitude = startingDecrepitude;
            RecalculateValue();
        }

        public sbyte BaseValue
        {
            get { return _baseValue; }
            set 
            { 
                _baseValue = value;
                RecalculateValue();
            }
        }

        public byte Decrepitude
        {
            get { return _decrepitude; }
        }

        public void AddDecrepitude(byte additionalDecrepitude)
        {
            _decrepitude += additionalDecrepitude;
            RecalculateValue();
        }

        public byte AddDecrepitudeToNextLevel()
        {
            byte decrepitudeUsed = 0;
            for(sbyte i = _baseValue; i < Value; i--)
            {
                decrepitudeUsed = (byte)(decrepitudeUsed + Math.Abs(i) + 1);
            }

            byte decrepitudeRemaining = (byte)(_decrepitude - decrepitudeUsed);
            byte decrepitudeNeeded = (byte)( Math.Abs(Value) + 1);
            byte addedDecreptide = (byte) (decrepitudeNeeded - decrepitudeRemaining);
            Value--;
            return addedDecreptide;
        }

        private void RecalculateValue()
        {
            sbyte tempValue = _baseValue;
            byte decrepitude = _decrepitude;
            sbyte absValue = Math.Abs(tempValue);

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
