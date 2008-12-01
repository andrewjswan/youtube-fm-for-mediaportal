namespace YouTubePlugin
{
  partial class SetupForm
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
      this.button2 = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.textBox_passw = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.textBox_user = new System.Windows.Forms.TextBox();
      this.textBox_pluginname = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.checkBox_filter = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.comboBox_startup = new System.Windows.Forms.ComboBox();
      this.textBox_startup = new System.Windows.Forms.TextBox();
      this.radioButton3 = new System.Windows.Forms.RadioButton();
      this.radioButton2 = new System.Windows.Forms.RadioButton();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.button5 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.button3 = new System.Windows.Forms.Button();
      this.listBox_history = new System.Windows.Forms.ListBox();
      this.checkBox_time = new System.Windows.Forms.CheckBox();
      this.groupBox1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(364, 364);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "OK";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(460, 364);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 1;
      this.button2.Text = "Cancel";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.textBox_passw);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.textBox_user);
      this.groupBox1.Location = new System.Drawing.Point(9, 223);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(254, 101);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Google Authentication";
      // 
      // textBox_passw
      // 
      this.textBox_passw.Location = new System.Drawing.Point(6, 71);
      this.textBox_passw.Name = "textBox_passw";
      this.textBox_passw.Size = new System.Drawing.Size(242, 20);
      this.textBox_passw.TabIndex = 6;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 55);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(53, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "Password";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(58, 13);
      this.label1.TabIndex = 4;
      this.label1.Text = "User name";
      // 
      // textBox_user
      // 
      this.textBox_user.Location = new System.Drawing.Point(6, 32);
      this.textBox_user.Name = "textBox_user";
      this.textBox_user.Size = new System.Drawing.Size(242, 20);
      this.textBox_user.TabIndex = 3;
      // 
      // textBox_pluginname
      // 
      this.textBox_pluginname.Location = new System.Drawing.Point(6, 31);
      this.textBox_pluginname.Name = "textBox_pluginname";
      this.textBox_pluginname.Size = new System.Drawing.Size(254, 20);
      this.textBox_pluginname.TabIndex = 3;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(7, 15);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(65, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Plugin name";
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Location = new System.Drawing.Point(1, 2);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(543, 356);
      this.tabControl1.TabIndex = 5;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.checkBox_time);
      this.tabPage1.Controls.Add(this.checkBox_filter);
      this.tabPage1.Controls.Add(this.groupBox2);
      this.tabPage1.Controls.Add(this.label3);
      this.tabPage1.Controls.Add(this.groupBox1);
      this.tabPage1.Controls.Add(this.textBox_pluginname);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(535, 330);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "General";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // checkBox_filter
      // 
      this.checkBox_filter.AutoSize = true;
      this.checkBox_filter.Location = new System.Drawing.Point(318, 31);
      this.checkBox_filter.Name = "checkBox_filter";
      this.checkBox_filter.Size = new System.Drawing.Size(159, 17);
      this.checkBox_filter.TabIndex = 6;
      this.checkBox_filter.Text = "Enable music videos filtering";
      this.checkBox_filter.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.comboBox_startup);
      this.groupBox2.Controls.Add(this.textBox_startup);
      this.groupBox2.Controls.Add(this.radioButton3);
      this.groupBox2.Controls.Add(this.radioButton2);
      this.groupBox2.Controls.Add(this.radioButton1);
      this.groupBox2.Location = new System.Drawing.Point(9, 58);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(254, 159);
      this.groupBox2.TabIndex = 5;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Startup";
      // 
      // comboBox_startup
      // 
      this.comboBox_startup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox_startup.FormattingEnabled = true;
      this.comboBox_startup.Location = new System.Drawing.Point(89, 19);
      this.comboBox_startup.Name = "comboBox_startup";
      this.comboBox_startup.Size = new System.Drawing.Size(150, 21);
      this.comboBox_startup.TabIndex = 4;
      // 
      // textBox_startup
      // 
      this.textBox_startup.Location = new System.Drawing.Point(89, 74);
      this.textBox_startup.Name = "textBox_startup";
      this.textBox_startup.Size = new System.Drawing.Size(150, 20);
      this.textBox_startup.TabIndex = 3;
      // 
      // radioButton3
      // 
      this.radioButton3.AutoSize = true;
      this.radioButton3.Location = new System.Drawing.Point(9, 124);
      this.radioButton3.Name = "radioButton3";
      this.radioButton3.Size = new System.Drawing.Size(43, 17);
      this.radioButton3.TabIndex = 2;
      this.radioButton3.TabStop = true;
      this.radioButton3.Text = "Ask";
      this.radioButton3.UseVisualStyleBackColor = true;
      this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
      // 
      // radioButton2
      // 
      this.radioButton2.AutoSize = true;
      this.radioButton2.Location = new System.Drawing.Point(9, 74);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(74, 17);
      this.radioButton2.TabIndex = 1;
      this.radioButton2.TabStop = true;
      this.radioButton2.Text = "Search for";
      this.radioButton2.UseVisualStyleBackColor = true;
      this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.Location = new System.Drawing.Point(9, 19);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(55, 17);
      this.radioButton1.TabIndex = 0;
      this.radioButton1.TabStop = true;
      this.radioButton1.Text = "Show ";
      this.radioButton1.UseVisualStyleBackColor = true;
      this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.button5);
      this.tabPage2.Controls.Add(this.button4);
      this.tabPage2.Controls.Add(this.textBox1);
      this.tabPage2.Controls.Add(this.button3);
      this.tabPage2.Controls.Add(this.listBox_history);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(535, 330);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Search history";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // button5
      // 
      this.button5.Location = new System.Drawing.Point(206, 61);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(130, 23);
      this.button5.TabIndex = 4;
      this.button5.Text = "Remove selected";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(342, 32);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(75, 23);
      this.button4.TabIndex = 3;
      this.button4.Text = "Add";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(206, 35);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(130, 20);
      this.textBox1.TabIndex = 2;
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(206, 6);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(130, 23);
      this.button3.TabIndex = 1;
      this.button3.Text = "Clear all";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // listBox_history
      // 
      this.listBox_history.FormattingEnabled = true;
      this.listBox_history.Location = new System.Drawing.Point(6, 6);
      this.listBox_history.Name = "listBox_history";
      this.listBox_history.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
      this.listBox_history.Size = new System.Drawing.Size(181, 316);
      this.listBox_history.TabIndex = 0;
      // 
      // checkBox_time
      // 
      this.checkBox_time.AutoSize = true;
      this.checkBox_time.Location = new System.Drawing.Point(318, 54);
      this.checkBox_time.Name = "checkBox_time";
      this.checkBox_time.Size = new System.Drawing.Size(113, 17);
      this.checkBox_time.TabIndex = 7;
      this.checkBox_time.Text = "Ask for time period";
      this.checkBox_time.UseVisualStyleBackColor = true;
      // 
      // SetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(544, 399);
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Name = "SetupForm";
      this.Text = "SetupForm";
      this.Load += new System.EventHandler(this.SetupForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.tabPage2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TextBox textBox_passw;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBox_user;
    private System.Windows.Forms.TextBox textBox_pluginname;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.ListBox listBox_history;
    private System.Windows.Forms.Button button5;
    private System.Windows.Forms.Button button4;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.RadioButton radioButton3;
    private System.Windows.Forms.RadioButton radioButton2;
    private System.Windows.Forms.RadioButton radioButton1;
    private System.Windows.Forms.ComboBox comboBox_startup;
    private System.Windows.Forms.TextBox textBox_startup;
    private System.Windows.Forms.CheckBox checkBox_filter;
    private System.Windows.Forms.CheckBox checkBox_time;
  }
}