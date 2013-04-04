using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks
{
    public class TwilightEventArgs : EventArgs
    {
        DateTime _duration;
        ushort _extraWarping;

        public TwilightEventArgs(DateTime duration, ushort extraWarping)
        {
            _duration = duration;
            _extraWarping = extraWarping;
        }
    }

	[Serializable]
	public class Magus : Character
	{
		public Houses House { get; set; }

        public Arts Arts { get; set; }

        private Ability _magicAbility;

        protected void CheckTwilight()
        {
        }

        public override double GetLabTotal(Ability technique, Ability form)
        {
            double magicTheory = GetAbility(_magicAbility).GetValue();
            double techValue = Arts.GetAbility(technique).GetValue();
            double formValue = Arts.GetAbility(form).GetValue();

            return magicTheory + techValue + formValue;
            //TODO: laboratory
            //TODO: foci
            //TODO: lab assistant
            //TODO: familiar
        }
	}
}
