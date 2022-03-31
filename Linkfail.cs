using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLPR
{
    public partial class Linkfail : Form
    {
        public Linkfail()
        {
            InitializeComponent();
        }

        private void cmdExit_Click(object sender, EventArgs e)
        {
            this.Hide();

        }

        private void Linkfail_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            externalDisplay();
            
        }
        public void externalDisplay()
        {
            if (Screen.AllScreens.Length > 1)
            {

                this.Location = new Point(1920+560, 340);

            }
            else if (Screen.AllScreens.Length == 1)
            {
                this.Location = new Point(560, 340);
            }
        }

    }
}
