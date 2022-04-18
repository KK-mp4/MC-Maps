using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCMaps
{
    public partial class UserControl2 : UserControl
    {
        public UserControl2()
        {
            InitializeComponent();
        }

        private void UserControl2_Load(object sender, EventArgs e)
        {
            X.Text = Properties.Settings.Default.x.ToString();
            Y.Text = Properties.Settings.Default.y.ToString();
            H.Text = Properties.Settings.Default.h.ToString();
            trackBar2.Value = Properties.Settings.Default.thresholdTop;
            trackBar1.Value = Properties.Settings.Default.thresholdBottom;

            label7.Text = trackBar1.Value.ToString();
            label9.Text = trackBar2.Value.ToString();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label7.Text = trackBar1.Value.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label9.Text = trackBar2.Value.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int x = Convert.ToInt32(X.Text);
                int y = Convert.ToInt32(Y.Text);
                int h = Convert.ToInt32(H.Text);
                int thresholdTop = trackBar2.Value;
                int thresholdBottom = trackBar1.Value;

                Form1 myParent = (Form1)this.Parent;
                myParent.x = x;
                myParent.y = y;
                myParent.height = h;
                myParent.thresholdTop = thresholdTop;
                myParent.thresholdBottom = thresholdBottom;
            }
            catch
            {
                MessageBox.Show("Invalid input", "Error");
            }
        }
    }
}
