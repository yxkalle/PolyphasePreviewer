using System;
#if DEBUG
using System.Diagnostics;
#endif
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PolyphasePreviewer
{
    public partial class MainForm
    {
        private const int CoeffsLength = 16;

        private Coeff[] vCoeffs, hCoeffs;

        private decimal scaleX;
        private decimal scaleY;

        private bool isChanged;
        private bool isDefaultGamma = true;

        private Bitmap image;
        private string imageName;

        private string filtersPath;
        private string gammaLutsPath;

        private readonly byte[,] gammaLut = new byte[3, 256];

        public MainForm()
        {
            InitializeComponent();

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            AddFiltersToComboBox();
            AddGammaLutsToComboBox();

            FilterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;
            GammaLutComboBox.SelectedIndexChanged += GammaLutComboBox_SelectedIndexChanged;

            LoadLastImage();
            LoadLastFilter();
            LoadLastGammaLut();

            FilterTextBox.Select(FilterTextBox.TextLength, 0);

            UpdateImage();
        }

        private void LoadLastFilter()
        {
            var lastFilter = Properties.Settings.Default.LastFilter;
            if (string.IsNullOrEmpty(lastFilter))
            {
                FilterComboBox.SelectedIndex = 0;
                return;
            }

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

                imageName = Path.GetFileNameWithoutExtension(lastImage).Trim();
            }
            catch
            {
                image = Resource.testImage;
                imageName = null;
            }
        }

        private class ComboBoxItem
        {
            public ComboBoxItem(string label, string path)
            {
                this.label = label;
                FilePath = path;
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

            filtersPath = Path.GetFullPath(filtersPath);

            FilterComboBox.Items.Clear();

            var filters = GetFilesAsComboBoxItems(filtersPath);

            FilterComboBox.Items.AddRange(filters);

            if (FilterComboBox.Items.Count == 0)
            {
                const string defaultName = "Sharp_Bilinear";
                FilterComboBox.Items.Add(new ComboBoxItem(defaultName, Path.Combine(filtersPath, defaultName, ".txt")));
            }

            FilterComboBox.SelectedIndex = 0;
        }

        private static object[] GetFilesAsComboBoxItems(string path, string searchPattern = "*.txt")
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)
                .Select(f =>
                {
                    var currentDir = Path.GetDirectoryName(f) ?? path;

                    var isRoot = currentDir == path;

                    return (object)new ComboBoxItem(
                        (isRoot
                            ? ""
                            : currentDir.Substring(path.Length + 1).Replace('\\', '/') + '/') +
                        Path.GetFileNameWithoutExtension(f),
                        f);
                })
                .ToArray();
        }

        private void AddGammaLutsToComboBox()
        {
            gammaLutsPath = "Gamma";

            GammaLutComboBox.Items.Clear();
            GammaLutComboBox.Items.Add(new ComboBoxItem("No gamma adjustment", null));
            GammaLutComboBox.SelectedIndex = 0;

            if (!Directory.Exists(gammaLutsPath))
                return;

            var luts = GetFilesAsComboBoxItems(gammaLutsPath);

            GammaLutComboBox.Items.AddRange(luts);
        }

        private void UpdateImage()
        {
            if (GetCoeffs())
                isChanged = true;

            if (GetScale())
                isChanged = true;

            if (!isChanged) return;

            Cursor = Cursors.WaitCursor;
            PreviewPictureBox.Image = ProcessImage(image ?? Resource.testImage);
            Cursor = Cursors.Default;

            isChanged = false;
        }

        private bool GetScale()
        {
            decimal newScaleX = Math.Round(ScaleXUpDown.Value, ScaleXUpDown.DecimalPlaces),
                    newScaleY = Math.Round(ScaleYUpDown.Value, ScaleYUpDown.DecimalPlaces);

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
                .ToArray();

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
                    var newCoeff = new Coeff(numbers[0], numbers[1], numbers[2], numbers[3]);

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

        private Bitmap ProcessImage(Bitmap sourceImage)
        {
            Bitmap outputImage;

#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            if (isDefaultGamma)
                outputImage = new Bitmap(sourceImage);
            else
                outputImage = ApplyGammaLut(sourceImage, gammaLut);

#if DEBUG
            Debug.WriteLine($@"Gamma correction took {sw.ElapsedMilliseconds} ms");
            sw.Restart();
#endif
            if (SnesModeChkBox.Checked)
                outputImage = DoubleHorizontalResolution(outputImage);

#if DEBUG
            Debug.WriteLine($@"Doubling horizontal resolution took {sw.ElapsedMilliseconds} ms");
            sw.Restart();
#endif
            outputImage = ScaleImage(outputImage);

#if DEBUG
            Debug.WriteLine($@"Scaling took {sw.ElapsedMilliseconds} ms");
#endif

            return outputImage;
        }

        private static byte ClampToByte(int val)
        {
            if (val < 0) return 0;
            if (val > 0xff) return 0xff;

            return (byte)val;
        }

        [DllImport("PolyphaseScaler_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Scale")]
        private static extern void Scale_x86(IntPtr input, IntPtr output, int width, int height, IntPtr hCoeffs, IntPtr vCoeffs, float scaleX, float scaleY);

        [DllImport("PolyphaseScaler_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Scale")]
        private static extern void Scale_x64(IntPtr input, IntPtr output, int width, int height, IntPtr hCoeffs, IntPtr vCoeffs, float scaleX, float scaleY);

        private Bitmap ScaleImage(Bitmap input)
        {
            var xF = (float)scaleX;
            var yF = (float)scaleY;

            if (SnesModeChkBox.Checked)
                xF /= 2f;

            GCHandle hCHandle = GCHandle.Alloc(hCoeffs.SelectMany(c => c).ToArray(), GCHandleType.Pinned),
                     vCHandle = GCHandle.Alloc(vCoeffs.SelectMany(c => c).ToArray(), GCHandleType.Pinned);

            try
            {
                IntPtr hCPtr = hCHandle.AddrOfPinnedObject(),
                       vCPtr = vCHandle.AddrOfPinnedObject();

                var output = new Bitmap((int)(xF * input.Width), (int)(yF * input.Height));

                BitmapData srcData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height),
                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb),
                          dstData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height),
                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                IntPtr srcPointer = srcData.Scan0,
                       dstPointer = dstData.Scan0;

                if (Environment.Is64BitProcess)
                    Scale_x64(srcPointer, dstPointer, input.Width, input.Height, hCPtr, vCPtr, xF, yF);
                else
                    Scale_x86(srcPointer, dstPointer, input.Width, input.Height, hCPtr, vCPtr, xF, yF);

                input.UnlockBits(srcData);
                output.UnlockBits(dstData);

                return output;
            }
            finally
            {
                if (hCHandle.IsAllocated)
                    hCHandle.Free();

                if (vCHandle.IsAllocated)
                    vCHandle.Free();
            }
        }

        private static Bitmap ApplyGammaLut(Bitmap input, byte[,] gammaLut)
        {
            var inputWidth = input.Width;
            var inputHeight = input.Height;

            var output = new Bitmap(input);

            BitmapData srcData = input.LockBits(new Rectangle(0, 0, inputWidth, inputHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb),
                       dstData = output.LockBits(new Rectangle(0, 0, inputWidth, inputHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            IntPtr srcPointer = srcData.Scan0,
                   dstPointer = dstData.Scan0;

            Parallel.For(0, inputHeight, y =>
            {
                IntPtr srcRowPointer = srcPointer + (y * inputWidth * sizeof(int)),
                       dstRowPointer = dstPointer + (y * inputWidth * sizeof(int));

                for (var x = 0; x < inputWidth; x++)
                {
                    var srcPixel = new Pixel { Argb = Marshal.ReadInt32(srcRowPointer) };
                    var dstPixel = new Pixel
                    {
                        R = gammaLut[2, srcPixel.R],
                        G = gammaLut[1, srcPixel.G],
                        B = gammaLut[0, srcPixel.B],
                        A = 0xff
                    };

                    Marshal.WriteInt32(dstRowPointer, dstPixel.Argb);

                    srcRowPointer += sizeof(int);
                    dstRowPointer += sizeof(int);
                }
            });

            output.UnlockBits(dstData);
            input.UnlockBits(srcData);

            return output;
        }

        private static Bitmap DoubleHorizontalResolution(Bitmap input)
        {
            var inputWidth = input.Width;
            var inputHeight = input.Height;

            var output = new Bitmap(inputWidth * 2, inputHeight);

            BitmapData srcData = input.LockBits(new Rectangle(0, 0, inputWidth, inputHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb),
                       dstData = output.LockBits(new Rectangle(0, 0, inputWidth, inputHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            IntPtr srcPointer = srcData.Scan0,
                   dstPointer = dstData.Scan0;

            Parallel.For(0, inputHeight, y =>
            {
                IntPtr srcRowPointer = srcPointer + (y * inputWidth * sizeof(int)),
                       dstRowPointer = dstPointer + (y * inputWidth * sizeof(int) * 2);

                for (var x = 0; x < inputWidth; x++)
                {
                    var srcPixel = Marshal.ReadInt32(srcRowPointer);

                    Marshal.WriteInt32(dstRowPointer, srcPixel);
                    dstRowPointer += sizeof(int);
                    Marshal.WriteInt32(dstRowPointer, srcPixel);
                    dstRowPointer += sizeof(int);
                    srcRowPointer += sizeof(int);
                }
            });

            output.UnlockBits(dstData);
            input.UnlockBits(srcData);

            return output;
        }

        private static bool CompareCoeffArrays(Coeff[] a, Coeff[] b)
        {
            if (a == null || a.Length == 0)
                return b == null || b.Length == 0;

            if (b == null || b.Length == 0)
                return false;

            for (var i = 0; i < a.Length; i++)
            {
                if (a[i].Equals(b[i]) != true)
                    return false;
            }

            return true;
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
                FilterTextBox_TextChanged(sender, e);
                return;
            }

            isDefaultGamma = false;

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
                    var val = ClampToByte(byte.TryParse(rows[i].Trim(), NumberStyles.Integer, null, out var v) ? v : 0);
                    gammaLut[0, i] = val;
                    gammaLut[1, i] = val;
                    gammaLut[2, i] = val;
                }
            }

            Properties.Settings.Default.LastGammaLut = selectedItem.FilePath;
            FilterTextBox_TextChanged(sender, e);
        }

        private void SetDefaultGammaLut()
        {
            isDefaultGamma = true;
            Properties.Settings.Default.LastGammaLut = null;
            GammaLutComboBox.SelectedIndex = 0;
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

        private void SnesModeChkBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SnesMode = SnesModeChkBox.Checked;

            if (AutomaticRedrawChkBox.Checked)
            {
                isChanged = true;
                UpdateImage();
            }
        }

        private void SavePreviewBtn_Click(object sender, EventArgs e)
        {
            var fileName = string.IsNullOrEmpty(imageName) ? FilterComboBox.Text : $"{imageName}_{FilterComboBox.Text.Replace('/', '_')}";
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
