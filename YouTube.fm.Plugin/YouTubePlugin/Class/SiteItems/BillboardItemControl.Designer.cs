namespace YouTubePlugin.Class.SiteItems
{
  partial class BillboardItemControl
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
      this.button1 = new System.Windows.Forms.Button();
      this.cmb_standardfeed = new System.Windows.Forms.ComboBox();
      this.txt_title = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.chk_all = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(190, 198);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(99, 23);
      this.button1.TabIndex = 13;
      this.button1.Text = "Generate title";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // cmb_standardfeed
      // 
      this.cmb_standardfeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmb_standardfeed.FormattingEnabled = true;
      this.cmb_standardfeed.Location = new System.Drawing.Point(2, 84);
      this.cmb_standardfeed.Name = "cmb_standardfeed";
      this.cmb_standardfeed.Size = new System.Drawing.Size(287, 21);
      this.cmb_standardfeed.TabIndex = 11;
      this.cmb_standardfeed.SelectedIndexChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // txt_title
      // 
      this.txt_title.Location = new System.Drawing.Point(4, 17);
      this.txt_title.Name = "txt_title";
      this.txt_title.Size = new System.Drawing.Size(285, 20);
      this.txt_title.TabIndex = 10;
      this.txt_title.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(3, 59);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(31, 13);
      this.label3.TabIndex = 12;
      this.label3.Text = "Feed";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(2, 1);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(27, 13);
      this.label2.TabIndex = 9;
      this.label2.Text = "Title";
      // 
      // chk_all
      // 
      this.chk_all.AutoSize = true;
      this.chk_all.Location = new System.Drawing.Point(5, 125);
      this.chk_all.Name = "chk_all";
      this.chk_all.Size = new System.Drawing.Size(95, 17);
      this.chk_all.TabIndex = 14;
      this.chk_all.Text = "Show all feeds";
      this.chk_all.UseVisualStyleBackColor = true;
      this.chk_all.CheckedChanged += new System.EventHandler(this.chk_all_CheckedChanged);
      // 
      // BillboardItemControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.chk_all);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.cmb_standardfeed);
      this.Controls.Add(this.txt_title);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Name = "BillboardItemControl";
      this.Size = new System.Drawing.Size(291, 223);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.ComboBox cmb_standardfeed;
    private System.Windows.Forms.TextBox txt_title;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.CheckBox chk_all;
  }
}
