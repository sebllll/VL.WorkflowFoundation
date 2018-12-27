using System;
using System.Activities;
using System.Activities.DynamicUpdate;
using System.Diagnostics;

namespace ActivityLibrary
{
    public sealed class WaitFor : NativeActivity
    {
        public WaitFor()
        {
        }

        public string MessageName { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            base.OnCreateDynamicUpdateMap(metadata, originalActivity);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var activities = WorkflowInspectionServices.GetActivities(this);
            string name = MessageName;

            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.WriteLine("Bookmark name is empty");
                return;
            }

            context.CreateBookmark(name);
        }

    }
}
