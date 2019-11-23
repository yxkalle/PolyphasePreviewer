using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PolyphasePreviewer
{
    public partial class Form
    {
        private const int CoeffsLength = 16;

        private Coeff[] vCoeffs, hCoeffs;

        private decimal scaleX;
        private decimal scaleY;

        private bool isChanged;

        private Bitmap image;
        private string imageName;

        private string filtersPath;
        private string gammaLutsPath;

        private readonly byte[,] gammaLut = new byte[3, 256];

        public Form()
        {
            InitializeComponent();

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            AddFiltersToComboBox();
            AddGammaLutsToComboBox();

            FilterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;
            GammaLutComboBox.SelectedIndexChanged += GammaLutComboBox_SelectedIndexChanged;

            LoadLastFilter();
            LoadLastGammaLut();
            LoadLastImage();

            FilterTextBox.Select(FilterTextBox.TextLength, 0);

            UpdateImage();
        }

        private void LoadLastFilter()
        {
            var lastFilter = Properties.Settings.Default.LastFilter;
            if (string.IsNullOrEmpty(lastFilter)) return;

            foreach (var cbi in FilterComboBox.Items.Cast<ComboBoxItem>())
            {
                if (cbi.FilePath != lastFilter) continue;

                FilterComboBox.SelectedItem = cbi;
                break;
            }
        }

        private void LoadLastGammaLut()
        {
            var lastGammaLut = Properties.Settings.Default.LastGammaLut;

            if (string.IsNullOrEmpty(lastGammaLut))
                SetDefaultGammaLut();

            foreach (var cbi in GammaLutComboBox.Items.Cast<ComboBoxItem>())
            {
                if (cbi.FilePath != lastGammaLut) continue;

                GammaLutComboBox.SelectedItem = cbi;
                return;
            }

            SetDefaultGammaLut();
        }

        private void LoadLastImage()
        {
            var lastImage = Properties.Settings.Default.LastImage;

            if (string.IsNullOrEmpty(lastImage) || !File.Exists(lastImage)) return;

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

        private void AddGammaLutsToComboBox()
        {
            gammaLutsPath = "Gamma";

            GammaLutComboBox.Items.Clear();
            GammaLutComboBox.Items.Add(new ComboBoxItem("No gamma adjustment", null));
            GammaLutComboBox.SelectedIndex = 0;

            if (!Directory.Exists(gammaLutsPath))
                return;

            GammaLutComboBox.Items.AddRange(
                Directory.GetFiles(gammaLutsPath, "*.txt")
                    .Select(g =>
                        (object)new ComboBoxItem(Path.GetFileNameWithoutExtension(g), Path.GetFullPath(g)))
                    .ToArray());
        }

        private void UpdateImage()
        {
            if (GetCoeffs())
                isChanged = true;

            if (GetScale())
                isChanged = true;

            if (!isChanged) return;

            Cursor = Cursors.WaitCursor;
            PreviewPictureBox.Image = ScaleImage(image ?? Resource.testImage);
            Cursor = Cursors.Default;
            isChanged = false;
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
            var numberOfErrors = 0;
            var numberOfWarnings = 0;

            var text = FilterTextBox.Text;
            var oldV = vCoeffs?.Clone() as Coeff[];
            var oldH = hCoeffs?.Clone() as Coeff[];

            hCoeffs = new Coeff[CoeffsLength];
            vCoeffs = new Coeff[CoeffsLength];

            var numberLines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
              .Select(l => l.Trim()).Where(tl => tl.Length > 0 && tl[0] != '#')
              .ToArray(); // remove comments

            short sn = 0;
            var lineIndex = 0;

            foreach (var l in numberLines)
            {
                var numbers = l.Split(new[] { ',' }, 4)
                  .Where(n => short.TryParse(n, out sn))
                  .Select(n => sn)
                  .ToArray();

                if (numbers.Length == 4)
                {
                    var newCoeff = new Coeff(numbers);

                    if (newCoeff.Sum() != 128)
                        numberOfWarnings++;

                    if (lineIndex < CoeffsLength)
                        hCoeffs[lineIndex] = newCoeff;
                    else
                        vCoeffs[lineIndex - CoeffsLength] = newCoeff;
                }
                else
                {
                    numberOfErrors++;
                    if (lineIndex < CoeffsLength)
                        hCoeffs[lineIndex] = new Coeff();
                    else
                        vCoeffs[lineIndex - CoeffsLength] = new Coeff();
                }

                if (++lineIndex >= CoeffsLength * 2)
                    break;
            }

            if (numberLines.Length < CoeffsLength * 2)
                numberOfErrors += CoeffsLength * 2 - numberLines.Length;
            else
                numberOfErrors += numberLines.Length - CoeffsLength * 2;

            for (; lineIndex < CoeffsLength * 2; lineIndex++) // fill remaining empty spots
            {
                var c = new Coeff();
                if (lineIndex < CoeffsLength)
                    hCoeffs[lineIndex] = c;
                else
                    vCoeffs[lineIndex - CoeffsLength] = c;
            }

            ErrorsAndWarningsLabel.Text = $@"{(numberOfErrors > 0 ? numberOfErrors + " error(s)" : "")}{(numberOfErrors > 0 && numberOfWarnings > 0 ? ", " : "")}{(numberOfWarnings > 0 ? numberOfWarnings + " warnings(s)" : "")}";

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

        private static byte ClampToByte(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 0xff;
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

        private static Bitmap ApplyGammaLut(Bitmap input, byte[,] gammaLut)
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

                            dstPointer[0] = BitConverter.ToInt32(new[] { gammaLut[2, c.B], gammaLut[1, c.G], gammaLut[0, c.R], (byte)0xff }, 0);
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
            {
                if (a[i]?.Equals(b[i]) != true)
                    return false;
            }

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
                    MessageBox.Show(this, @"The selected file was not a valid image.", @"Invalid image file",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
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
                FilterTextBox.SelectAll();
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

            File.WriteAllText(filePath, FilterTextBox.Text.Replace(Environment.NewLine, "\n"), Encoding.UTF8);

            FilterComboBox.SelectedIndexChanged -= FilterComboBox_SelectedIndexChanged;

            AddFiltersToComboBox();

            foreach (ComboBoxItem item in FilterComboBox.Items)
            {
                if (item.FilePath == filePath)
                {
                    FilterComboBox.SelectedItem = item;
                    break;
                }
            }

            FilterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(FilterComboBox.SelectedItem is ComboBoxItem selectedItem && File.Exists(selectedItem.FilePath)))
                return;

            var text = File.ReadAllText(selectedItem.FilePath);
            var rows = text.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.None);
            FilterTextBox.Text = string.Join(Environment.NewLine, rows);

            Properties.Settings.Default.LastFilter = selectedItem.FilePath;
        }

        private void GammaLutComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            isChanged = true;

            if (GammaLutComboBox.SelectedIndex == 0)
            {
                SetDefaultGammaLut();
                Properties.Settings.Default.LastGammaLut = null;
                FilterTextBox_TextChanged(sender, e);
                return;
            }

            if (!(GammaLutComboBox.SelectedItem is ComboBoxItem selectedItem && File.Exists(selectedItem.FilePath)))
                return;

            var text = File.ReadAllText(selectedItem.FilePath);
            var rows = text.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim()).Where(l => l.Length > 0 && l[0] != '#') // remove comments
                .ToArray();

            for (var i = 0; i < 256 && i < rows.Length; i++)
            {
                var values = rows[i].Split(',');
                if (values.Length == 3)
                {
                    gammaLut[0, i] = ClampToByte(byte.TryParse(values[0].Trim(), NumberStyles.Integer, null, out var r) ? r : 0);
                    gammaLut[1, i] = ClampToByte(byte.TryParse(values[1].Trim(), NumberStyles.Integer, null, out var g) ? g : 0);
                    gammaLut[2, i] = ClampToByte(byte.TryParse(values[2].Trim(), NumberStyles.Integer, null, out var b) ? b : 0);
                }
                else
                {
                    gammaLut[0, i] = gammaLut[1, i] = gammaLut[2, i] = ClampToByte(byte.TryParse(rows[i].Trim(), NumberStyles.Integer, null, out var value) ? value : 0);
                }
            }

            Properties.Settings.Default.LastGammaLut = selectedItem.FilePath;
            FilterTextBox_TextChanged(sender, e);
        }

        private void SetDefaultGammaLut()
        {
            Properties.Settings.Default.LastGammaLut = null;

            for (var i = 0; i < 256; i++)
            {
                gammaLut[0, i] = (byte)i; // default LUT
                gammaLut[1, i] = (byte)i;
                gammaLut[2, i] = (byte)i;
            }
        }

        private void PreviewPictureBox_Click(object sender, EventArgs e)
        {
            var mouseEvent = (MouseEventArgs)e;

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
            Properties.Settings.Default.WindowState = WindowState;

            switch (WindowState)
            {
                case FormWindowState.Normal:
                    Properties.Settings.Default.Location = Location;
                    Properties.Settings.Default.Size = Size;
                    break;
                default:
                    Properties.Settings.Default.Location = RestoreBounds.Location;
                    Properties.Settings.Default.Size = RestoreBounds.Size;
                    break;
            }

            Properties.Settings.Default.Save();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            WindowState = Properties.Settings.Default.WindowState;
            Location = Properties.Settings.Default.Location;
            Size = Properties.Settings.Default.Size;
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
