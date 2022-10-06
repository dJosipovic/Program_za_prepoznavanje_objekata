using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Alturos.Yolo;
using Alturos.Yolo.Model;

namespace Program_za_prepoznavanje_objekata
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<string> listimage;
        string _filename;
        YoloWrapper yoloWrapper;
        //OpenFileDialog OpenFileDialog = new OpenFileDialog();

        private void btnOtvori_Click(object sender, EventArgs e)
        {
            //OpenFileDialog.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";
            //if (OpenFileDialog.ShowDialog() == DialogResult.OK && OpenFileDialog.CheckPathExists)
            //{
            //    foreach (string File in OpenFileDialog.FileNames)
            //    {
            //        ((DataGridViewImageCell)dataGridView1.Rows[0].Cells[1]).Value = Image.FromFile(File);
            //    }
            //}

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    var files = Directory.GetFiles(fbd.SelectedPath);
                    var table = new DataTable();
                    table.Columns.Add("File Name");

                    foreach (var file in files)
                    {
                        if (Regex.IsMatch(file, @".jpg|.png|.bmp|.jpeg|.JPG|.PNG|.JPEG|.BMP$"))
                        {
                            listimage.Add(file);
                            table.Rows.Add(Path.GetFileNameWithoutExtension(file));
                        }
                    }
                    dataGridView1.DataSource = table;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listimage = new List<string>();

            var configurationDetector = new YoloConfigurationDetector();
            var config = configurationDetector.Detect();
            yoloWrapper = new YoloWrapper(config);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            _filename = listimage[dataGridView1.CurrentRow.Index];
            var image = new Bitmap(_filename);
            pictureBox1.Image = image;         
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            var items = yoloWrapper.Detect(_filename);

            DrawBorderResult(items.ToList(), _filename);
        }

        private void DrawBorderResult(List<YoloItem> items, string filename)
        {
            var image = Image.FromFile(filename);
            using(var canvas = Graphics.FromImage(image))
            {
                foreach (var item in items)
                {
                    var x = item.X;
                    var y = item.Y;
                    var width = item.Width;
                    var height = item.Height;

                    var confidence = item.Confidence.ToString("P", CultureInfo.InvariantCulture);
                    Random rnd = new Random();
                    Color color = Color.FromArgb(255, rnd.Next(255), rnd.Next(255));
                    using(var pen = new Pen(color, 3))
                    {
                        canvas.DrawRectangle(pen, x, y, width, height);
                        canvas.DrawString(item.Type + " " + confidence, new Font("Arial", 5), new SolidBrush(color), x, y);
                        canvas.Flush();
                    }
                }

                pictureBox1.Image = image;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
