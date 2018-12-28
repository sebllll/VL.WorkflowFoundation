using Microsoft.CSharp.Activities;
using RehostedWorkflowDesigner.CSharpExpressionEditor;
using System;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xaml;
using System.Xml;

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

            var wfDesigner = new WorkflowDesigner();
            var dcs = wfDesigner.Context.Services.GetService<DesignerConfigurationService>();
            dcs.TargetFrameworkName = new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 7, 2));
            dcs.LoadingFromUntrustedSourceEnabled = true;
            wfDesigner.Context.Services.Publish<IExpressionEditorService>(expressionEditorService);

            //associates all of the basic activities with their designers
            new DesignerMetadata().Register();

            string temp = File.ReadAllText(Path.Combine(AssemblyDirectory, @"colors.xaml"));

            StringReader reader = new StringReader(temp);
            XmlReader xmlReader = XmlReader.Create(reader);
            ResourceDictionary fontAndColorDictionary = (ResourceDictionary)System.Windows.Markup.XamlReader.Load(xmlReader);

            //var keys = GetColorKeys();

            //foreach (var key in keys)
            //{
            //    fontAndColorDictionary[key] = Brushes.Pink;
            //}

            Hashtable hashTable = new Hashtable();
            foreach (var key in fontAndColorDictionary.Keys)
            {
                hashTable.Add(key, fontAndColorDictionary[key]);
            }

            wfDesigner.PropertyInspectorFontAndColorData = XamlServices.Save(hashTable);

            //load Workflow Xaml
            wfDesigner.Load(sourceFile);
            //var g = wfDesigner.View as Grid;
            //g.Background = Brushes.Pink;
            return wfDesigner;
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

        public static IEnumerable<string> GetColorKeys()
        {
            return typeof(WorkflowDesignerColors).GetFields(BindingFlags.Public | BindingFlags.Static)
                      .Where(f => f.FieldType == typeof(string) && f.Name.EndsWith("ColorKey") || f.Name.EndsWith("GradientBeginKey") || f.Name.EndsWith("GradientEndKey")).Select(fi => fi.GetValue(null)).Cast<string>();
        }
    }
}
