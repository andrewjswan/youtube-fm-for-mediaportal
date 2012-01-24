namespace TestClient
{
  partial class Client
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
      this.button1 = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.btn_config = new System.Windows.Forms.Button();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(139, 62);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "Send";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(12, 36);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(202, 20);
      this.textBox1.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 98);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(202, 25);
      this.label1.TabIndex = 3;
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // btn_config
      // 
      this.btn_config.Image = global::YoutubeFmClient.Properties.Resources.emblem_system;
      this.btn_config.Location = new System.Drawing.Point(12, 62);
      this.btn_config.Name = "btn_config";
      this.btn_config.Size = new System.Drawing.Size(35, 33);
      this.btn_config.TabIndex = 4;
      this.btn_config.UseVisualStyleBackColor = true;
      this.btn_config.Click += new System.EventHandler(this.btn_config_Click);
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.Location = new System.Drawing.Point(53, 66);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(80, 17);
      this.checkBox1.TabIndex = 5;
      this.checkBox1.Text = "Stay on top";
      this.checkBox1.UseVisualStyleBackColor = true;
      this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
      // 
      // comboBox1
      // 
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Items.AddRange(new object[] {
            "Play ",
            "Search ",
            "Artist"});
      this.comboBox1.Location = new System.Drawing.Point(12, 9);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(202, 21);
      this.comboBox1.TabIndex = 6;
      // 
      // Client
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(222, 136);
      this.Controls.Add(this.comboBox1);
      this.Controls.Add(this.checkBox1);
      this.Controls.Add(this.btn_config);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.button1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "Client";
      this.Text = "YoutubeFmRemote";
      this.Load += new System.EventHandler(this.Client_Load);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Client_DragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Client_DragEnter);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btn_config;
    private System.Windows.Forms.CheckBox checkBox1;
    private System.Windows.Forms.ComboBox comboBox1;
  }
}

