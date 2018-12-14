﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
  public partial class Form1 : Form
  {
    private Coeff[] vCoeffs;
    private Coeff[] hCoeffs;

    private float xScale;
    private float yScale;

    private float brightness = 1f;

    private bool isChanged;

    private Bitmap image = null;
    private string imageName = null;

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
        Label = label;
        FilePath = filePath;
      }

      public override string ToString()
      {
        return Label;
      }

      public string Label;
      public string FilePath;
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
          hCoeffs[lineIndex] = new Coeff(0, 128, 0, 0);
        else
          vCoeffs[lineIndex - 16] = new Coeff(0, 128, 0, 0);

        lineIndex++;
      }

      brightness = (hCoeffs.Sum(c => c.Sum()) + vCoeffs.Sum(c => c.Sum())) / 4096f;

      return !(CompareCoeffArrays(hCoeffs, oldH) && CompareCoeffArrays(vCoeffs, oldV));
    }

    private Bitmap ScaleImage(Bitmap sourceImage)
    {
      var outputImage = new Bitmap(sourceImage); // AdjustGamma(sourceImage, brightness);
      outputImage.RotateFlip(RotateFlipType.Rotate90FlipX);
      outputImage = ScaleImage(outputImage, vCoeffs, yScale);
      outputImage.RotateFlip(RotateFlipType.Rotate270FlipY);
      outputImage = ScaleImage(outputImage, hCoeffs, xScale);

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

        unsafe
        {
          int* srcPointer = (int*)srcData.Scan0;
          var inputWidth = input.Width;

          Parallel.For(0, dstData.Height, y =>
          {
            int* dstPointer = (int*)(dstData.Scan0);
            dstPointer += y * dstData.Width;

            for (var x = 0; x < dstData.Width; x++)
            {
              var xx = x / scale;
              var c = GetValueOf(coeffs, (xx + 0.5f) % 1f);

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

    // Perform gamma correction on the image.
    private Bitmap AdjustGamma(Image image, float gamma)
    {
      // Set the ImageAttributes object's gamma value.
      ImageAttributes attributes = new ImageAttributes();
      attributes.SetGamma(gamma);

      // Draw the image onto the new bitmap
      // while applying the new gamma value.
      Point[] points =
      {
        new Point(0, 0),
        new Point(image.Width, 0),
        new Point(0, image.Height),
    };
      Rectangle rect =
          new Rectangle(0, 0, image.Width, image.Height);

      // Make the result bitmap.
      Bitmap bm = new Bitmap(image.Width, image.Height);
      using (Graphics gr = Graphics.FromImage(bm))
      {
        gr.DrawImage(image, points, rect,
            GraphicsUnit.Pixel, attributes);
      }

      // Return the result.
      return bm;
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

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!(comboBox1.SelectedItem is ComboBoxItem selectedItem && File.Exists(selectedItem.FilePath)))
        return;

      textBox1.Text = File.ReadAllText(selectedItem.FilePath);
    }

    private void button3_Click_1(object sender, EventArgs e)
    {
      string filePath;

      if (!(comboBox1.SelectedItem is ComboBoxItem selectedItem && File.Exists(selectedItem.FilePath)))
      {
        int index = 0;
        var fileName = comboBox1.Text;
        if (string.IsNullOrWhiteSpace(fileName))
          fileName = "New_Filter";

        do
        {
          index++;
          filePath = Path.Combine(Directory.GetCurrentDirectory(), filtersPath, $"{comboBox1.Text.Trim()}{(index > 1 ? "_" + index : "")}.txt");
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
        button3_Click(sender, new EventArgs());
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
