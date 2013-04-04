using System;
using System.Collections.Generic;

namespace WizardMonks
{
    public interface ISeasonDecision
    {
        ISeasonDecision Decide();
    }

    [Serializable]
    public class SeasonDecision<T> where T: ISeasonDecision
    {
        private SortedList<double, T> decisionList = new SortedList<double,T>();

        public SortedList<double, T> DecisionList { get; set; }
        
        public virtual ISeasonDecision Decide()
        {
            double value = Die.Instance.RollDouble();
            int i = 0;
            while (decisionList.Keys[i] < value)
            {
                i++;
            }
            return decisionList.Values[i].Decide();
        }
    }
}
