using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;

namespace WizardMonks.Decisions
{
    public class ConsideredActions
    {
        readonly Dictionary<Activity, IList<IActivity>> ActionTypeMap = [];

        public void Add(IActivity action)
        {
            if (!ActionTypeMap.TryGetValue(action.Action, out IList<IActivity> value))
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
            List<string> log = new();
            log.Add("----------");
            log.AddRange(ActionTypeMap.SelectMany(a => a.Value).OrderByDescending(a => a.Desire).Select(a => a.Log()));
            log.Add("----------");
            return log;
        }

        public IActivity GetBestAction()
        {
            return ActionTypeMap.SelectMany(a => a.Value).OrderByDescending(a => a.Desire).FirstOrDefault();
        }
    }
}
