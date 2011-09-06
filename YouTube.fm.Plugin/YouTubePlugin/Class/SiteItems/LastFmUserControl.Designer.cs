namespace YouTubePlugin.Class.SiteItems
{
  partial class LastFmUserControl
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
      this.cmb_type = new System.Windows.Forms.ComboBox();
      this.txt_title = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // cmb_type
      // 
      this.cmb_type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmb_type.FormattingEnabled = true;
      this.cmb_type.Items.AddRange(new object[] {
            "TopTracks"});
      this.cmb_type.Location = new System.Drawing.Point(0, 69);
      this.cmb_type.Name = "cmb_type";
      this.cmb_type.Size = new System.Drawing.Size(279, 21);
      this.cmb_type.TabIndex = 10;
      this.cmb_type.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // txt_title
      // 
      this.txt_title.Location = new System.Drawing.Point(0, 19);
      this.txt_title.Name = "txt_title";
      this.txt_title.Size = new System.Drawing.Size(279, 20);
      this.txt_title.TabIndex = 8;
      this.txt_title.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(0, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(27, 13);
      this.label1.TabIndex = 9;
      this.label1.Text = "Title";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(3, 53);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(31, 13);
      this.label2.TabIndex = 11;
      this.label2.Text = "Type";
      // 
      // LastFmUserControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.cmb_type);
      this.Controls.Add(this.txt_title);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.label2);
      this.Name = "LastFmUserControl";
      this.Size = new System.Drawing.Size(287, 219);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox cmb_type;
    private System.Windows.Forms.TextBox txt_title;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
  }
}
