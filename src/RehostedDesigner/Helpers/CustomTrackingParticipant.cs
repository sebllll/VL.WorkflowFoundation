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
    class CustomTrackingParticipant : TrackingParticipant
    {
        public IObservable<ExecutionMessage> Messages;
        readonly Subject<ExecutionMessage> subject = new Subject<ExecutionMessage>();

        public CustomTrackingParticipant()
        {
            Messages = subject;
        }

        /// <summary>
        /// Appends the current TrackingRecord data to the Workflow Execution Log
        /// </summary>
        /// <param name="trackRecord">Tracking Record Data</param>
        /// <param name="timeStamp">Timestamp</param>
        protected override void Track(TrackingRecord trackRecord, TimeSpan timeStamp)
        {
            ActivityStateRecord recordEntry = trackRecord as ActivityStateRecord;

            if (recordEntry != null)
            {
                var message = new ExecutionMessage() { Sender = recordEntry.Activity.Name, ExecutionState = recordEntry.State, TrackingRecord = recordEntry };

                if(recordEntry.Activity.TryGetInstance(out var instance))
                {
                    message.Instance = instance;
                    message.ParentMachine = GetParentMachine(instance);
                }

                subject.OnNext(message);
            }
        }

        private ActivityInstance GetParentMachine(ActivityInstance instance)
        {
            if(instance.TryGetParent(out var parent))
            {
                if (parent.Activity is StateMachine)
                    return parent;
                else
                    return GetParentMachine(parent);
            }

            return null;
        }
    }

    public class ExecutionMessage
    {
        public string Sender { get; internal set; }
        public string ExecutionState { get; internal set; }
        public ActivityStateRecord TrackingRecord { get; internal set; }
        public ActivityInstance Instance { get; internal set; }
        public ActivityInstance ParentMachine { get; internal set; }
    }
}
