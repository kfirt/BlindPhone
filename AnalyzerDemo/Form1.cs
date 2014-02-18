using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnalyzeTrafficLight;

namespace AnalyzerDemo
{
    public partial class Form1 : Form
    {
        string path = @"C:\Users\Adi\Source\Repos\BlindPhone\AnalyzerTests\greens\WP_20140216_011.jpg";
		System.Drawing.Bitmap bmp;

        public Form1()
        {
            InitializeComponent();
            if (System.IO.File.Exists(path))
            {
                pictureBox1.Image = new System.Drawing.Bitmap(path);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Wrap the creation of the OpenFileDialog instance in a using statement,
            // rather than manually calling the Dispose method to ensure proper disposal
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                //dlg.Filter = "bmp files (*.bmp)|*.bmp";

                if (dlg.ShowDialog() == DialogResult.OK)
                {

                    // Create a new Bitmap object from the picture file on disk,
                    // and assign that to the PictureBox.Image property
                    this.path = dlg.FileName;
                    pictureBox1.Image = new System.Drawing.Bitmap(dlg.FileName);
					bmp = new System.Drawing.Bitmap(pictureBox1.Image);
					this.FindForm().Text = this.path;
                }

            }
        }

        private void PictureBox1_LoadCompleted(Object sender, AsyncCompletedEventArgs e)
        {
            //MessageBox.Show("Kfir");
        }

        private List<RectWithColor> rects = new List<RectWithColor>();

        private void button2_Click(object sender, EventArgs e)
        {
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(path);

            // Loop through the image
            int[] map = new int[(int)image.Width * (int)image.Height];

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);
                    map[y * image.Width + x] = pixelColor.ToArgb();
                    //pixelColor.
                }
            }


            Analyzer analyzer = new Analyzer();
            var analyzedObjects = analyzer.analyzeImage(map, image.Width, image.Height);
            this.rects.Clear();

            foreach (var analyzedObject in analyzedObjects)
            {
                int x1 = analyzedObject.bBox.topLeft.x;
                int y1 = analyzedObject.bBox.topLeft.y;
                int x2 = analyzedObject.bBox.bottomRight.x;
                int y2 = analyzedObject.bBox.bottomRight.y;

                double scaleX = (double)pictureBox1.Width / (double)image.Width;
                double scaleY = (double)pictureBox1.Height / (double)image.Height;

                int scalex1 = Convert.ToInt32(x1 * scaleX);
                int scalex2 = Convert.ToInt32(x2 * scaleX);

                int scaley1 = Convert.ToInt32(y1 * scaleY);
                int scaley2 = Convert.ToInt32(y2 * scaleY);

                System.Drawing.Color color;
                if (analyzedObject.decision)
                {
                    color = System.Drawing.Color.Yellow;
                }
                else
                {
                    color = convertColor(analyzedObject.color);
                }

                //A circle with Red Color and 2 Pixel wide line
                rects.Add(new RectWithColor
                {
                    Rectangle = new Rectangle(scalex1, scaley1, scalex2 - scalex1, scaley2 - scaley1),
                    Pen = new Pen(color, 2)
                });
            }

            pictureBox1.Invalidate();
        }

        static System.Drawing.Color convertColor(AnalyzeTrafficLight.Color color)
        {
            if (color.Equal(AnalyzeTrafficLight.Color.red))
            {
                return System.Drawing.Color.DarkRed;
            }
            else if (color.Equal(AnalyzeTrafficLight.Color.green))
            {
                return System.Drawing.Color.DarkGreen;
            }
            // Should not happend
            return System.Drawing.Color.Yellow;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Pen p = new Pen(System.Drawing.Color.Red);
            var g = e.Graphics;
            foreach (var rect in rects)
            {
                g.DrawRectangle(rect.Pen, rect.Rectangle);
            }
		}

		private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
		{
			System.Drawing.Color c = bmp.GetPixel(e.X, e.Y);
			textBox1.Text = "X=" + e.X + ", Y=" + e.Y + "   R=" + c.R + ", G=" + c.G + ", B=" + c.B;
		}
    }
    public class RectWithColor
    {
        public Pen Pen { get; set; }
        public Rectangle Rectangle { get; set; }
    }
}
