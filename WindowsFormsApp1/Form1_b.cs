using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Coeff[] v1Coeffs, h1Coeffs, v2Coeffs, h2Coeffs;

        private float xScale, yScale;

        private bool isChanged;

        private Bitmap image;
        private string imageName;

        private string filtersPath;

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 250;
            AddFilesToComboBox();
            UpdateImage();
        }

        private class ComboBoxItem
        {
            public ComboBoxItem(string label, string filePath)
            {
                this.label = label;
                FilePath = filePath;
            }

            public override string ToString()
            {
                return label;
            }

            private readonly string label;
            public readonly string FilePath;
        }

        private void AddFilesToComboBox()
        {
            filtersPath = "Filters";

            if (!Directory.Exists(filtersPath))
                filtersPath = ".";

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(
              Directory.GetFiles(filtersPath, "*.txt")
              .Select(f =>
              new ComboBoxItem(Path.GetFileNameWithoutExtension(f), Path.GetFullPath(f)))
              .ToArray());

            if (comboBox1.Items.Count == 0)
                comboBox1.Items.Add("Sharp_Bilinear");

            comboBox1.SelectedIndex = 0;
        }

        private void UpdateImage()
        {
            if (GetCoeffs())
                isChanged = true;

            if (GetScale())
                isChanged = true;

            if (isChanged)
            {
                Cursor = Cursors.WaitCursor;
                pictureBox1.Image = ScaleImage(image ?? Resource.testImage);
                Cursor = Cursors.Default;
                isChanged = false;
            }
        }

        private bool GetScale()
        {
            var xScale = (float)Math.Round(numericUpDown1.Value, numericUpDown1.DecimalPlaces);
            var yScale = (float)Math.Round(numericUpDown2.Value, numericUpDown2.DecimalPlaces);

            if (xScale == this.xScale && yScale == this.yScale)
                return false;

            this.xScale = xScale;
            this.yScale = yScale;

            return true;
        }

        private bool GetCoeffs()
        {
            var text = textBox1.Text;
            var oldV1 = v1Coeffs?.Clone() as Coeff[];
            var oldH1 = h1Coeffs?.Clone() as Coeff[];
            var oldV2 = v2Coeffs?.Clone() as Coeff[];
            var oldH2 = h2Coeffs?.Clone() as Coeff[];

            h1Coeffs = new Coeff[16];
            v1Coeffs = new Coeff[16];
            h2Coeffs = new Coeff[16];
            v2Coeffs = new Coeff[16];

            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
              .Select(l => l.Trim()).Where(l => l.Length > 0 && l[0] != '#'); // remove comments

            short nn = 0;
            var lineIndex = 0;

            foreach (var line in lines)
            {
                var numbers = line.Split(new[] { ',' }, 4)
                  .Where(n => short.TryParse(n, out nn))
                  .Select(n => nn)
                  .ToList();

                if (!numbers.Any())
                    continue;

                if (lineIndex < 16)
                    h1Coeffs[lineIndex] = new Coeff(numbers);
                else if (lineIndex < 32)
                    v1Coeffs[lineIndex - 16] = new Coeff(numbers);
                else if (lineIndex < 48)
                    h2Coeffs[lineIndex - 32] = new Coeff(numbers);
                else
                    v2Coeffs[lineIndex - 48] = new Coeff(numbers);

                if (++lineIndex >= 64)
                    break;
            }

            while (lineIndex < 64)
            {
                var x = lineIndex / 8 % 2 == 0;
                var c = new Coeff(0, x ? 1f : 0, x ? 0 : 1f, 0);
                if (lineIndex < 16)
                    h1Coeffs[lineIndex] = c;
                else if (lineIndex < 32)
                    v1Coeffs[lineIndex - 16] = c;
                else if (lineIndex < 48)
                    h2Coeffs[lineIndex - 32] = c;
                else
                    v2Coeffs[lineIndex - 48] = c;

                lineIndex++;
            }

            return !(
                CompareCoeffArrays(h1Coeffs, oldH1) &&
                CompareCoeffArrays(v1Coeffs, oldV1) &&
                CompareCoeffArrays(h2Coeffs, oldH2) &&
                CompareCoeffArrays(v2Coeffs, oldV2));
        }

        private Bitmap ScaleImage(Bitmap sourceImage)
        {
            var outputImage = new Bitmap(sourceImage); // AdjustGamma(sourceImage, brightness);
            outputImage.RotateFlip(RotateFlipType.Rotate90FlipX);
            outputImage = ScaleImage(outputImage, v1Coeffs, v2Coeffs, yScale);
            outputImage.RotateFlip(RotateFlipType.Rotate270FlipY);
            outputImage = ScaleImage(outputImage, h1Coeffs, h2Coeffs, xScale);

            return outputImage;
        }

        private Bitmap ScaleImage(Bitmap sourceImage, Coeff[] coeffs1, Coeff[] coeffs2, float scale)
        {
            float outputWidth = scale * sourceImage.Width;
            float outputHeight = sourceImage.Height;

            var input = new Bitmap(sourceImage);

            using (var output = new Bitmap((int)outputWidth, (int)outputHeight))
            {
                var srcData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var dstData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                unsafe
                {
                    var srcPointer = (int*)srcData.Scan0;
                    var inputWidth = input.Width;

                    Parallel.For(0, dstData.Height, y =>
                    {
                        var dstPointer = (int*)dstData.Scan0;
                        dstPointer += y * dstData.Width;

                        for (var x = 0; x < dstData.Width; x++)
                        {
                            var xx = x / scale;
                            var c = GetValueOf(coeffs1, (xx + 0.5f) % 1f);
                            var d = GetValueOf(coeffs2, (xx + 0.5f) % 1f);

                            var t0 = Max((int)(xx - 1.5f), 0);
                            var t1 = Max((int)(xx - 0.5f), 0);
                            var t2 = Min((int)(xx + 0.5f), inputWidth - 1);
                            var t3 = Min((int)(xx + 1.5f), inputWidth - 1);

                            var pixels = new[]
                            {
                                Color.FromArgb(srcPointer[t0 + y * inputWidth]),
                                Color.FromArgb(srcPointer[t1 + y * inputWidth]),
                                Color.FromArgb(srcPointer[t2 + y * inputWidth]),
                                Color.FromArgb(srcPointer[t3 + y * inputWidth])
                            };

                            var e = c + d;

                            dstPointer[0] = Coeff.GetNewColor(c, d, pixels);
                            dstPointer++;
                        }
                    });
                }

                output.UnlockBits(dstData);
                input.UnlockBits(srcData);

                return new Bitmap(output);
            }
        }

        private static bool CompareCoeffArrays(Coeff[] a, Coeff[] b)
        {
            if (a == null || a.Length == 0)
                return b == null || b.Length == 0;

            if (b == null || b.Length == 0)
                return false;

            for (var i = 0; i < a.Length; i++)
                if (!a[i].Equals(b[i]))
                    return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Coeff GetValueOf(Coeff[] coeffs, float v)
        {
            var p = v * coeffs.Length;
            return coeffs[(int)p];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Min(int v1, int v2)
        {
            return v1 < v2 ? v1 : v2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Max(int v1, int v2)
        {
            return v1 > v2 ? v1 : v2;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
                return;

            timer1.Stop();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            UpdateImage();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.A))
            {
                textBox1.SelectAll();
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var stream = openFileDialog1.OpenFile())
                        image = new Bitmap(stream);

                    imageName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName).Trim();
                }
                catch (ArgumentException)
                {
                    MessageBox.Show(this, "The selected file was not a valid image.", "Invalid image file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    image = null;
                    imageName = null;
                }
                finally
                {
                    isChanged = true; // force update
                    timer1.Stop();
                    timer1.Start();
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;

            if (checkBox1.Checked)
            {
                button2.Enabled = false;
                isChanged = true; // force update
                UpdateImage();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            isChanged = true; // force update
            UpdateImage();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouseEvent = (MouseEventArgs)e;

            if (mouseEvent.Button == MouseButtons.Right)
                Clipboard.SetImage(pictureBox1.Image);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(comboBox1.SelectedItem is ComboBoxItem selectedItem && File.Exists(selectedItem.FilePath)))
                return;

            var text = File.ReadAllText(selectedItem.FilePath);
            var rows = text.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.None);
            textBox1.Text = string.Join(Environment.NewLine, rows);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string filePath;

            if (!(comboBox1.SelectedItem is ComboBoxItem selectedItem && File.Exists(selectedItem.FilePath)))
            {
                var index = 0;
                var fileName = comboBox1.Text;
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = "New_Filter";

                do
                {
                    index++;
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), filtersPath, $"{fileName.Trim()}{(index > 1 ? "_" + index : "")}.txt");
                } while (File.Exists(filePath));
            }
            else
            {
                filePath = selectedItem.FilePath;
            }

            File.WriteAllText(filePath, textBox1.Text);
            AddFilesToComboBox();

            foreach (ComboBoxItem item in comboBox1.Items)
            {
                if (item.FilePath == filePath)
                {
                    comboBox1.SelectedItem = item;
                    break;
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == (Keys.Control | Keys.S))
                button3_Click_1(sender, new EventArgs());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var fileName = string.IsNullOrEmpty(imageName) ? comboBox1.Text : $"{imageName}_{comboBox1.Text}";
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Previews");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, $"{fileName}.png");

            isChanged = true;
            UpdateImage();

            pictureBox1.Image.Save(filePath);
        }
    }
}
