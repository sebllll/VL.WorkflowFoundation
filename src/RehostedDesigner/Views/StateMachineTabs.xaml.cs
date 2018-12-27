using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
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
            var culture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            InitializeComponent();
        }

        public IStateMachineController AddMachine(string name = null)
        {
            name = string.IsNullOrWhiteSpace(name) ? "Machine " + (MachineTabs.Items.Count + 1) : name;
            var machineControl = new StateMachineControl() { Name = name };
            var tab = new TabItem() { Header = name, Content = machineControl };
            MachineTabs.SelectedIndex = MachineTabs.Items.Add(tab);
            return machineControl;
        }

        internal void RemoveMachine(IStateMachineController stateMachine)
        {
            var tab = MachineTabs.Items.OfType<TabItem>().FirstOrDefault(t => t.Content == stateMachine);
            if (tab != null)
            {
                MachineTabs.Items.Remove(tab); 
            }
        }
    }
}
