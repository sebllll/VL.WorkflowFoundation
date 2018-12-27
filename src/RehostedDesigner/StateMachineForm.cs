using RehostedWorkflowDesigner.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RehostedWorkflowDesigner
{
    public partial class StateMachineForm : Form
    {
        

        internal StateMachineForm()
        {
            InitializeComponent();
        }

        internal IStateMachineController AddMachine(string name = null)
        {
            return stateMachineTabs1.AddMachine(name);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            e.Cancel = true;
            Hide();
        }

        internal void RemoveMachine(IStateMachineController stateMachine)
        {
            stateMachineTabs1.RemoveMachine(stateMachine);
        }
    }
}
