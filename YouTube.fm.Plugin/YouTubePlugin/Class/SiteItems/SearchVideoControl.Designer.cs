namespace YouTubePlugin.Class.SiteItems
{
  partial class SearchVideoControl
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
      this.label2 = new System.Windows.Forms.Label();
      this.button1 = new System.Windows.Forms.Button();
      this.txt_term = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cmb_sort = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.cmb_time = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // txt_title
      // 
      this.txt_title.Location = new System.Drawing.Point(4, 19);
      this.txt_title.Name = "txt_title";
      this.txt_title.Size = new System.Drawing.Size(287, 20);
      this.txt_title.TabIndex = 5;
      this.txt_title.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(4, 3);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(27, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Title";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(189, 197);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(99, 23);
      this.button1.TabIndex = 9;
      this.button1.Text = "Generate title";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // txt_term
      // 
      this.txt_term.Location = new System.Drawing.Point(4, 59);
      this.txt_term.Name = "txt_term";
      this.txt_term.Size = new System.Drawing.Size(287, 20);
      this.txt_term.TabIndex = 10;
      this.txt_term.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(4, 43);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 13);
      this.label1.TabIndex = 11;
      this.label1.Text = "Search term";
      // 
      // cmb_sort
      // 
      this.cmb_sort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmb_sort.FormattingEnabled = true;
      this.cmb_sort.Items.AddRange(new object[] {
            "relevance – Entries are ordered by their relevance to a search query. This is the" +
                " default setting for video search results feeds.",
            "published – Entries are returned in reverse chronological order. This is the defa" +
                "ult value for video feeds other than search results feeds.",
            "viewCount – Entries are ordered from most views to least views.",
            "rating – Entries are ordered from highest rating to lowest rating"});
      this.cmb_sort.Location = new System.Drawing.Point(4, 98);
      this.cmb_sort.Name = "cmb_sort";
      this.cmb_sort.Size = new System.Drawing.Size(284, 21);
      this.cmb_sort.TabIndex = 12;
      this.cmb_sort.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(4, 82);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(40, 13);
      this.label3.TabIndex = 13;
      this.label3.Text = "Sort by";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(4, 122);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(59, 13);
      this.label4.TabIndex = 15;
      this.label4.Text = "Time frame";
      // 
      // cmb_time
      // 
      this.cmb_time.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmb_time.FormattingEnabled = true;
      this.cmb_time.Location = new System.Drawing.Point(4, 142);
      this.cmb_time.Name = "cmb_time";
      this.cmb_time.Size = new System.Drawing.Size(287, 21);
      this.cmb_time.TabIndex = 14;
      this.cmb_time.TextChanged += new System.EventHandler(this.txt_title_TextChanged);
      // 
      // SearchVideoControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(this.label4);
      this.Controls.Add(this.cmb_time);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.cmb_sort);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txt_term);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.txt_title);
      this.Controls.Add(this.label2);
      this.Name = "SearchVideoControl";
      this.Size = new System.Drawing.Size(289, 221);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txt_title;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.TextBox txt_term;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox cmb_sort;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox cmb_time;
  }
}
