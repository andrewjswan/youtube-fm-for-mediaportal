using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestClient
{
  public partial class SettingsForm : Form
  {
    public Settings Settings { get; set; }
    public SettingsForm()
    {
      InitializeComponent();
      Settings = new Settings();
    }

    private void SettingsForm_Load(object sender, EventArgs e)
    {
      txt_server.Text = Settings.Host;
      txt_port.Text = Settings.Port.ToString();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      try
      {
        Settings.Port = Convert.ToInt32(txt_port.Text);
      }
      catch (Exception)
      {
        MessageBox.Show("Wrong port value !!");
        return;
      }
      Settings.Host = txt_server.Text;
      Settings.Save();
      Close();
     }

  }
}
