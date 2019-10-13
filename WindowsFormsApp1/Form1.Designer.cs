namespace WindowsFormsApp1
{
  partial class Form
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form));
            this.filterTextBox = new System.Windows.Forms.TextBox();
            this.PreviewPictureBox = new System.Windows.Forms.PictureBox();
            this.RedrawTimer = new System.Windows.Forms.Timer(this.components);
            this.ScaleXUpDown = new System.Windows.Forms.NumericUpDown();
            this.ScaleYUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.loadImageBtn = new System.Windows.Forms.Button();
            this.OpenImageDialog = new System.Windows.Forms.OpenFileDialog();
            this.AutomaticRedrawChkBox = new System.Windows.Forms.CheckBox();
            this.updateBtn = new System.Windows.Forms.Button();
            this.FilterComboBox = new System.Windows.Forms.ComboBox();
            this.SaveBtn = new System.Windows.Forms.Button();
            this.savePreviewBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleXUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleYUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // filterTextBox
            // 
            this.filterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.filterTextBox.Font = new System.Drawing.Font("Lucida Console", 12.25F);
            this.filterTextBox.Location = new System.Drawing.Point(12, 40);
            this.filterTextBox.Multiline = true;
            this.filterTextBox.Name = "filterTextBox";
            this.filterTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.filterTextBox.Size = new System.Drawing.Size(450, 419);
            this.filterTextBox.TabIndex = 0;
            this.filterTextBox.Text = resources.GetString("filterTextBox.Text");
            this.filterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.filterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            // 
            // PreviewPictureBox
            // 
            this.PreviewPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PreviewPictureBox.Location = new System.Drawing.Point(468, 12);
            this.PreviewPictureBox.Name = "PreviewPictureBox";
            this.PreviewPictureBox.Size = new System.Drawing.Size(633, 418);
            this.PreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.PreviewPictureBox.TabIndex = 1;
            this.PreviewPictureBox.TabStop = false;
            this.PreviewPictureBox.Click += new System.EventHandler(this.PreviewPictureBox_Click);
            // 
            // RedrawTimer
            // 
            this.RedrawTimer.Tick += new System.EventHandler(this.RedrawTimer_Tick);
            // 
            // ScaleXUpDown
            // 
            this.ScaleXUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ScaleXUpDown.DecimalPlaces = 3;
            this.ScaleXUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ScaleXUpDown.Location = new System.Drawing.Point(914, 440);
            this.ScaleXUpDown.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.ScaleXUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ScaleXUpDown.Name = "ScaleXUpDown";
            this.ScaleXUpDown.Size = new System.Drawing.Size(60, 20);
            this.ScaleXUpDown.TabIndex = 2;
            this.ScaleXUpDown.Value = new decimal(new int[] {
            45,
            0,
            0,
            65536});
            this.ScaleXUpDown.ValueChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            // 
            // ScaleYUpDown
            // 
            this.ScaleYUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ScaleYUpDown.DecimalPlaces = 3;
            this.ScaleYUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ScaleYUpDown.Location = new System.Drawing.Point(1041, 440);
            this.ScaleYUpDown.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.ScaleYUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ScaleYUpDown.Name = "ScaleYUpDown";
            this.ScaleYUpDown.Size = new System.Drawing.Size(60, 20);
            this.ScaleYUpDown.TabIndex = 3;
            this.ScaleYUpDown.Value = new decimal(new int[] {
            45,
            0,
            0,
            65536});
            this.ScaleYUpDown.ValueChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(866, 442);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "X scale";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(993, 442);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Y scale";
            // 
            // loadImageBtn
            // 
            this.loadImageBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.loadImageBtn.Location = new System.Drawing.Point(468, 437);
            this.loadImageBtn.Name = "loadImageBtn";
            this.loadImageBtn.Size = new System.Drawing.Size(75, 23);
            this.loadImageBtn.TabIndex = 6;
            this.loadImageBtn.Text = "Load image";
            this.loadImageBtn.UseVisualStyleBackColor = true;
            this.loadImageBtn.Click += new System.EventHandler(this.LoadImageBtn_Click);
            // 
            // OpenImageDialog
            // 
            this.OpenImageDialog.Filter = "Image files|*.png;*.bmp;*.gif;*.jpg;*.jpeg";
            // 
            // AutomaticRedrawChkBox
            // 
            this.AutomaticRedrawChkBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AutomaticRedrawChkBox.AutoSize = true;
            this.AutomaticRedrawChkBox.Checked = true;
            this.AutomaticRedrawChkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutomaticRedrawChkBox.Location = new System.Drawing.Point(736, 441);
            this.AutomaticRedrawChkBox.Name = "AutomaticRedrawChkBox";
            this.AutomaticRedrawChkBox.Size = new System.Drawing.Size(108, 17);
            this.AutomaticRedrawChkBox.TabIndex = 10;
            this.AutomaticRedrawChkBox.Text = "Automatic redraw";
            this.AutomaticRedrawChkBox.UseVisualStyleBackColor = true;
            this.AutomaticRedrawChkBox.CheckedChanged += new System.EventHandler(this.AutomaticRedrawChkBox_CheckedChanged);
            // 
            // updateBtn
            // 
            this.updateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.updateBtn.Enabled = false;
            this.updateBtn.Location = new System.Drawing.Point(655, 437);
            this.updateBtn.Name = "updateBtn";
            this.updateBtn.Size = new System.Drawing.Size(75, 23);
            this.updateBtn.TabIndex = 11;
            this.updateBtn.Text = "Update";
            this.updateBtn.UseVisualStyleBackColor = true;
            this.updateBtn.Click += new System.EventHandler(this.UpdateBtn_Click);
            // 
            // FilterComboBox
            // 
            this.FilterComboBox.FormattingEnabled = true;
            this.FilterComboBox.Location = new System.Drawing.Point(13, 13);
            this.FilterComboBox.Name = "FilterComboBox";
            this.FilterComboBox.Size = new System.Drawing.Size(368, 21);
            this.FilterComboBox.Sorted = true;
            this.FilterComboBox.TabIndex = 12;
            // 
            // SaveBtn
            // 
            this.SaveBtn.Location = new System.Drawing.Point(387, 12);
            this.SaveBtn.Name = "SaveBtn";
            this.SaveBtn.Size = new System.Drawing.Size(75, 23);
            this.SaveBtn.TabIndex = 13;
            this.SaveBtn.Text = "Save";
            this.SaveBtn.UseVisualStyleBackColor = true;
            this.SaveBtn.Click += new System.EventHandler(this.SaveBtn_Click);
            // 
            // savePreviewBtn
            // 
            this.savePreviewBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.savePreviewBtn.Location = new System.Drawing.Point(549, 437);
            this.savePreviewBtn.Name = "savePreviewBtn";
            this.savePreviewBtn.Size = new System.Drawing.Size(100, 23);
            this.savePreviewBtn.TabIndex = 14;
            this.savePreviewBtn.Text = "Save preview";
            this.savePreviewBtn.UseVisualStyleBackColor = true;
            this.savePreviewBtn.Click += new System.EventHandler(this.SavePreviewBtn_Click);
            // 
            // Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1113, 471);
            this.Controls.Add(this.savePreviewBtn);
            this.Controls.Add(this.SaveBtn);
            this.Controls.Add(this.FilterComboBox);
            this.Controls.Add(this.updateBtn);
            this.Controls.Add(this.AutomaticRedrawChkBox);
            this.Controls.Add(this.loadImageBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ScaleYUpDown);
            this.Controls.Add(this.ScaleXUpDown);
            this.Controls.Add(this.PreviewPictureBox);
            this.Controls.Add(this.filterTextBox);
            this.Name = "Form";
            this.ShowIcon = false;
            this.Text = "Polyphase Previewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleXUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleYUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox filterTextBox;
    private System.Windows.Forms.PictureBox PreviewPictureBox;
    private System.Windows.Forms.Timer RedrawTimer;
    private System.Windows.Forms.NumericUpDown ScaleXUpDown;
    private System.Windows.Forms.NumericUpDown ScaleYUpDown;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button loadImageBtn;
    private System.Windows.Forms.OpenFileDialog OpenImageDialog;
    private System.Windows.Forms.CheckBox AutomaticRedrawChkBox;
    private System.Windows.Forms.Button updateBtn;
    private System.Windows.Forms.ComboBox FilterComboBox;
    private System.Windows.Forms.Button SaveBtn;
    private System.Windows.Forms.Button savePreviewBtn;
  }
}

