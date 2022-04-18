using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCMaps
{
    public partial class UserControl1 : UserControl
    {
        private Bitmap map = null;

        private List<PictureBox> markerList = new List<PictureBox>();

        private List<string> pathList = new List<string>();

        public UserControl1()
        {
            InitializeComponent();
        }

        private void bOpenMap_Click(object sender, EventArgs e)//loads map to pictureBox1
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"../../../";
            openFileDialog.Filter = "Images (png, jpg) |*.png;*.jpg|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                map = new Bitmap(openFileDialog.FileName);
                pictureBox1.Image = map;
            }
        }

        private void button1_Click(object sender, EventArgs e)//calls marker rendering
        {
            for (int i = 0; i < markerList.Count; ++i)
            {
                this.Controls.Remove(markerList[i]);
                markerList.Remove(markerList[i]);
                pathList.RemoveAt(i);
            }
            markerList.Clear();
            pathList.Clear();

            Form1 myParent = (Form1)this.Parent;
            for (int i = 0; i < myParent.coordList.Count; ++i)
                MakePictureBox(myParent.coordList[i].point, i);
        }

        private void MakePictureBox(Point point, int index)//renders markers
        {
            PictureBox newMarker = new PictureBox();
            newMarker.Width = 10;
            newMarker.Height = 10;
            newMarker.BackColor = Color.FromArgb(255, 0, 144);
            newMarker.Image = new Bitmap(@"..\..\..\Icons\marker.png");

            try
            {
                int originalXmin = Convert.ToInt32(xmin.Text);
                int originalXmax = Convert.ToInt32(xmax.Text);

                int originalYmin = Convert.ToInt32(zmin.Text);
                int originalYmax = Convert.ToInt32(zmax.Text);

                Size result = GetDisplayedImageSize(pictureBox1);
                float mapXperX = ((float)(originalXmax - originalXmin) / result.Width);
                float mapYperY = ((float)(originalYmax - originalYmin) / result.Height);
                int x = Convert.ToInt32(((float)(point.X - originalXmin) / mapXperX) - (float)newMarker.Width / 2);
                int y = Convert.ToInt32(((float)(point.Y - originalYmin) / mapYperY) - (float)newMarker.Height / 2);

                if (result.Width < pictureBox1.Width)
                    x += Convert.ToInt32((float)(pictureBox1.Width - result.Width) / 2);

                if (result.Height < pictureBox1.Height)
                    y += Convert.ToInt32((float)(pictureBox1.Height - result.Height) / 2);

                if (x > result.Width || x < 0)
                    return;

                if (y > result.Height || y < 0)
                    return;

                newMarker.Location = new Point(x, y);

                newMarker.Click += newMarker_Click;

                markerList.Add(newMarker);
                this.Controls.Add(newMarker);
                Form1 myParent = (Form1)this.Parent;
                pathList.Add(myParent.coordList[index].path);
                newMarker.BringToFront();
            }
            catch
            {
                MessageBox.Show("Mistake in coordinate system input", "Error");
            }     
        }

        private void newMarker_Click(object sender, EventArgs e)//loads selected marker image path
        {
            PictureBox temPic = sender as PictureBox;
            int index = markerList.IndexOf(sender as PictureBox);
            pictureBox2.Image = new Bitmap(pathList[index]);
            label5.Visible = true;
            label5.Text = Path.GetFileName(pathList[index]);
        }

        private void button2_Click(object sender, EventArgs e)//exports marker data
        {
            this.Cursor = Cursors.WaitCursor;
            using (StreamWriter writer = new StreamWriter(@"..\..\..\marker_data.txt"))
            {
                Form1 myParent = (Form1)this.Parent;
                for (int i = 0; i < myParent.coordList.Count; ++i)
                    writer.WriteLine($"{myParent.coordList[i].path}\t{myParent.coordList[i].point}");
            }
            this.Cursor = Cursors.Default;
        }

        #region helperfunctions

        private Size GetDisplayedImageSize(PictureBox pictureBox)//gets pictureBox display dimentions
        {
            Size containerSize = pictureBox.ClientSize;
            float containerAspectRatio = (float)containerSize.Height / (float)containerSize.Width;
            Size originalImageSize = pictureBox.Image.Size;
            float imageAspectRatio = (float)originalImageSize.Height / (float)originalImageSize.Width;

            Size result = new Size();
            if (containerAspectRatio > imageAspectRatio)
            {
                result.Width = containerSize.Width;
                result.Height = (int)(imageAspectRatio * (float)containerSize.Width);
            }
            else
            {
                result.Height = containerSize.Height;
                result.Width = (int)((1.0f / imageAspectRatio) * (float)containerSize.Height);
            }
            return result;
        }

        private void UserControl1_Resize(object sender, EventArgs e)//clears markers when resizing
        {
            for (int i = 0; i < markerList.Count; ++i)
            {
                this.Controls.Remove(markerList[i]);
                markerList.Remove(markerList[i]);
            }
        }
        #endregion
    }
}
