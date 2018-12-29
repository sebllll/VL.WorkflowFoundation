using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Activities;
using System.Activities.Presentation.Toolbox;
using System.Reflection;
using System.IO;
using System.Activities.XamlIntegration;
using Microsoft.Win32;
using System.ComponentModel;
using System.Timers;
using System.Activities.Presentation;
using System.Windows.Controls;
using System.Activities.Statements;
using System.Activities.Core.Presentation;
using ActivityLibrary;
using System.Activities.Presentation.Model;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Windows.Media;
using System.Activities.Presentation.Metadata;
using ActivityLibrary.VL;
using System.Xml.Linq;

namespace RehostedWorkflowDesigner.Views
{
    /// <summary>
    /// Interaction logic for StateMachineTab.xaml
    /// </summary>
    public partial class StateMachineControl : UserControl, IStateMachineController, INotifyPropertyChanged
    {
        private WorkflowApplication _wfApp;
        private ToolboxControl _wfToolbox;
        private ExecutionLogger _executionLog;
        private WorkflowDesigner _wfDesigner;
        ModelTreeManager modelTreeManager;
        public IObservable<ExecutionMessage> Messages { get; private set; }
        Subject<ExecutionMessage> subject = new Subject<ExecutionMessage>();
        SerialDisposable disposable = new SerialDisposable();

        private string _currentWorkflowFile = string.Empty;

        public string FilePath
        {
            get { return _currentWorkflowFile; }
            set
            {
                _currentWorkflowFile = value;
                NotifyPropertyChanged("FilePath");
            }
        }


        public StateMachineControl()
        {
            InitializeComponent();

            // register metadata
            (new DesignerMetadata()).Register();
            RegisterCustomMetadata();

            Messages = subject.Publish().RefCount();

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //load all available workflow activities from loaded assemblies 
            InitializeActivitiesToolbox();

            SetDesigner();
            WfPropertyBorder.Child = _wfDesigner.PropertyInspectorView;
            //ConsoleOutput.AddTextBox(consoleOutput);
        }

        void RegisterCustomMetadata()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(ActivityLibrary.WaitFor), new DesignerAttribute(typeof(WaitForDesigner)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        private void ModelTreeManager_EditingScopeCompleted(object sender, EditingScopeEventArgs e)
        {
            if(e.EditingScope.HasEffectiveChanges)
                foreach(var change in e.EditingScope.Changes)
                {
                    var s = change.Description;
                    Debug.WriteLine(s);
                    IsDirty = true;
                }
        }

        public new string Name
        {
            get
            {
                return (string)modelTreeManager.Root.Properties["Name"].ComputedValue;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;
                Dispatcher.Invoke(new Action(() =>
                {
                    modelTreeManager.Root.Properties["Name"].ComputedValue = value;
                    UpdateTabName();
                }));
            }
        }

        private void UpdateTabName()
        {
            var ti = Parent as TabItem;
            if (ti != null)
                ti.Header = IsDirty ? Name + "*" : Name;
        }

        public int MaxExecutionLogLines { get; set; } = 50;
        private bool isDirty;

        public bool IsDirty
        {
            get { return isDirty; }
            set
            {
                if (value != isDirty)
                {
                    isDirty = value;
                    UpdateTabName();
                }
            }
        }

        private Dictionary<string, bool> emptyStates = new Dictionary<string, bool>(0);
        public IReadOnlyDictionary<string, bool> EnteredStates => _executionLog?.EnteredStates ?? emptyStates;

        public void SendMessage(string message, object value)
        {
            _wfApp?.ResumeBookmark(message, value);
        }

        public void Start()
        {
            CmdWorkflowRun(this, null);
        }

        public void Stop()
        {
            CmdWorkflowStop(this, null);
        }

        public void Load()
        {
            var filePath = _currentWorkflowFile;
            Dispatcher.Invoke(new Action(() =>
            {
                filePath = Path.ChangeExtension(filePath, ".xaml");
                if (File.Exists(filePath))
                {
                    SetDesigner(filePath);

                    var x = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");
                    var xElem = XElement.Parse(_wfDesigner.Text);
                    Name = xElem.Attribute(x + "Class").Value;
                    IsDirty = false;
                    UpdateTabName();
                }

                FilePath = filePath;
            }));
        }

        public void SaveAs(string filePath)
        {
            FilePath = filePath;
            Save();
        }

        public void Save()
        {
            var filePath = _currentWorkflowFile;
            Dispatcher.Invoke(new Action(() =>
            {
                FileInfo fi = null;
                try
                {
                    fi = new FileInfo(filePath);
                }
                catch (ArgumentException) { }
                catch (PathTooLongException) { }
                catch (NotSupportedException) { }

                if (ReferenceEquals(fi, null))
                {
                    Debug.WriteLine("Filename invalid: " + filePath);
                }
                else
                {
                    filePath = Path.ChangeExtension(filePath, ".xaml");
                    _wfDesigner.Save(filePath);
                    FilePath = filePath;
                    IsDirty = false;
                }
            }));
        }

        private void SetDesigner(string filePath = null)
        {
            _wfDesigner = CustomWfDesigner.NewInstance(filePath);
            modelTreeManager = _wfDesigner.Context.Services.GetService<ModelTreeManager>();
            if (modelTreeManager != null)
                modelTreeManager.EditingScopeCompleted -= ModelTreeManager_EditingScopeCompleted;
            modelTreeManager.EditingScopeCompleted += ModelTreeManager_EditingScopeCompleted;
            WfDesignerBorder.Child = _wfDesigner.View;
            WfPropertyBorder.Child = _wfDesigner.PropertyInspectorView;
        }


        /// <summary>
        /// Creates a new Workflow Application instance and executes the Current Workflow
        /// </summary>
        private void CmdWorkflowRun(object sender, ExecutedRoutedEventArgs e)
        {
            _wfApp?.Abort();

            var name = Name;
            var m = new ExecutionMessage() { ExecutionState = "WorkflowStart", Sender = Name, ParentNames = new string[0] };
            subject.OnNext(m);
            var s = string.Format("[{0}] [{1}] [{2}]",
                    DateTime.Now.ToString("HH:mm:ss"),
                    m.Sender,
                    m.ExecutionState);
            LogExecution(s);

            var activityExecute = GetActivity();

            //configure workflow application
            executionLog.Text = string.Empty;
            //consoleOutput.Text = string.Empty;
            _executionLog = new ExecutionLogger();
            disposable.Disposable = _executionLog.Messages.Subscribe(InjectExecutionLog);
            
            _wfApp = new WorkflowApplication(activityExecute);
            _wfApp.Extensions.Add(_executionLog);
            _wfApp.Completed = WfExecutionCompleted;
            _wfApp.Aborted = WfAborted;

            //execute 
            _wfApp.Run();
        }

        private void WfAborted(WorkflowApplicationAbortedEventArgs obj)
        {
            _executionLog?.ClearStates();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var m = new ExecutionMessage() { ExecutionState = "WorkflowStop", Sender = Name, ParentNames = new string[0] };
                subject.OnNext(m);
                var s = string.Format("[{0}] [{1}] [{2}]",
                        DateTime.Now.ToString("HH:mm:ss"),
                        m.Sender,
                        m.ExecutionState);
                LogExecution(s);
            }));
        }

        private Activity GetActivity()
        {
            //get workflow source from designer
            _wfDesigner.Flush();
            var workflowStream = new MemoryStream(Encoding.UTF8.GetBytes(_wfDesigner.Text));

            var settings = new ActivityXamlServicesSettings()
            {
                CompileExpressions = true
            };

            return ActivityXamlServices.Load(workflowStream, settings);
        }

        Queue<string> executionLogQueue = new Queue<string>();

        private void InjectExecutionLog(ExecutionMessage m)
        {
            var recordEntry = m.TrackingRecord;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var s = string.Format("[{0}] [{1}] [{2}]",
                    recordEntry.EventTime.ToLocalTime().ToString("HH:mm:ss"),
                    m.Address,
                    recordEntry.State);
                LogExecution(s);
            }));
            subject.OnNext(m);
        }

        private void LogExecution(string s)
        {
            executionLogQueue.Enqueue(s);
            if (executionLogQueue.Count > MaxExecutionLogLines)
                executionLogQueue.Dequeue();

            executionLog.Text = string.Join(Environment.NewLine, executionLogQueue);
            executionLog.ScrollToEnd();
        }

        /// <summary>
        /// Stops the Current Workflow
        /// </summary>
        private void CmdWorkflowStop(object sender, ExecutedRoutedEventArgs e)
        {
            //manual stop
            _wfApp?.Abort("Stopped by User");
        }

        /// <summary>
        /// Save the current state of a Workflow
        /// </summary>
        private void CmdWorkflowSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (_currentWorkflowFile == String.Empty)
            {
                var dialogSave = new SaveFileDialog();
                dialogSave.Title = "Save Workflow";
                dialogSave.Filter = "Workflows (.xaml)|*.xaml";

                if (dialogSave.ShowDialog() == true)
                {
                    SaveAs(dialogSave.FileName);
                }
            }
            else
            {
                Save();
            }
        }


        /// <summary>
        /// Creates a new Workflow Designer instance and loads the Default Workflow 
        /// </summary>
        private void CmdWorkflowNew(object sender, ExecutedRoutedEventArgs e)
        {
            SetDesigner();
        }


        /// <summary>
        /// Loads a Workflow into a new Workflow Designer instance
        /// </summary>
        private void CmdWorkflowOpen(object sender, ExecutedRoutedEventArgs e)
        {
            var dialogOpen = new OpenFileDialog();
            dialogOpen.Title = "Open Workflow";
            dialogOpen.Filter = "Workflows (.xaml)|*.xaml";

            if (dialogOpen.ShowDialog() == true)
            {
                var prev = FilePath;
                FilePath = dialogOpen.FileName;
                Load();
                FilePath = prev;
            }
        }

        /// <summary>
        /// Retrieves all Workflow Activities from the loaded assemblies and inserts them into a ToolboxControl 
        /// </summary>
        private void InitializeActivitiesToolbox()
        {
            try
            {
                _wfToolbox = new ToolboxControl();

                // Create a category.  
                var stateMachineCategory = new ToolboxCategory("StateMachine");

                // Create Toolbox items.  
                var tool1 = new ToolboxItemWrapper("System.Activities.Statements.StateMachine",
                    typeof(StateMachine).Assembly.FullName, null, "StateMachine");

                var tool2 = new ToolboxItemWrapper("System.Activities.Statements.State",
                    typeof(State).Assembly.FullName, null, "State");

                var tool3 = new ToolboxItemWrapper("System.Activities.Core.Presentation.FinalState",
                    typeof(FinalState).Assembly.FullName, null, "FinalState");

                // Add the Toolbox items to the category.  
                stateMachineCategory.Add(tool1);
                stateMachineCategory.Add(tool2);
                stateMachineCategory.Add(tool3);

                // Create a category.  
                var primitiveCategory = new ToolboxCategory("Primitive");

                // Create Toolbox items.  
                var pTool1 = new ToolboxItemWrapper("System.Activities.Statements.Assign",
                    typeof(Assign).Assembly.FullName, null, "Assign");

                var pTool2 = new ToolboxItemWrapper("System.Activities.Statements.Delay",
                    typeof(Delay).Assembly.FullName, null, "Delay");

                var pTool3 = new ToolboxItemWrapper("ActivityLibrary.Wait",
                    typeof(ActivityLibrary.Wait).Assembly.FullName, null, "Wait");

                var pTool4 = new ToolboxItemWrapper("ActivityLibrary.WaitFor",
                    typeof(ActivityLibrary.WaitFor).Assembly.FullName, null, "WaitFor");

                var pTool5 = new ToolboxItemWrapper("System.Activities.Statements.If",
                   typeof(If).Assembly.FullName, null, "If");

                //var pTool5 = new ToolboxItemWrapper("System.Activities.Statements.WriteLine",
                //    typeof(WriteLine).Assembly.FullName, null, "WriteLine");

                // Add the Toolbox items to the category.  
                primitiveCategory.Add(pTool1);
                primitiveCategory.Add(pTool2);
                primitiveCategory.Add(pTool3);
                primitiveCategory.Add(pTool4);
                primitiveCategory.Add(pTool5);

                // Add the category to the ToolBox control.  
                _wfToolbox.Categories.Add(stateMachineCategory);
                _wfToolbox.Categories.Add(primitiveCategory);

                WfToolboxBorder.Child = _wfToolbox;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// Retrieve Workflow Execution Logs and Workflow Execution Outputs
        /// </summary>
        private void WfExecutionCompleted(WorkflowApplicationCompletedEventArgs ev)
        {
            try
            {
                //retrieve & display execution output
                foreach (var item in ev.Outputs)
                {
                    //consoleOutput.Dispatcher.Invoke(
                    //    System.Windows.Threading.DispatcherPriority.Normal,
                    //    new Action(
                    //        delegate ()
                    //        {
                    //            consoleOutput.Text += String.Format("[{0}] {1}" + Environment.NewLine, item.Key, item.Value);
                    //        }
                    //));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
