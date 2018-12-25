using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace RehostedWorkflowDesigner.Views
{

    public partial class MainWindow : INotifyPropertyChanged
    {

        public MainWindow()
        {
            InitializeComponent();
            AddMachine();
        }

        void AddMachine()
        {
            var machineTab = new StateMachineControl();
            var tab = new TabItem() { Header = "Machine", Content = machineTab };
            MachineTabs.Items.Add(tab);
        }

        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
