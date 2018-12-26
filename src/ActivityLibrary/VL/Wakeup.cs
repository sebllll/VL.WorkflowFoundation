using System;
using System.Activities;
using System.Activities.DynamicUpdate;

namespace ActivityLibrary
{
    public sealed class Wakeup : NativeActivity
    {
        public Wakeup()
        {
        }

        public InArgument<string> BookmarkName { get; set; }

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
            string name = this.BookmarkName.Get(context);

            if (name == null)
            {
                throw new ArgumentException(string.Format("ReadLine {0}: BookmarkName cannot be null", this.DisplayName), "BookmarkName");
            }

            context.CreateBookmark(name);
        }

    }
}
