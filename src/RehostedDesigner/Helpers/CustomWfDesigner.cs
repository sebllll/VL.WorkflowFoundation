using Microsoft.CSharp.Activities;
using RehostedWorkflowDesigner.CSharpExpressionEditor;
using System;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.IO;
using System.Reflection;

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
                sourceFile = Path.Combine(AssemblyDirectory, _defaultActivity);

            var expressionEditorService = new RoslynExpressionEditorService();
            ExpressionTextBox.RegisterExpressionActivityEditor(new CSharpValue<string>().Language, typeof(RoslynExpressionEditor), CSharpExpressionHelper.CreateExpressionFromString);            

            var _wfDesigner = new WorkflowDesigner();
            _wfDesigner.Context.Services.GetService<DesignerConfigurationService>().TargetFrameworkName = new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 6, 1));
            _wfDesigner.Context.Services.GetService<DesignerConfigurationService>().LoadingFromUntrustedSourceEnabled = true;
            _wfDesigner.Context.Services.Publish<IExpressionEditorService>(expressionEditorService);

            //associates all of the basic activities with their designers
            new DesignerMetadata().Register();

            //load Workflow Xaml
            _wfDesigner.Load(sourceFile);
            return _wfDesigner;
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
