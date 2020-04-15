using System.Windows.Forms.VisualStyles;

namespace PolyphasePreviewer
{
    partial class Form: System.Windows.Forms.Form
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
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.RedrawTimer = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.loadImageBtn = new System.Windows.Forms.Button();
            this.OpenImageDialog = new System.Windows.Forms.OpenFileDialog();
            this.updateBtn = new System.Windows.Forms.Button();
            this.FilterComboBox = new System.Windows.Forms.ComboBox();
            this.SaveBtn = new System.Windows.Forms.Button();
            this.savePreviewBtn = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.GammaLutComboBox = new System.Windows.Forms.ComboBox();
            this.PreviewPictureBox = new System.Windows.Forms.PictureBox();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.AutomaticRedrawChkBox = new System.Windows.Forms.CheckBox();
            this.ScaleYUpDown = new System.Windows.Forms.NumericUpDown();
            this.ScaleXUpDown = new System.Windows.Forms.NumericUpDown();
            this.ErrorsAndWarningsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleYUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleXUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.FilterTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterTextBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FilterTextBox.Location = new System.Drawing.Point(12, 40);
            this.FilterTextBox.Multiline = true;
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.FilterTextBox.Size = new System.Drawing.Size(450, 599);
            this.FilterTextBox.TabIndex = 0;
            this.FilterTextBox.Text = resources.GetString("FilterTextBox.Text");
            this.FilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            // 
            // RedrawTimer
            // 
            this.RedrawTimer.Interval = 50;
            this.RedrawTimer.Tick += new System.EventHandler(this.RedrawTimer_Tick);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(240, 705);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "X Scale";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(354, 705);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Y Scale";
            // 
            // loadImageBtn
            // 
            this.loadImageBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.loadImageBtn.Location = new System.Drawing.Point(12, 671);
            this.loadImageBtn.Name = "loadImageBtn";
            this.loadImageBtn.Size = new System.Drawing.Size(100, 23);
            this.loadImageBtn.TabIndex = 6;
            this.loadImageBtn.Text = "Load Image";
            this.loadImageBtn.Click += new System.EventHandler(this.LoadImageBtn_Click);
            // 
            // OpenImageDialog
            // 
            this.OpenImageDialog.Filter = "Image files|*.png;*.bmp;*.gif;*.jpg;*.jpeg";
            // 
            // updateBtn
            // 
            this.updateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.updateBtn.Enabled = false;
            this.updateBtn.Location = new System.Drawing.Point(12, 700);
            this.updateBtn.Name = "updateBtn";
            this.updateBtn.Size = new System.Drawing.Size(100, 23);
            this.updateBtn.TabIndex = 11;
            this.updateBtn.Text = "Update";
            this.updateBtn.Click += new System.EventHandler(this.UpdateBtn_Click);
            // 
            // FilterComboBox
            // 
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
            this.SaveBtn.Click += new System.EventHandler(this.SaveBtn_Click);
            // 
            // savePreviewBtn
            // 
            this.savePreviewBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.savePreviewBtn.Location = new System.Drawing.Point(118, 671);
            this.savePreviewBtn.Name = "savePreviewBtn";
            this.savePreviewBtn.Size = new System.Drawing.Size(108, 23);
            this.savePreviewBtn.TabIndex = 14;
            this.savePreviewBtn.Text = "Save Preview";
            this.savePreviewBtn.Click += new System.EventHandler(this.SavePreviewBtn_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(240, 676);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Gamma LUT";
            // 
            // GammaLutComboBox
            // 
            this.GammaLutComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GammaLutComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.GammaLutComboBox.Location = new System.Drawing.Point(313, 673);
            this.GammaLutComboBox.Name = "GammaLutComboBox";
            this.GammaLutComboBox.Size = new System.Drawing.Size(149, 21);
            this.GammaLutComboBox.TabIndex = 18;
            // 
            // PreviewPictureBox
            // 
            this.PreviewPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewPictureBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(22)))));
            this.PreviewPictureBox.ErrorImage = null;
            this.PreviewPictureBox.Location = new System.Drawing.Point(468, 12);
            this.PreviewPictureBox.Name = "PreviewPictureBox";
            this.PreviewPictureBox.Size = new System.Drawing.Size(720, 711);
            this.PreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.PreviewPictureBox.TabIndex = 1;
            this.PreviewPictureBox.TabStop = false;
            this.PreviewPictureBox.Click += new System.EventHandler(this.PreviewPictureBox_Click);
            // 
            // AutomaticRedrawChkBox
            // 
            this.AutomaticRedrawChkBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AutomaticRedrawChkBox.AutoSize = true;
            this.AutomaticRedrawChkBox.Checked = global::PolyphasePreviewer.Properties.Settings.Default.AutomaticRedraw;
            this.AutomaticRedrawChkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutomaticRedrawChkBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::PolyphasePreviewer.Properties.Settings.Default, "AutomaticRedraw", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.AutomaticRedrawChkBox.Location = new System.Drawing.Point(118, 704);
            this.AutomaticRedrawChkBox.Name = "AutomaticRedrawChkBox";
            this.AutomaticRedrawChkBox.Size = new System.Drawing.Size(113, 17);
            this.AutomaticRedrawChkBox.TabIndex = 10;
            this.AutomaticRedrawChkBox.Text = "Automatic Redraw";
            this.AutomaticRedrawChkBox.CheckedChanged += new System.EventHandler(this.AutomaticRedrawChkBox_CheckedChanged);
            // 
            // ScaleYUpDown
            // 
            this.ScaleYUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScaleYUpDown.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::PolyphasePreviewer.Properties.Settings.Default, "ScaleY", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ScaleYUpDown.DecimalPlaces = 3;
            this.ScaleYUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ScaleYUpDown.Location = new System.Drawing.Point(402, 703);
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
            this.ScaleYUpDown.Value = global::PolyphasePreviewer.Properties.Settings.Default.ScaleY;
            this.ScaleYUpDown.ValueChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            // 
            // ScaleXUpDown
            // 
            this.ScaleXUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScaleXUpDown.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::PolyphasePreviewer.Properties.Settings.Default, "ScaleX", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ScaleXUpDown.DecimalPlaces = 3;
            this.ScaleXUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ScaleXUpDown.Location = new System.Drawing.Point(288, 703);
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
            this.ScaleXUpDown.Value = global::PolyphasePreviewer.Properties.Settings.Default.ScaleX;
            this.ScaleXUpDown.ValueChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            // 
            // ErrorsAndWarningsLabel
            // 
            this.ErrorsAndWarningsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ErrorsAndWarningsLabel.AutoSize = true;
            this.ErrorsAndWarningsLabel.ForeColor = System.Drawing.Color.Red;
            this.ErrorsAndWarningsLabel.Location = new System.Drawing.Point(12, 642);
            this.ErrorsAndWarningsLabel.Name = "ErrorsAndWarningsLabel";
            this.ErrorsAndWarningsLabel.Size = new System.Drawing.Size(0, 13);
            this.ErrorsAndWarningsLabel.TabIndex = 19;
            // 
            // Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 735);
            this.Controls.Add(this.ErrorsAndWarningsLabel);
            this.Controls.Add(this.GammaLutComboBox);
            this.Controls.Add(this.label3);
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
            this.Controls.Add(this.FilterTextBox);
            this.Name = "Form";
            this.Text = "MiSTer Polyphase Previewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleYUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScaleXUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox PreviewPictureBox;
        private System.Windows.Forms.Timer RedrawTimer;
        private System.Windows.Forms.OpenFileDialog OpenImageDialog;
        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.NumericUpDown ScaleXUpDown;
        private System.Windows.Forms.NumericUpDown ScaleYUpDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button loadImageBtn;
        private System.Windows.Forms.CheckBox AutomaticRedrawChkBox;
        private System.Windows.Forms.Button updateBtn;
        private System.Windows.Forms.ComboBox FilterComboBox;
        private System.Windows.Forms.Button SaveBtn;
        private System.Windows.Forms.Button savePreviewBtn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox GammaLutComboBox;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.Label ErrorsAndWarningsLabel;
    }
}

