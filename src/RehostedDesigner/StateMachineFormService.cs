using RehostedWorkflowDesigner.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RehostedWorkflowDesigner
{
    public static class StateMachineService
    {
        static readonly Lazy<StateMachineForm> lazy = new Lazy<StateMachineForm>(() => new StateMachineForm());

        /// <summary>
        /// Gets the current workflow form Instance
        /// </summary>
        public static StateMachineForm Instance => lazy.Value;

        public static IStateMachineController AddStateMachine(string name = null)
        {
            if (!Instance.Visible)
                Instance.Show();

            var result = Instance.AddMachine(name);

            return result;
        }

        public static void RemoveStateMachine(IStateMachineController stateMachine)
        {
            Instance.RemoveMachine(stateMachine);
        }
    }
}
