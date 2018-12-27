using Microsoft.CSharp.Activities;
using RehostedWorkflowDesigner.CSharpExpressionEditor;
using System;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.Activities.Statements;

namespace RehostedWorkflowDesigner
{
    /// <summary>
    /// Workflow Designer Wrapper
    /// </summary>
    public static class CustomWfDesigner
    {
        private const string _defaultActivity = "defaultActivity.xaml";

        /// <summary>
        /// Creates a new Workflow Designer instance with C# Expression Editor
        /// </summary>
        /// <param name="sourceFile">Workflow FileName</param>
        public static WorkflowDesigner NewInstance(string sourceFile = _defaultActivity)
        {
            if (string.IsNullOrWhiteSpace(sourceFile))
                sourceFile = _defaultActivity;

            var expressionEditorService = new RoslynExpressionEditorService();
            ExpressionTextBox.RegisterExpressionActivityEditor(new CSharpValue<string>().Language, typeof(RoslynExpressionEditor), CSharpExpressionHelper.CreateExpressionFromString);            

            var _wfDesigner = new WorkflowDesigner();
            _wfDesigner.Context.Services.GetService<DesignerConfigurationService>().TargetFrameworkName = new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 7, 2));
            _wfDesigner.Context.Services.GetService<DesignerConfigurationService>().LoadingFromUntrustedSourceEnabled = true;
            _wfDesigner.Context.Services.Publish<IExpressionEditorService>(expressionEditorService);

            //associates all of the basic activities with their designers
            new DesignerMetadata().Register();

            //load Workflow Xaml
            _wfDesigner.Load(sourceFile);
            return _wfDesigner;
        }
    }
}
