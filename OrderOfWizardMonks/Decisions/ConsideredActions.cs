using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;

namespace WizardMonks.Decisions
{
    public class ConsideredActions
    {
        readonly Dictionary<Activity, IList<IActivity>> ActionTypeMap = [];
        private readonly bool _useMaxSemantics;

        public ConsideredActions(bool useMaxSemantics = false)
        {
            _useMaxSemantics = useMaxSemantics;
        }

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
                    if (_useMaxSemantics)
                        match.Desire = System.Math.Max(match.Desire, action.Desire);
                    else
                        match.Desire += action.Desire;
                }
                else
                {
                    value.Add(action);
                }
            }
        }

        /// <summary>
        /// Additively merges all actions from <paramref name="other"/> into this collection,
        /// regardless of this collection's accumulation mode. Used to combine per-goal
        /// max-mode collections into the global sum-mode collection.
        /// </summary>
        public void MergeFrom(ConsideredActions other)
        {
            foreach (var action in other.ActionTypeMap.SelectMany(kvp => kvp.Value))
            {
                if (!ActionTypeMap.TryGetValue(action.Action, out IList<IActivity> value))
                {
                    ActionTypeMap[action.Action] = [action];
                }
                else
                {
                    var match = value.Where(a => a.Matches(action)).FirstOrDefault();
                    if (match != null)
                        match.Desire += action.Desire;
                    else
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
