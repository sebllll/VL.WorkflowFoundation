using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RehostedWorkflowDesigner
{
    public static class ConsoleOutput
    {
        public static void AddTextBox(TextBox textBox)
        {
            var tbr = new TextBoxTextWriter(textBox);
            var ctr = Console.Out as ConsoleTextWriter;
            if(ctr != null)
                Console.SetOut(new ConsoleTextWriter(ctr.writers.Concat(new[] { tbr })));
            else
                Console.SetOut(new ConsoleTextWriter(tbr, Console.Out));
        }
    }

    public class ConsoleTextWriter : TextWriter
    {
        public IEnumerable<TextWriter> writers;
        public ConsoleTextWriter(IEnumerable<TextWriter> writers)
        {
            this.writers = writers.ToList();
        }
        public ConsoleTextWriter(params TextWriter[] writers)
        {
            this.writers = writers;
        }

        public override void Write(char value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void Write(string value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void Flush()
        {
            foreach (var writer in writers)
                writer.Flush();
        }

        public override void Close()
        {
            foreach (var writer in writers)
                writer.Close();
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }

    public class TextBoxTextWriter : TextWriter
    {
        TextBox textBox = null;

        public TextBoxTextWriter(TextBox output)
        {
            textBox = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value.ToString());
            }));
        }

        public override void Write(string value)
        {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value);
            }));
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
