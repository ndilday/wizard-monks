using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions
{
    public class ConsideredActions
    {
        readonly Dictionary<Activity, IList<IAction>> ActionTypeMap = [];

        public void Add(IAction action)
        {
            if (!ActionTypeMap.TryGetValue(action.Action, out IList<IAction> value))
            {
                ActionTypeMap[action.Action] = [action];
            }
            else
            {
                var match = value.Where(a => a.Matches(action)).FirstOrDefault();
                if (match != null)
                {
                    match.Desire += action.Desire;
                }
                else
                {
                    value.Add(action);
                }
            }
        }

        public IList<string> Log()
        {
            List<string> log =
            [
                "----------",
                .. ActionTypeMap.SelectMany(a => a.Value).OrderByDescending(a => a.Desire).Select(a => a.Log()),
                "----------",
            ];
            return log;
        }

        public IAction GetBestAction()
        {
            return ActionTypeMap.SelectMany(a => a.Value).OrderByDescending(a => a.Desire).FirstOrDefault();
        }
    }
}
