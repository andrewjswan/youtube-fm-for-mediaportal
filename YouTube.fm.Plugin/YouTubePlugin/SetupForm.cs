using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;


namespace YouTubePlugin
{
  public partial class SetupForm : Form
  {
    public Settings _settings;
    public SetupForm()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      _settings.User = textBox_user.Text;
      _settings.Password = textBox_passw.Text;
      _settings.PluginName = textBox_pluginname.Text;
      _settings.SearchHistory.Clear();
      _settings.InitialSearch = textBox_startup.Text;
      _settings.InitialCat = comboBox_startup.SelectedIndex;
      _settings.MusicFilter = checkBox_filter.Checked;
      _settings.Time = checkBox_time.Checked;
      foreach (string s in listBox_history.Items)
      {
        _settings.SearchHistory.Add(s);
      }
      _settings.Save();
      if (radioButton1.Checked)
        _settings.InitialDisplay = 1;
      if (radioButton2.Checked)
        _settings.InitialDisplay = 2;
      if (radioButton3.Checked)
        _settings.InitialDisplay = 3;

      this.Close();
    }

    private void SetupForm_Load(object sender, EventArgs e)
    {
      textBox_user.Text = _settings.User;
      textBox_passw.Text = _settings.Password;
      textBox_pluginname.Text = _settings.PluginName;
      listBox_history.Items.AddRange(_settings.SearchHistory.ToArray());
      checkBox_filter.Checked = _settings.MusicFilter;
      checkBox_time.Checked = _settings.Time;
      switch (_settings.InitialDisplay)
      {
       case 1:
         radioButton1.Checked = true;
         break;
        case 2:
          radioButton2.Checked = true;
          break;
        case 3:
          radioButton3.Checked = true;
          break;
        default:
          break;
      }
      comboBox_startup.Items.AddRange(_settings.Cats.ToArray());
      comboBox_startup.SelectedIndex = _settings.InitialCat;
      textBox_startup.Text = _settings.InitialSearch;
    }

    private void button2_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void button3_Click(object sender, EventArgs e)
    {
      listBox_history.Items.Clear();
    }

    private void button4_Click(object sender, EventArgs e)
    {
      listBox_history.Items.Add(textBox1.Text);
      textBox1.Text = "";
    }

    private void button5_Click(object sender, EventArgs e)
    {
      List<string> sel = new List<string>();
      foreach (string s in listBox_history.SelectedItems)
      {
        sel.Add(s);
      }
      foreach (string s in sel)
      {
        listBox_history.Items.Remove(s);
      }

    }

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton1.Checked)
      {
        radioButton2.Checked = false;
        comboBox_startup.Enabled = true;
        textBox_startup.Enabled = false;
        radioButton3.Checked = false;
      }
    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton2.Checked)
      {
        radioButton1.Checked = false;
        radioButton3.Checked = false;
        comboBox_startup.Enabled = false;
        textBox_startup.Enabled = true;

      }
    }

    private void radioButton3_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton3.Checked)
      {
        comboBox_startup.Enabled = false;
        textBox_startup.Enabled = false;

        radioButton1.Checked = false;
        radioButton2.Checked = false;
      }
    }


  }
}