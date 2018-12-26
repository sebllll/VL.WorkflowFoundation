using Microsoft.CSharp.Activities;
using RehostedWorkflowDesigner.CSharpExpressionEditor;
using System;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.Activities.Statements;

namespace RehostedWorkflowDesigner.Helpers
{
    /// <summary>
    /// Workflow Designer Wrapper
    /// </summary>
    public static class CustomWfDesigner
    {
        private const string _defaultActivity = "defaultActivity.xaml";
        private static RoslynExpressionEditorService _expressionEditorService;

        /// <summary>
        /// Creates a new Workflow Designer instance with C# Expression Editor
        /// </summary>
        /// <param name="sourceFile">Workflow FileName</param>
        public static WorkflowDesigner NewInstance(string sourceFile = _defaultActivity)
        {
            _expressionEditorService = new RoslynExpressionEditorService();
            ExpressionTextBox.RegisterExpressionActivityEditor(new CSharpValue<string>().Language, typeof(RoslynExpressionEditor), CSharpExpressionHelper.CreateExpressionFromString);            

            var _wfDesigner = new WorkflowDesigner();
            _wfDesigner.Context.Services.GetService<DesignerConfigurationService>().LoadingFromUntrustedSourceEnabled = true;
            _wfDesigner.Context.Services.Publish<IExpressionEditorService>(_expressionEditorService);

            //associates all of the basic activities with their designers
            new DesignerMetadata().Register();

            //load Workflow Xaml
            _wfDesigner.Load(sourceFile);
            return _wfDesigner;
        }
    }
}
