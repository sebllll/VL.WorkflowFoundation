using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RehostedWorkflowDesigner.Views
{
    /// <summary>
    /// Interaction logic for StateMachineTabs.xaml
    /// </summary>
    public partial class StateMachineTabs : UserControl
    {
        public StateMachineTabs()
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
    }
}
