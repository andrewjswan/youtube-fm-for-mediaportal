namespace YouTubePlugin.Class.SiteItems
{
  partial class GenericSiteItemControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.cmb_time = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.txt_title = new System.Windows.Forms.TextBox();
      this.cmb_standardfeed = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.cmb_region = new System.Windows.Forms.ComboBox();
      this.button1 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // cmb_time
      // 
      this.cmb_time.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmb_time.FormattingEnabled = true;
      this.cmb_time.Location = new System.Drawing.Point(2, 55);
      this.cmb_time.Name = "cmb_time";
      this.cmb_time.Size = new System.Drawing.Size(287, 21);
      this.cmb_time.TabIndex = 0;
      this.cmb_time.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(4, 39);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(59, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Time frame";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(1, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(27, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Title";
      // 
      // txt_title
      // 
      this.txt_title.Location = new System.Drawing.Point(3, 16);
      this.txt_title.Name = "txt_title";
      this.txt_title.Size = new System.Drawing.Size(285, 20);
      this.txt_title.TabIndex = 3;
      this.txt_title.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // cmb_standardfeed
      // 
      this.cmb_standardfeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmb_standardfeed.FormattingEnabled = true;
      this.cmb_standardfeed.Location = new System.Drawing.Point(1, 95);
      this.cmb_standardfeed.Name = "cmb_standardfeed";
      this.cmb_standardfeed.Size = new System.Drawing.Size(287, 21);
      this.cmb_standardfeed.TabIndex = 4;
      this.cmb_standardfeed.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(3, 79);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(79, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "Standard feeds";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(4, 119);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(41, 13);
      this.label10.TabIndex = 6;
      this.label10.Text = "Region";
      // 
      // cmb_region
      // 
      this.cmb_region.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmb_region.FormattingEnabled = true;
      this.cmb_region.Location = new System.Drawing.Point(1, 135);
      this.cmb_region.Name = "cmb_region";
      this.cmb_region.Size = new System.Drawing.Size(287, 21);
      this.cmb_region.TabIndex = 7;
      this.cmb_region.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(189, 197);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(99, 23);
      this.button1.TabIndex = 8;
      this.button1.Text = "Generate title";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // GenericSiteItemControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(this.button1);
      this.Controls.Add(this.cmb_region);
      this.Controls.Add(this.label10);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.cmb_standardfeed);
      this.Controls.Add(this.txt_title);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.cmb_time);
      this.Name = "GenericSiteItemControl";
      this.Size = new System.Drawing.Size(291, 223);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox cmb_time;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox txt_title;
    private System.Windows.Forms.ComboBox cmb_standardfeed;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.ComboBox cmb_region;
    private System.Windows.Forms.Button button1;
  }
}
