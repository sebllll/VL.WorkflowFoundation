using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities.Tracking;
using System.IO;
using System.Activities;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Activities.Statements;

namespace RehostedWorkflowDesigner
{

    /// <summary>
    /// Workflow Tracking Participant - Custom Implementation
    /// </summary>
    internal class ExecutionLogger : TrackingParticipant
    {
        public IObservable<ExecutionMessage> Messages;
        readonly Subject<ExecutionMessage> subject = new Subject<ExecutionMessage>();

        public ExecutionLogger()
        {
            Messages = subject;
        }

        StringBuilder parentStringBuilder = new StringBuilder();

        private Dictionary<string, bool> enteredStates = new Dictionary<string, bool>(0);
        public IReadOnlyDictionary<string, bool> EnteredStates => enteredStates;

        public void ClearStates()
        {
            enteredStates.Clear();
        }

        /// <summary>
        /// Appends the current TrackingRecord data to the Workflow Execution Log
        /// </summary>
        /// <param name="trackRecord">Tracking Record Data</param>
        /// <param name="timeStamp">Timestamp</param>
        protected override void Track(TrackingRecord trackRecord, TimeSpan timeStamp)
        {
            ActivityStateRecord recordEntry = trackRecord as ActivityStateRecord;

            var name = recordEntry?.Activity.Name;
            if (recordEntry != null && name != "StateMachineEventManagerFactory" && name != "DynamicActivity")
            {
                var message = new ExecutionMessage() { Sender = name, ExecutionState = recordEntry.State, TrackingRecord = recordEntry };

                if(recordEntry.Activity.TryGetInstance(out var instance))
                {
                    var l = new List<ActivityInstance>();
                    var n = new List<string>();
                    parentStringBuilder.Clear();
                    GetParentChain(instance, l, n);
                    message.ParentChain = l;
                    message.ParentNames = n;
                    if(instance.Activity.GetType() == ReflectionUtils.internalStateType)
                    {
                        if(message.ExecutionState == "Executing")
                            enteredStates[message.Address] = true;
                        else if(message.ExecutionState == "Closed")
                            enteredStates[message.Address] = false;
                    }
                }

                subject.OnNext(message);
            }
        }

        private void GetParentChain(ActivityInstance instance, List<ActivityInstance> parents, List<string> sb)
        {
            if(instance.TryGetParent(out var parent))
            {
                var parentName = parent.Activity.DisplayName;
                if (parentName != "DynamicActivity")
                {
                    parents.Add(parent);
                    sb.Add(parent.Activity.DisplayName); 
                }
                GetParentChain(parent, parents, sb);
            }
        }
    }

    public class ExecutionMessage
    {
        public string Sender { get; internal set; }
        public string ExecutionState { get; internal set; }
        public ActivityStateRecord TrackingRecord { get; internal set; }
        public ActivityInstance Instance { get; internal set; }
        public IReadOnlyList<ActivityInstance> ParentChain { get; internal set; }
        public IReadOnlyList<string> ParentNames { get; internal set; }

        Lazy<string> address;
        public ExecutionMessage()
        {
            address = new Lazy<string>(() => string.Join("/", ParentNames.Reverse().SkipWhile(p => p == "StateMachine").Concat(new[] { Sender })));
        }

        public string Address => address.Value;
        
    }
}
