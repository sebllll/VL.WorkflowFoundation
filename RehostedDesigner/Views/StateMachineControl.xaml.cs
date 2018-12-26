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
using RehostedWorkflowDesigner.Helpers;
using System.ComponentModel;
using System.Timers;
using System.Activities.Presentation;
using System.Windows.Controls;
using System.Activities.Statements;
using System.Activities.Core.Presentation;
using ActivityLibrary;
using System.Activities.Presentation.Model;

namespace RehostedWorkflowDesigner.Views
{
    /// <summary>
    /// Interaction logic for StateMachineTab.xaml
    /// </summary>
    public partial class StateMachineControl : UserControl
    {
        private WorkflowApplication _wfApp;
        private ToolboxControl _wfToolbox;
        private CustomTrackingParticipant _executionLog;
        private WorkflowDesigner _wfDesigner;
        ModelTreeManager modelTreeManager;

        private string _currentWorkflowFile = string.Empty;
        private Timer _timer;


        public StateMachineControl()
        {
            InitializeComponent();
            _timer = new Timer(1000);
            _timer.Enabled = false;
            _timer.Elapsed += TrackingDataRefresh;

            //load all available workflow activities from loaded assemblies 
            InitializeActivitiesToolbox();

            _wfDesigner = CustomWfDesigner.NewInstance();

            modelTreeManager = _wfDesigner.Context.Services.GetService<ModelTreeManager>();

            //initialize designer
            WfDesignerBorder.Child = _wfDesigner.View;
            WfPropertyBorder.Child = _wfDesigner.PropertyInspectorView;

        }

        public string WorkflowName
        {
            get
            {
                return (string)modelTreeManager.Root.Properties["Name"].ComputedValue;
            }
            set
            {
                modelTreeManager.Root.Properties["Name"].ComputedValue = value;
            }
        }

        public string ExecutionLog
        {
            get
            {
                if (_executionLog != null)
                    return _executionLog.TrackData;
                else
                    return string.Empty;
            }
            set { _executionLog.TrackData = value; NotifyPropertyChanged("ExecutionLog"); }
        }


        private void TrackingDataRefresh(Object source, ElapsedEventArgs e)
        {
            NotifyPropertyChanged("ExecutionLog");
        }


        private void consoleExecutionLog_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            consoleExecutionLog.ScrollToEnd();
        }


        /// <summary>
        /// show execution log in ui
        /// </summary>
        private void UpdateTrackingData()
        {
            //retrieve & display execution log
            //consoleExecutionLog.Dispatcher.Invoke(
            //    System.Windows.Threading.DispatcherPriority.Normal,
            //    new Action(
            //        delegate ()
            //        {
            //            //consoleExecutionLog.Text = _executionLog.TrackData;
            NotifyPropertyChanged("ExecutionLog");
            //        }
            //));
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

                var pTool2 = new ToolboxItemWrapper("ActivityLibrary.Wakeup",
                    typeof(Wakeup).Assembly.FullName, null, "Wakeup");

                // Add the Toolbox items to the category.  
                primitiveCategory.Add(pTool1);
                primitiveCategory.Add(pTool2);

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
                //retrieve & display execution log
                _timer.Stop();
                UpdateTrackingData();

                //retrieve & display execution output
                foreach (var item in ev.Outputs)
                {
                    consoleOutput.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                            delegate ()
                            {
                                consoleOutput.Text += String.Format("[{0}] {1}" + Environment.NewLine, item.Key, item.Value);
                            }
                    ));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        /// <summary>
        /// Creates a new Workflow Application instance and executes the Current Workflow
        /// </summary>
        private void CmdWorkflowRun(object sender, ExecutedRoutedEventArgs e)
        {
            //get workflow source from designer
            _wfDesigner.Flush();
            MemoryStream workflowStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(_wfDesigner.Text));

            ActivityXamlServicesSettings settings = new ActivityXamlServicesSettings()
            {
                CompileExpressions = true
            };

            DynamicActivity activityExecute = ActivityXamlServices.Load(workflowStream, settings) as DynamicActivity;

            //configure workflow application
            consoleExecutionLog.Text = String.Empty;
            consoleOutput.Text = String.Empty;
            _executionLog = new CustomTrackingParticipant();
            _wfApp = new WorkflowApplication(activityExecute);
            _wfApp.Extensions.Add(_executionLog);
            _wfApp.Completed = WfExecutionCompleted;

            //execute 
            _wfApp.Run();

            //enable timer for real-time logging
            _timer.Start();
        }

        /// <summary>
        /// Stops the Current Workflow
        /// </summary>
        private void CmdWorkflowStop(object sender, ExecutedRoutedEventArgs e)
        {
            //manual stop
            if (_wfApp != null)
            {
                _wfApp.Abort("Stopped by User");
                _timer.Stop();
                UpdateTrackingData();
            }

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
                    _wfDesigner.Save(dialogSave.FileName);
                    _currentWorkflowFile = dialogSave.FileName;
                }
            }
            else
            {
                _wfDesigner.Save(_currentWorkflowFile);
            }
        }


        /// <summary>
        /// Creates a new Workflow Designer instance and loads the Default Workflow 
        /// </summary>
        private void CmdWorkflowNew(object sender, ExecutedRoutedEventArgs e)
        {
            _currentWorkflowFile = String.Empty;
            _wfDesigner = CustomWfDesigner.NewInstance();
            WfDesignerBorder.Child = _wfDesigner.View;
            WfPropertyBorder.Child = _wfDesigner.PropertyInspectorView;
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
                using (var file = new StreamReader(dialogOpen.FileName, true))
                {
                    _wfDesigner = CustomWfDesigner.NewInstance(dialogOpen.FileName);
                    WfDesignerBorder.Child = _wfDesigner.View;
                    WfPropertyBorder.Child = _wfDesigner.PropertyInspectorView;

                    _currentWorkflowFile = dialogOpen.FileName;
                }
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
