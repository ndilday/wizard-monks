﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WizardMonks.Instances;

namespace WizardMonks
{
	[Serializable]
	public class IBook
	{
        public string Title { get; set; }
		public Character Author { get; set; }
		public Ability Topic { get; set; }
		public double Quality { get; set; }
		public virtual double Level { get; set; }
        //public virtual double Value { get; set; }
	}

	[Serializable]
	public class Summa : IBook
	{
        public double GetWritingPointsNeeded()
        {
            return MagicArts.IsArt(Topic) ? Level : Level * 5;
        }
        public double PointsComplete { get; set; }
	}

    [Serializable]
    public class Tractatus : IBook
    {
        public override double Level
        {
            get
            {
                return -1;
            }
            set
            {
            }
        }
    }

    public class EvaluatedBook
    {
        public IBook Book { get; set; }
        public double PerceivedValue { get; set; }
    }
}
