using RehostedWorkflowDesigner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RehostTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LatestMachine = StateMachineService.AddStateMachine();
        }

        IStateMachineController LatestMachine;
        private void button1_Click(object sender, EventArgs e)
        {
            LatestMachine = StateMachineService.AddStateMachine();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LatestMachine.Name = textBox1.Text;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LatestMachine.Name = textBox1.Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LatestMachine.ResumeBookmark(textBox2.Text, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StateMachineService.RemoveStateMachine(LatestMachine);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LatestMachine.FilePath = textBox3.Text;
            LatestMachine.Load();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            LatestMachine.FilePath = textBox4.Text;
            LatestMachine.Save();
        }
    }
}
