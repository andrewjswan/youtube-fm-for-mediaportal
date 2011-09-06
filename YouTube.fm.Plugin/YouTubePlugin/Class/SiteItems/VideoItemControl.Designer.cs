namespace YouTubePlugin.Class.SiteItems
{
  partial class VideoItemControl
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
      this.txt_videoId = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // txt_title
      // 
      this.txt_title.Location = new System.Drawing.Point(5, 25);
      this.txt_title.Name = "txt_title";
      this.txt_title.Size = new System.Drawing.Size(279, 20);
      this.txt_title.TabIndex = 2;
      this.txt_title.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(5, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(27, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Title";
      // 
      // txt_videoId
      // 
      this.txt_videoId.Location = new System.Drawing.Point(5, 68);
      this.txt_videoId.Name = "txt_videoId";
      this.txt_videoId.Size = new System.Drawing.Size(279, 20);
      this.txt_videoId.TabIndex = 4;
      this.txt_videoId.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(5, 52);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(89, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "Youtube Video Id";
      // 
      // VideoItemControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.txt_videoId);
      this.Controls.Add(this.txt_title);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.label2);
      this.Name = "VideoItemControl";
      this.Size = new System.Drawing.Size(287, 219);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txt_title;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txt_videoId;
    private System.Windows.Forms.Label label2;
  }
}
