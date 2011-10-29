namespace YouTubePlugin.Class.SiteItems
{
  partial class DiscoControl
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
      this.cmb_region = new System.Windows.Forms.ComboBox();
      this.label10 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // cmb_region
      // 
      this.cmb_region.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmb_region.FormattingEnabled = true;
      this.cmb_region.Location = new System.Drawing.Point(0, 24);
      this.cmb_region.Name = "cmb_region";
      this.cmb_region.Size = new System.Drawing.Size(287, 21);
      this.cmb_region.TabIndex = 9;
      this.cmb_region.SelectedIndexChanged += new System.EventHandler(this.cmb_region_SelectedIndexChanged);
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(3, 8);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(41, 13);
      this.label10.TabIndex = 8;
      this.label10.Text = "Region";
      // 
      // DiscoControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.label10);
      this.Controls.Add(this.cmb_region);
      this.Name = "DiscoControl";
      this.Size = new System.Drawing.Size(291, 223);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox cmb_region;
    private System.Windows.Forms.Label label10;
  }
}
