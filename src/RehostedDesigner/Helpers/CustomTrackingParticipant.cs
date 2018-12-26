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

namespace RehostedWorkflowDesigner.Helpers
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
                var message = new ExecutionMessage() { Sender = recordEntry.Activity.Name, State = recordEntry.State, TrackingRecord = recordEntry };

                if(recordEntry.Activity.TryGetInstance(out var instance))
                {
                    //instance.GetPath();
                    message.Instance = instance;
                }

                subject.OnNext(message);
            }
        }
    }

    public class ExecutionMessage
    {
        public string Sender { get; set; }
        public string State { get; set; }
        public ActivityStateRecord TrackingRecord { get; set; }
        public ActivityInstance Instance { get; set; }
    }
}
