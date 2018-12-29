using System;
using System.Activities;
using System.Activities.DynamicUpdate;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Activities.Validation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ActivityLibrary
{
    public sealed class Wait : NativeActivity<object>
    {
        //public OutArgument<object> Result { get; set; }
        GetParentChain parentChain = new GetParentChain();

        public Wait()
        {
            Constraints.Add(SetMessageName());
        }

        public string MessageName { get; private set; }

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
            string name = MessageName;

            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.WriteLine("Bookmark name is empty");
                return;
            }

            context.CreateBookmark(name, OnResumeBookmark);
        }

        public void OnResumeBookmark(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            // When the Bookmark is resumed, assign its value to
            // the Result argument.
            Result.Set(context, obj);
        }

        private Constraint<Wait> SetMessageName()
        {
            var activityBeingValidated = new DelegateInArgument<Wait>();
            var validationContext = new DelegateInArgument<ValidationContext>();
            var parent = new DelegateInArgument<Activity>();
            var parentIsTransitionTrigger = new Variable<bool>();

            return new Constraint<Wait>
            {
                Body = new ActivityAction<Wait, ValidationContext>
                {
                    Argument1 = activityBeingValidated,
                    Argument2 = validationContext,

                    Handler = new Sequence
                    {
                        Variables =
                        {
                            parentIsTransitionTrigger
                        },
                        Activities =
                        {
                            new ForEach<Activity>
                            {
                                Values = new GetParentChain
                                {
                                    ValidationContext = validationContext
                                },

                                Body = new ActivityAction<Activity>
                                {
                                    Argument = parent,

                                    Handler = new If
                                    {
                                        Condition = new InArgument<bool>(context => GetParentTransitionName(context, parent.Get(context))),

                                        Then = new Assign<bool>
                                        {
                                            To = parentIsTransitionTrigger,
                                            Value = true
                                        }
                                    }
                                }
                            },

                            new AssertValidation
                            {
                                Assertion = parentIsTransitionTrigger,
                                Message = "Wait must be placed inside the Trigger of a Transition or you can use WaitFor instead"
                            }
                        }
                    }
                }
            };
        }

        bool GetParentTransitionName(ActivityContext context, Activity parent)
        {
            var transitions = parent.GetType().GetProperty("Transitions")?.GetValue(parent) as IEnumerable<Transition>;
            if(transitions != null)
            {
                var transition = transitions.FirstOrDefault(t => t.Trigger == this);
                if (transition != null)
                {
                    MessageName = transition.DisplayName;
                    return true;
                }
                else
                {
                    foreach(var t in transitions.Where(trans => trans.Trigger is Sequence))
                    {
                        var seq = t.Trigger as Sequence;
                        if (seq.Activities.Any(a => a == this))
                        {
                            MessageName = t.DisplayName;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
