namespace YouTubePlugin.Class.SiteItems
{
  partial class UserPlaylitsControl
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
      this.txt_user = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.txt_title = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // txt_user
      // 
      this.txt_user.Location = new System.Drawing.Point(3, 70);
      this.txt_user.Name = "txt_user";
      this.txt_user.Size = new System.Drawing.Size(281, 20);
      this.txt_user.TabIndex = 0;
      this.txt_user.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(5, 54);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(41, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "User Id";
      // 
      // txt_title
      // 
      this.txt_title.Location = new System.Drawing.Point(3, 18);
      this.txt_title.Name = "txt_title";
      this.txt_title.Size = new System.Drawing.Size(281, 20);
      this.txt_title.TabIndex = 15;
      this.txt_title.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(5, 2);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(27, 13);
      this.label2.TabIndex = 14;
      this.label2.Text = "Title";
      // 
      // UserPlaylitsControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.txt_title);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txt_user);
      this.Name = "UserPlaylitsControl";
      this.Size = new System.Drawing.Size(287, 219);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txt_user;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txt_title;
    private System.Windows.Forms.Label label2;
  }
}
