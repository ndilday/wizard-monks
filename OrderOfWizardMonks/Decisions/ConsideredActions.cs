using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions
{
    public class ConsideredActions
    {
        Dictionary<Activity, IList<IAction>> ActionTypeMap = new();

        public void Add(IAction action)
        {
            if (!ActionTypeMap.ContainsKey(action.Action))
            {
                ActionTypeMap[action.Action] = new List<IAction>();
                ActionTypeMap[action.Action].Add(action);
            }
            else
            {
                var match = ActionTypeMap[action.Action].Where(a => a.Matches(action)).FirstOrDefault();
                if (match != null)
                {
                    match.Desire += action.Desire;
                }
                else
                {
                    ActionTypeMap[action.Action].Add(action);
                }
            }
        }

        public IList<string> Log()
        {
            List<string> log = new();
            log.Add("----------");
            log.AddRange(ActionTypeMap.SelectMany(a => a.Value).Select(a => a.Log()));
            log.Add("----------");
            return log;
        }

        public IAction GetBestAction()
        {
            return ActionTypeMap.SelectMany(a => a.Value).OrderByDescending(a => a.Desire).FirstOrDefault();
        }
    }
}
