namespace YouTubePlugin.Class.SiteItems
{
  partial class FolderControl
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
      this.txt_title = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // txt_title
      // 
      this.txt_title.Location = new System.Drawing.Point(3, 32);
      this.txt_title.Name = "txt_title";
      this.txt_title.Size = new System.Drawing.Size(279, 20);
      this.txt_title.TabIndex = 0;
      this.txt_title.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(27, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Title";
      // 
      // FolderControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.txt_title);
      this.Controls.Add(this.label1);
      this.Name = "FolderControl";
      this.Size = new System.Drawing.Size(285, 217);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txt_title;
    private System.Windows.Forms.Label label1;
  }
}
