using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RehostedWorkflowDesigner
{
    public static class StateMachineFormService
    {
        static readonly Lazy<StateMachineForm> lazy = new Lazy<StateMachineForm>(() => new StateMachineForm());

        /// <summary>
        /// Gets the current workflow form Instance
        /// </summary>
        public static StateMachineForm Instance => lazy.Value;

        public static void AddWorkflow(string name = null)
        {
            Instance.AddMachine(name);

            if (!Instance.Visible)
                Instance.Show();
        }
        
    }
}
