using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RehostedWorkflowDesigner
{
    public interface IStateMachineController
    {
        string Name { get; set; }
        string FilePath { get; set; }

        void SendMessage(string message, object value);

        IObservable<ExecutionMessage> Messages { get; }
        IReadOnlyDictionary<string, bool> EnteredStates { get; }

        void Start();
        void Stop();
        void Load();
        void Save();
    }
}
