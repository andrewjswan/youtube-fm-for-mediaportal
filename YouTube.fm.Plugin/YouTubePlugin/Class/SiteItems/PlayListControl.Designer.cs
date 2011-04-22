namespace YouTubePlugin.Class.SiteItems
{
  partial class PlayListControl
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
      this.label1 = new System.Windows.Forms.Label();
      this.txt_id = new System.Windows.Forms.TextBox();
      this.txt_title = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 39);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(51, 13);
      this.label1.TabIndex = 15;
      this.label1.Text = "Playlist Id";
      // 
      // txt_id
      // 
      this.txt_id.Location = new System.Drawing.Point(1, 58);
      this.txt_id.Name = "txt_id";
      this.txt_id.Size = new System.Drawing.Size(281, 20);
      this.txt_id.TabIndex = 14;
      this.txt_id.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // txt_title
      // 
      this.txt_title.Location = new System.Drawing.Point(1, 16);
      this.txt_title.Name = "txt_title";
      this.txt_title.Size = new System.Drawing.Size(281, 20);
      this.txt_title.TabIndex = 13;
      this.txt_title.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(27, 13);
      this.label2.TabIndex = 12;
      this.label2.Text = "Title";
      // 
      // PlayListControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txt_id);
      this.Controls.Add(this.txt_title);
      this.Controls.Add(this.label2);
      this.Name = "PlayListControl";
      this.Size = new System.Drawing.Size(285, 217);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txt_id;
    private System.Windows.Forms.TextBox txt_title;
    private System.Windows.Forms.Label label2;
  }
}
