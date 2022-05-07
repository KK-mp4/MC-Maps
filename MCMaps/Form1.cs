namespace MCMaps
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Patagames.Ocr;
    using Patagames.Ocr.Enums;

    public partial class Form1 : Form
    {
        private UserControl1 mapUC = new UserControl1();

        private UserControl2 settingsUC = new UserControl2();

        private List<Image> LoadedImages { get; set; }

        public List<string> FilePaths = new List<string>();

        public static Bitmap Image = null;

        public List<Coordinates> CoordList = new List<Coordinates>();

        public int X = 0;

        public int Y = 0;

        public int Height = 0;

        public int ThresholdTop = 0;

        public int ThresholdBottom = 0;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadedImages = new List<Image>();

            GetSettings();

            mapUC.Dock = DockStyle.Fill;
            this.Controls.Add(mapUC);
            mapUC.BringToFront();
            mapUC.Hide();

            settingsUC.Dock = DockStyle.Fill;
            this.Controls.Add(settingsUC);
            settingsUC.BringToFront();
            settingsUC.Hide();
        }

        private void bCropBinaraze_Click(object sender, EventArgs e)//crops and binirizes
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                this.Cursor = Cursors.WaitCursor;

                var selectedIndex = listView1.SelectedIndices[0];
                var img = new Bitmap(LoadedImages[LoadedImages.Count - 1 - selectedIndex]);

                Rectangle rect = new Rectangle(X, Y, 499, Height);
                Bitmap imgCropped = img.Clone(rect, img.PixelFormat);
                img.Dispose();

                int w = imgCropped.Width;
                int h = imgCropped.Height;

                byte[] img_bytes = GetRGBValues(imgCropped);

                int imglength = w * h * 4;

                byte[] img_out_bytes = new byte[imglength];

                for (int i = 0; i < imglength - 2; i += 4)
                {
                    if ((img_bytes[i + 2] >= ThresholdBottom && img_bytes[i + 1] >= ThresholdBottom && img_bytes[i] >= ThresholdBottom) && (img_bytes[i + 2] <= ThresholdTop && img_bytes[i + 1] <= ThresholdTop && img_bytes[i] <= ThresholdTop))
                    {
                        img_out_bytes[i + 2] = 0;
                        img_out_bytes[i + 1] = 0;
                        img_out_bytes[i] = 0;
                    }
                    else
                    {
                        img_out_bytes[i + 2] = 255;
                        img_out_bytes[i + 1] = 255;
                        img_out_bytes[i] = 255;
                    }
                }

                Bitmap img_out = new Bitmap(w, h, PixelFormat.Format32bppRgb);
                writeImageBytes(img_out, img_out_bytes);

                pictureBox1.Image = img_out;
                SavetoLayerList(img_out);

                this.Cursor = Cursors.Default;
                timer.Stop();
                debug.Text = "Last calculation time: " + timer.ElapsedMilliseconds + " ms. or " + Math.Round(timer.Elapsed.TotalSeconds, 3) + " s.";
            }
            else
            {
                MessageBox.Show("Image is not selected", "Error");
            }
        }

        private void bGetText_Click(object sender, EventArgs e)//gets text
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                this.Cursor = Cursors.WaitCursor;

                var selectedIndex = listView1.SelectedIndices[0];
                var img = new Bitmap(LoadedImages[LoadedImages.Count - 1 - selectedIndex]);

                using (var objOcr = OcrApi.Create())
                {
                    //objOcr.Init(Patagames.Ocr.Enums.Languages.English);

                    //Thanks to Hayden Carpenter for MC font training data: https://codedecatur.org/tutorials/hayden-carpenter/minecraft-ocr-with-pytesseract
                    objOcr.Init(@"..\..\tessdata", "mc", OcrEngineMode.OEM_DEFAULT);
                    objOcr.SetVariable("tessedit_char_whitelist", "0123456789,/ -");
                    string returnText = null;
                    try
                    {
                        returnText += objOcr.GetTextFromImage(img);
                        char[] xyz = returnText.ToCharArray();
                        string x = null;
                        string y = null;
                        int i = 0;
                        foreach (char ch in xyz)
                        {
                            if (ch != ' ')
                            {
                                if (i == 0)
                                {
                                    x += ch;
                                }

                                if (i == 4)
                                {
                                    y += ch;
                                }
                            }
                            else
                            {
                                i++;
                            }
                        }

                        if (x == null || y == null)
                        {
                            img.Dispose();
                            this.Cursor = Cursors.Default;
                            timer.Stop();
                            return;
                        }

                        int intx = Convert.ToInt32(Convert.ToDouble(x));
                        int inty = Convert.ToInt32(Convert.ToDouble(y));
                        Point newPoint = new Point(intx, inty);
                        Coordinates newCoord = new Coordinates() { Point = newPoint, Path = FilePaths[LoadedImages.Count - 1 - selectedIndex] };
                        CoordList.Add(newCoord);
                    }
                    catch
                    {
                        img.Dispose();
                        this.Cursor = Cursors.Default;
                        timer.Stop();
                        MessageBox.Show("Tesseract.Net.SDK error.\nProbably forgot to crop...", "Error");
                        return;
                    }

                    textBox1.Text = returnText;
                }

                img.Dispose();

                this.Cursor = Cursors.Default;
                timer.Stop();
                debug.Text = "Last calculation time: " + timer.ElapsedMilliseconds + " ms. or " + Math.Round(timer.Elapsed.TotalSeconds, 3) + " s.";
            }
            else
            {
                MessageBox.Show("Image is not selected", "Error");
            }
        }

        private void processBatchToolStripMenuItem_Click(object sender, EventArgs e)//batch processing start
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                this.Cursor = Cursors.WaitCursor;

                var selectedDirectory = folderBrowser.SelectedPath;
                var imagePaths = Directory.GetFiles(selectedDirectory);

                //foreach (string path in imagePaths)
                //{
                //    process(path);
                //}

                Parallel.For(0, imagePaths.Length, i =>
                {
                    process(imagePaths[i]);
                });

                this.Cursor = Cursors.Default;
                timer.Stop();
                debug.Text = "Last calculation time: " + timer.ElapsedMilliseconds + " ms. or " + Math.Round(timer.Elapsed.TotalSeconds, 3) + " s.";

            }
        }

        private void process(string path)//processes folder of images
        {
            Bitmap img;
            try
            {
                img = new Bitmap(path);
            }
            catch
            {
                MessageBox.Show("Folder contains non image files", "Error");
                return;
            }

            Rectangle rect = new Rectangle(X, Y, 499, Height);
            Bitmap imgCropped = img.Clone(rect, PixelFormat.Format32bppRgb);
            img.Dispose();

            int w = imgCropped.Width;
            int h = imgCropped.Height;

            byte[] img_bytes = GetRGBValues(imgCropped);
            imgCropped.Dispose();

            int imglength = w * h * 4;

            byte[] img_out_bytes = new byte[imglength];

            for (int i = 0; i < imglength - 3; i += 4)
            {
                if ((img_bytes[i + 2] >= ThresholdBottom && img_bytes[i + 1] >= ThresholdBottom && img_bytes[i] >= ThresholdBottom) && (img_bytes[i + 2] <= ThresholdTop && img_bytes[i + 1] <= ThresholdTop && img_bytes[i] <= ThresholdTop))
                {
                    img_out_bytes[i + 2] = 0;
                    img_out_bytes[i + 1] = 0;
                    img_out_bytes[i] = 0;
                }
                else
                {
                    img_out_bytes[i + 2] = 255;
                    img_out_bytes[i + 1] = 255;
                    img_out_bytes[i] = 255;
                }
            }

            Bitmap img_out = new Bitmap(w, h, PixelFormat.Format32bppRgb);
            writeImageBytes(img_out, img_out_bytes);

            using (var objOcr = OcrApi.Create())
            {
                //Thanks to Hayden Carpenter for MC font training data: https://codedecatur.org/tutorials/hayden-carpenter/minecraft-ocr-with-pytesseract
                objOcr.Init(@"..\..\tessdata", "mc", OcrEngineMode.OEM_DEFAULT);
                objOcr.SetVariable("tessedit_char_whitelist", "0123456789,/ -");
                string returnText = null;
                try
                {
                    returnText += objOcr.GetTextFromImage(img_out);
                    char[] xyz = returnText.ToCharArray();
                    string x = null;
                    string y = null;
                    int i = 0;
                    foreach (char ch in xyz)
                    {
                        if (ch != ' ')
                        {
                            if (i == 0)
                            {
                                x += ch;
                            }

                            if (i == 4)
                            {
                                y += ch;
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }

                    if (x == null || y == null)
                    {
                        img.Dispose();
                        this.Cursor = Cursors.Default;
                        return;
                    }

                    int intx = Convert.ToInt32(Convert.ToDouble(x));
                    int inty = Convert.ToInt32(Convert.ToDouble(y));
                    Point newPoint = new Point(intx, inty);
                    Coordinates newCoord = new Coordinates() { Point = newPoint, Path = path };
                    FilePaths.Add(path);
                    CoordList.Add(newCoord);
                }
                catch
                {
                    img_out.Dispose();
                    return;
                }
            }

            img_out.Dispose();
        }

        private void SavetoLayerList(Bitmap img_out)//reloads listBox
        {
            this.Cursor = Cursors.WaitCursor;
            LoadedImages.Add(img_out);

            var selectedIndex = listView1.SelectedIndices[0];
            int invertedIndex = LoadedImages.Count - 1 - selectedIndex;
            FilePaths.Add(FilePaths[invertedIndex - 1]);
            listView1.Items.Clear();

            ImageList images = new ImageList();
            images.ImageSize = new Size(80, 45);
            foreach (var image in LoadedImages)
            {
                images.Images.Add(image);
            }

            listView1.LargeImageList = images;

            for (int itemIndex = LoadedImages.Count - 1; itemIndex >= 0; --itemIndex)
            {
                listView1.Items.Add(new ListViewItem($"Image {itemIndex}", itemIndex));
            }

            this.Cursor = Cursors.Default;
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)//opens folder of images
        {
            if (LoadedImages.Count == 0)
            {
                FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    var selectedDirectory = folderBrowser.SelectedPath;
                    var imagePaths = Directory.GetFiles(selectedDirectory);
                    LoadImagesFromFolder(imagePaths);

                    ImageList images = new ImageList();
                    images.ImageSize = new Size(80, 45);

                    foreach (var image in LoadedImages)
                    {
                        images.Images.Add(image);
                    }

                    listView1.LargeImageList = images;

                    for (int itemIndex = LoadedImages.Count - 1; itemIndex >= 0; --itemIndex)
                    {
                        listView1.Items.Add(new ListViewItem($"Image {itemIndex}", itemIndex));
                    }
                }
            }
            else
            {
                FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    var selectedDirectory = folderBrowser.SelectedPath;
                    var imagePaths = Directory.GetFiles(selectedDirectory);
                    LoadImagesFromFolder(imagePaths);

                    listView1.Items.Clear();

                    ImageList images = new ImageList();
                    images.ImageSize = new Size(80, 45);

                    foreach (var image in LoadedImages)
                    {
                        images.Images.Add(image);
                    }

                    listView1.LargeImageList = images;

                    for (int itemIndex = LoadedImages.Count - 1; itemIndex >= 0; --itemIndex)
                    {
                        listView1.Items.Add(new ListViewItem($"Image {itemIndex}", itemIndex));
                    }
                }
            }
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)//opens single image
        {
            if (LoadedImages.Count == 0)
            {
                OpenFileDialog folderBrowser = new OpenFileDialog();
                folderBrowser.InitialDirectory = @"../../../";
                folderBrowser.Filter = "Image Files(**.JPG;*.PNG)|*.JPG;*.PNG|All files (*.*)|*.*";
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string path = folderBrowser.FileName;
                        var tempImage = System.Drawing.Image.FromFile(path);
                        LoadedImages.Add(tempImage);
                        FilePaths.Add(path);

                        ImageList images = new ImageList();
                        images.ImageSize = new Size(80, 45);

                        foreach (var image in LoadedImages)
                        {
                            images.Images.Add(image);
                        }

                        listView1.LargeImageList = images;

                        for (int itemIndex = LoadedImages.Count - 1; itemIndex >= 0; --itemIndex)
                        {
                            listView1.Items.Add(new ListViewItem($"Image {itemIndex}", itemIndex));
                        }
                    }
                    catch
                    {
                        DialogResult rezult = MessageBox.Show("Impossible to open selected file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                OpenFileDialog folderBrowser = new OpenFileDialog();
                folderBrowser.InitialDirectory = @"../../../";
                folderBrowser.Filter = "Image Files(**.JPG;*.PNG)|*.JPG;*.PNG|All files (*.*)|*.*";
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string path = folderBrowser.FileName;
                        var tempImage = System.Drawing.Image.FromFile(path);
                        LoadedImages.Add(tempImage);
                        FilePaths.Add(path);

                        listView1.Items.Clear();

                        ImageList images = new ImageList();

                        images.ImageSize = new Size(80, 45);
                        foreach (var image in LoadedImages)
                        {
                            images.Images.Add(image);
                        }

                        listView1.LargeImageList = images;

                        for (int itemIndex = LoadedImages.Count - 1; itemIndex >= 0; --itemIndex)
                        {
                            listView1.Items.Add(new ListViewItem($"Image {itemIndex}", itemIndex));
                        }
                    }
                    catch
                    {
                        DialogResult rezult = MessageBox.Show("Impossible to open selected file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

        private void LoadImagesFromFolder(string[] paths) //image loading from file
        {
            try
            {
                foreach (var path in paths)
                {
                    var tempImage = System.Drawing.Image.FromFile(path);
                    LoadedImages.Add(tempImage);
                    FilePaths.Add(path);
                }
            }
            catch
            {
                MessageBox.Show("Folder contains non image files", "Error");
                return;
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)//display selected item
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                var selectedIndex = listView1.SelectedIndices[0];
                var invertedIndex = LoadedImages.Count - 1 - selectedIndex;
                Image selectedImage = LoadedImages[invertedIndex];
                pictureBox1.Image = selectedImage;
            }
        }

        #region helperfunctions

        private void pictureBox2_Click_1(object sender, EventArgs e)//copies coordinates to clipboard
        {
            if (textBox1.Text != string.Empty)
            {
                Clipboard.SetText(textBox1.Text);
            }
        }

        private byte[] GetRGBValues(Bitmap bmp)//converts Bitmap to byte[]
        {

            // Lock the bitmap's bits. 
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData =
             bmp.LockBits(rect, ImageLockMode.ReadOnly,
             bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes); bmp.UnlockBits(bmpData);

            return rgbValues;
        }

        static void writeImageBytes(Bitmap img, byte[] bytes)//converts byte[] to Bitmap
        {
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.WriteOnly,
                img.PixelFormat);
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);

            img.UnlockBits(data);
        }

        private void mapToolStripMenuItem_Click(object sender, EventArgs e)//opens map tab
        {
            mapUC.Show();
            settingsUC.Hide();
        }

        private void processingToolStripMenuItem_Click(object sender, EventArgs e)//closes map tab
        {
            mapUC.Hide();
            settingsUC.Hide();
        }

        protected override void OnPaintBackground(PaintEventArgs e)//draws bg gradient
        {
            using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(this.ClientRectangle,
                                                                       Color.FromArgb(25, 23, 54),
                                                                       Color.FromArgb(144, 42, 137),
                                                                       -45F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)//updates bg on resize
        {
            this.Invalidate();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)//saves image
        {
            if (pictureBox1.Image != null)
            {
                Clipboard.SetImage(pictureBox1.Image);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)//deletes item
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                pictureBox1.Image = null;

                int selectedIndex = listView1.SelectedIndices[0];
                int invertedIndex = LoadedImages.Count - 1 - listView1.SelectedIndices[0];

                FilePaths.RemoveAt(invertedIndex);
                LoadedImages.RemoveAt(invertedIndex);
                listView1.Items.RemoveAt(selectedIndex);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)//opens settings tab
        {
            settingsUC.Show();
            mapUC.Hide();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)//saves user settings
        {
            Properties.Settings.Default.x = X;
            Properties.Settings.Default.y = Y;
            Properties.Settings.Default.h = Height;
            Properties.Settings.Default.thresholdTop = ThresholdTop;
            Properties.Settings.Default.thresholdBottom = ThresholdBottom;

            Properties.Settings.Default.Save();
        }

        public void GetSettings()//loads user settings
        {
            X = Properties.Settings.Default.x;
            Y = Properties.Settings.Default.y;
            Height = Properties.Settings.Default.h;
            ThresholdTop = Properties.Settings.Default.thresholdTop;
            ThresholdBottom = Properties.Settings.Default.thresholdBottom;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)//saves user settings
        {
            Properties.Settings.Default.x = X;
            Properties.Settings.Default.y = Y;
            Properties.Settings.Default.h = Height;
            Properties.Settings.Default.thresholdTop = ThresholdTop;
            Properties.Settings.Default.thresholdBottom = ThresholdBottom;

            Properties.Settings.Default.Save();
            this.Close();
        }
        #endregion
    }

}
