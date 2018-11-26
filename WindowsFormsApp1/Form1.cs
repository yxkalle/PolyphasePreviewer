using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
  public partial class Form1 : Form
  {
    private Coeff[] vCoeffs;
    private Coeff[] hCoeffs;

    private float xScale;
    private float yScale;

    private bool isChanged;

    private Bitmap image = null;

    public Form1()
    {
      InitializeComponent();
      timer1.Interval = 1000;
      UpdateImage();
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
      var xScale = (float)numericUpDown1.Value;
      var yScale = (float)numericUpDown2.Value;

      if (xScale == this.xScale && yScale == this.yScale)
        return false;

      this.xScale = xScale;
      this.yScale = yScale;

      return true;
    }

    private bool GetCoeffs()
    {
      var text = textBox1.Text;
      var oldV = vCoeffs?.Clone() as Coeff[];
      var oldH = hCoeffs?.Clone() as Coeff[];

      hCoeffs = new Coeff[16];
      vCoeffs = new Coeff[16];

      var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
        .Select(l => l.Trim()).Where(l => l.Length > 0 && l[0] != '#');

      short nn = 0;
      var lineIndex = 0;

      foreach (var line in lines)
      {
        var numbers = line.Split(new[] { ',' }, 4)
          .Where(n => short.TryParse(n, out nn))
          .Select(n => nn);

        if (numbers.Count() == 0)
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
        if (lineIndex < 16)
          hCoeffs[lineIndex] = new Coeff(0, 0, 0, 0);
        else
          vCoeffs[lineIndex - 16] = new Coeff(0, 0, 0, 0);

        lineIndex++;
      }

      return !(CompareCoeffArrays(hCoeffs, oldH) && CompareCoeffArrays(vCoeffs, oldV));
    }

    private Image ScaleImage(Image sourceImage)
    {
      var outputImage = new Bitmap(sourceImage);
      outputImage.RotateFlip(RotateFlipType.Rotate90FlipX);
      outputImage = ScaleImage(outputImage, vCoeffs, yScale);
      outputImage.RotateFlip(RotateFlipType.Rotate270FlipY);
      outputImage = ScaleImage(outputImage, hCoeffs, xScale);

      progressBar1.Hide();

      return outputImage;
    }

    private Bitmap ScaleImage(Bitmap sourceImage, Coeff[] coeffs, float scale)
    {
      float outputWidth = scale * sourceImage.Width;
      float outputHeight = sourceImage.Height;

      var input = new Bitmap(sourceImage);

      using (var output = new Bitmap((int)outputWidth, (int)outputHeight))
      {
        BitmapData srcData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData dstData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

        progressBar1.Value = 0;
        progressBar1.Maximum = dstData.Height;
        progressBar1.Visible = true;

        unsafe
        {
          int* srcPointer = (int*)srcData.Scan0;
          int* dstPointer = (int*)dstData.Scan0;

          for (var y = 0; y < dstData.Height; y++)
          {
            for (var x = 0; x < dstData.Width; x++)
            {
              var xx = x / scale;
              var c = GetValueOf(coeffs, xx % 1f);

              var t0 = Max((int)xx - 1, 0);
              var t1 = (int)xx;
              var t2 = Min((int)xx + 1, input.Width - 1);
              var t3 = Min((int)xx + 2, input.Width - 1);

              var pixels = new[]
              {
                Color.FromArgb(srcPointer[t0 + y * input.Width]),
                Color.FromArgb(srcPointer[t1 + y * input.Width]),
                Color.FromArgb(srcPointer[t2 + y * input.Width]),
                Color.FromArgb(srcPointer[t3 + y * input.Width])
              };

              dstPointer[0] = c * pixels;
              dstPointer++;
            }

            progressBar1.PerformStep();
            progressBar1.Update();
          }
        }

        output.UnlockBits(dstData);
        input.UnlockBits(srcData);

        return new Bitmap(output);
      }
    }

    private bool CompareCoeffArrays(Coeff[] a, Coeff[] b)
    {
      if ((a == null && b == null) || (a.Length == 0 && b.Length == 0))
        return true;

      if ((a == null && b != null) || (a != null && b == null))
        return false;

      for (var i = 0; i < a.Length; i++)
        if (!a[i].Equals(b[i]))
          return false;

      return true;
    }

    private static Coeff GetPreciseValueOf(Coeff[] c, float v)
    {
      var p = v * c.Length;

      var q = p % 1f;

      var r = (int)p;

      var a = c[r];
      var b = c[r + 1 == c.Length ? c.Length - 1 : r + 1];

      return a * (1 - q) + b * q;
    }

    private static Coeff GetValueOf(Coeff[] coeffs, float v)
    {
      var p = v * coeffs.Length;
      return coeffs[(int)p];
    }

    private static int Min(int v1, int v2)
    {
      return v1 < v2 ? v1 : v2;
    }

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
        }
        catch (ArgumentException)
        {
          MessageBox.Show(this, "The selected file was not a valid image.", "Invalid image file", MessageBoxButtons.OK, MessageBoxIcon.Error);
          image = null;
        }
        finally
        {
          isChanged = true; // force update
          timer1.Stop();
          timer1.Start();
        }
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      ShiftRight(hCoeffs);
      WriteCoeffs();
    }

    private void button3_Click(object sender, EventArgs e)
    {
      ShiftRight(vCoeffs);
      WriteCoeffs();
    }

    private void WriteCoeffs()
    {
      var sb = new StringBuilder();

      sb.AppendLine("# horizontal coefficients");

      foreach(var c in hCoeffs)
        sb.AppendLine($"{c.A,4},{c.B,4},{c.C,4},{c.D,4}");

      sb.AppendLine();
      sb.AppendLine("# vertical coefficients");

      foreach (var c in vCoeffs)
        sb.AppendLine($"{c.A,4},{c.B,4},{c.C,4},{c.D,4}");

      textBox1.Text = sb.ToString();
    }

    private void ShiftRight(Coeff[] c)
    {
      var cLast = c[c.Length - 1];

      for (var i = c.Length - 1; i > 0; i--)
        c[i] = c[i - 1];

      c[0] = new Coeff(cLast.D, cLast.C, cLast.B, cLast.A);

      Cursor = Cursors.WaitCursor;
      pictureBox1.Image = ScaleImage(image ?? Resource.testImage);
      Cursor = Cursors.Default;
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
  }
}
