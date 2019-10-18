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
    public partial class Form : System.Windows.Forms.Form
    {
        private const decimal DefaultGamma = 2.2m;

        private Coeff[] vCoeffs, hCoeffs;

        private decimal scaleX;
        private decimal scaleY;

        private bool isChanged;

        private Bitmap image;
        private string imageName;

        private string filtersPath;

        private decimal gamma;
        private readonly byte[] gammaLut = new byte[256];

        public Form()
        {
            InitializeComponent();
            AddFiltersToComboBox();
            FilterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;

            GammaUpDown.Value = Properties.Settings.Default.Gamma;

            LoadLastImage();

            LoadLastFilter();

            AutomaticRedrawChkBox.Checked = Properties.Settings.Default.AutomaticRedraw;

            ScaleXUpDown.Value = Properties.Settings.Default.ScaleX;
            ScaleYUpDown.Value = Properties.Settings.Default.ScaleY;

            UpdateImage();
        }

        private void LoadLastFilter()
        {
            var lastFilter = Properties.Settings.Default.LastFilter;
            if (!string.IsNullOrEmpty(lastFilter))
            {
                foreach (var cbi in FilterComboBox.Items.Cast<ComboBoxItem>())
                {
                    if (cbi.FilePath != lastFilter) continue;

                    FilterComboBox.SelectedItem = cbi;
                    break;
                }
            }
        }

        private void LoadLastImage()
        {
            var lastImage = Properties.Settings.Default.LastImage;

            if (!string.IsNullOrEmpty(lastImage) &&
                File.Exists(lastImage))
            {
                try
                {
                    using (var stream = new FileStream(lastImage, FileMode.Open, FileAccess.Read))
                        image = new Bitmap(stream);

                    imageName = Path.GetFileNameWithoutExtension(OpenImageDialog.FileName)?.Trim();
                }
                catch
                {
                    image = Resource.testImage;
                    imageName = null;
                }
            }
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

        private void AddFiltersToComboBox()
        {
            filtersPath = "Filters";

            if (!Directory.Exists(filtersPath))
                filtersPath = ".";

            FilterComboBox.Items.Clear();
            FilterComboBox.Items.AddRange(
              Directory.GetFiles(filtersPath, "*.txt")
              .Select(f =>
              (object)new ComboBoxItem(Path.GetFileNameWithoutExtension(f), Path.GetFullPath(f)))
              .ToArray());

            if (FilterComboBox.Items.Count == 0)
                FilterComboBox.Items.Add("Sharp_Bilinear");

            FilterComboBox.SelectedIndex = 0;
        }

        private void UpdateImage()
        {
            if (GetGamma())             
                isChanged = true;

            if (GetCoeffs())
                isChanged = true;

            if (GetScale())
                isChanged = true;

            if (isChanged)
            {
                Cursor = Cursors.WaitCursor;
                PreviewPictureBox.Image = ScaleImage(image ?? Resource.testImage);
                Cursor = Cursors.Default;
                isChanged = false;
            }
        }

        private bool GetScale()
        {
            var newScaleX = Math.Round(ScaleXUpDown.Value, ScaleXUpDown.DecimalPlaces);
            var newScaleY = Math.Round(ScaleYUpDown.Value, ScaleYUpDown.DecimalPlaces);

            if (newScaleX == scaleX && newScaleY == scaleY)
                return false;

            scaleX = newScaleX;
            scaleY = newScaleY;

            Properties.Settings.Default.ScaleX = scaleX;
            Properties.Settings.Default.ScaleY = scaleY;

            return true;
        }

        private bool GetCoeffs()
        {
            var text = filterTextBox.Text;
            var oldV = vCoeffs?.Clone() as Coeff[];
            var oldH = hCoeffs?.Clone() as Coeff[];

            hCoeffs = new Coeff[16];
            vCoeffs = new Coeff[16];

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
                    hCoeffs[lineIndex] = new Coeff(numbers);
                else
                    vCoeffs[lineIndex - 16] = new Coeff(numbers);

                if (++lineIndex >= 32)
                    break;
            }

            while (lineIndex < 32)
            {
                var x = lineIndex / 8 % 2 == 0;
                var c = new Coeff(0, x ? 1f : 0, x ? 0 : 1f, 0);
                if (lineIndex < 16)
                    hCoeffs[lineIndex] = c;
                else
                    vCoeffs[lineIndex - 16] = c;

                lineIndex++;
            }

            return !(CompareCoeffArrays(hCoeffs, oldH) && CompareCoeffArrays(vCoeffs, oldV));
        }

        private Bitmap ScaleImage(Bitmap sourceImage)
        {
            var outputImage = ApplyGammaLut(sourceImage, gammaLut);
            outputImage.RotateFlip(RotateFlipType.Rotate90FlipX);
            outputImage = ScaleImage(outputImage, vCoeffs, scaleY);
            outputImage.RotateFlip(RotateFlipType.Rotate270FlipY);
            outputImage = ScaleImage(outputImage, hCoeffs, scaleX);

            return outputImage;
        }

        private void UpdateGammaLut(decimal gammaCorrection)
        {
            var len = gammaLut.Length;
            for (var i = 0; i < len; i++)
                gammaLut[i] = Clamp((int)(255.0 * Math.Pow(i / (double)(len - 1), 1 / (double)gammaCorrection) + .5));
        }

        private static byte Clamp(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return (byte)v;
        }

        private static Bitmap ScaleImage(Bitmap sourceImage, Coeff[] coeffs, decimal scale)
        {
            decimal outputWidth = scale * sourceImage.Width;
            decimal outputHeight = sourceImage.Height;

            var input = new Bitmap(sourceImage);

            using (var output = new Bitmap((int)outputWidth, (int)outputHeight))
            {
                var srcData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var dstData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                unsafe
                {
                    var srcPointer = (int*)srcData.Scan0;
                    if (srcPointer == null)
                        throw new NullReferenceException();

                    var inputWidth = input.Width;

                    Parallel.For(0, dstData.Height, y =>
                    {
                        var dstPointer = (int*)dstData.Scan0;
                        dstPointer += y * dstData.Width;

                        for (var x = 0; x < dstData.Width; x++)
                        {
                            var xx = x / scale;
                            var c = GetValueOf(coeffs, (xx + 0.5m) % 1.0m);

                            var t0 = Max((int)(xx - 1.5m), 0);
                            var t1 = Max((int)(xx - 0.5m), 0);
                            var t2 = Min((int)(xx + 0.5m), inputWidth - 1);
                            var t3 = Min((int)(xx + 1.5m), inputWidth - 1);

                            var pixels = new[]
                            {
                                Color.FromArgb(srcPointer[t0 + y * inputWidth]),
                                Color.FromArgb(srcPointer[t1 + y * inputWidth]),
                                Color.FromArgb(srcPointer[t2 + y * inputWidth]),
                                Color.FromArgb(srcPointer[t3 + y * inputWidth])
                            };

                            dstPointer[0] = c * pixels;
                            dstPointer++;
                        }
                    });
                }

                output.UnlockBits(dstData);
                input.UnlockBits(srcData);

                return new Bitmap(output);
            }
        }

        private static Bitmap ApplyGammaLut(Bitmap input, byte[] gammaLut)
        {
            var inputWidth = input.Width;
            var inputHeight = input.Height;

            using (var output = new Bitmap(inputWidth, inputHeight))
            {
                var srcData = input.LockBits(new Rectangle(0, 0, inputWidth, inputHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var dstData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                unsafe
                {
                    var srcPointer = (int*)srcData.Scan0;
                    if (srcPointer == null)
                        throw new NullReferenceException();

                    Parallel.For(0, dstData.Height, y =>
                    {
                        var dstPointer = (int*)dstData.Scan0;
                        dstPointer += y * dstData.Width;

                        for (var x = 0; x < dstData.Width; x++)
                        {
                            var c = Color.FromArgb(srcPointer[x + y * inputWidth]);

                            dstPointer[0] = BitConverter.ToInt32(new byte[] { gammaLut[c.B], gammaLut[c.G], gammaLut[c.R], 0xff }, 0);
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
        private static Coeff GetValueOf(Coeff[] coeffs, decimal v)
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

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == (Keys.Control | Keys.S))
                SaveBtn_Click(sender, new EventArgs());
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            isChanged = true; // force update
            UpdateImage();
        }

        private void LoadImageBtn_Click(object sender, EventArgs e)
        {
            if (OpenImageDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var stream = OpenImageDialog.OpenFile())
                        image = new Bitmap(stream);

                    imageName = Path.GetFileNameWithoutExtension(OpenImageDialog.FileName)?.Trim();

                    Properties.Settings.Default.LastImage = OpenImageDialog.FileName;
                }
                catch (ArgumentException)
                {
                    MessageBox.Show(this, @"The selected file was not a valid image.", @"Invalid image file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    image = null;
                    imageName = null;
                }
                finally
                {
                    isChanged = true; // force update
                    RedrawTimer.Stop();
                    RedrawTimer.Start();
                }
            }
        }

        private void AutomaticRedrawChkBox_CheckedChanged(object sender, EventArgs e)
        {
            updateBtn.Enabled = true;

            if (AutomaticRedrawChkBox.Checked)
            {
                updateBtn.Enabled = false;
                isChanged = true; // force update
                UpdateImage();
            }

            Properties.Settings.Default.AutomaticRedraw = AutomaticRedrawChkBox.Checked;
        }

        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!AutomaticRedrawChkBox.Checked)
                return;

            RedrawTimer.Stop();
            RedrawTimer.Start();
        }

        private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.A))
            {
                filterTextBox.SelectAll();
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            string filePath;

            if (!(FilterComboBox.SelectedItem is ComboBoxItem selectedItem && File.Exists(selectedItem.FilePath)))
            {
                var index = 0;
                var fileName = FilterComboBox.Text;
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

            File.WriteAllText(filePath, filterTextBox.Text);
            AddFiltersToComboBox();

            foreach (ComboBoxItem item in FilterComboBox.Items)
            {
                if (item.FilePath == filePath)
                {
                    FilterComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(FilterComboBox.SelectedItem is ComboBoxItem selectedItem && File.Exists(selectedItem.FilePath)))
                return;

            var text = File.ReadAllText(selectedItem.FilePath);
            var rows = text.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.None);
            filterTextBox.Text = string.Join(Environment.NewLine, rows);

            Properties.Settings.Default.LastFilter = selectedItem.FilePath;
        }

        private void PreviewPictureBox_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouseEvent = (MouseEventArgs)e;

            if (mouseEvent.Button == MouseButtons.Right)
                Clipboard.SetImage(PreviewPictureBox.Image);
        }

        private void RedrawTimer_Tick(object sender, EventArgs e)
        {
            RedrawTimer.Stop();
            UpdateImage();
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private bool GetGamma()
        {
            var oldGamma = gamma;
            gamma = GammaUpDown.Value;
            if (gamma == oldGamma)
                return false;

            UpdateGammaLut(DefaultGamma / gamma);
            Properties.Settings.Default.Gamma = gamma;
            return true;
        }

        private void SavePreviewBtn_Click(object sender, EventArgs e)
        {
            var fileName = string.IsNullOrEmpty(imageName) ? FilterComboBox.Text : $"{imageName}_{FilterComboBox.Text}";
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Previews");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, $"{fileName}.png");

            isChanged = true;
            UpdateImage();

            PreviewPictureBox.Image.Save(filePath);
        }
    }
}
