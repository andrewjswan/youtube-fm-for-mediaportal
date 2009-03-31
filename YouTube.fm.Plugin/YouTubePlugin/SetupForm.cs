using System;
using System.Collections;
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

using MediaPortal.GUI.Library;


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
      _settings.ShowNowPlaying = checkBox_nowplaying.Checked;
      _settings.UseYouTubePlayer = checkBox_useplayer.Checked;
      _settings.UseExtremFilter = checkBox_extremfilter.Checked;
      _settings.VideoQuality = comboBox_videoquality.SelectedIndex;
      _settings.UseSMSStyleKeyBoard = checkBox_sms.Checked;
      _settings.InstantAction = (Action.ActionType)comboBox_action.SelectedValue;
      try
      {
      }
      catch (Exception)
      {
        _settings.InstantChar = 0;
      }

      foreach (string s in listBox_history.Items)
      {
        _settings.SearchHistory.Add(s);
      }
      if (radioButton1.Checked)
        _settings.InitialDisplay = 1;
      if (radioButton2.Checked)
        _settings.InitialDisplay = 2;
      if (radioButton3.Checked)
        _settings.InitialDisplay = 3;
      _settings.Save();
      this.Close();
    }

    private ArrayList GenerateActionList()
    {
      ArrayList ret = new ArrayList();
      string[] names = Enum.GetNames(typeof(Action.ActionType));
      int[] values = (int[])Enum.GetValues(typeof(Action.ActionType));
      for (int i = 0; i < names.Length; i++)
      {
        ret.Add(new ActionEntry(names[i], values[i]));
      }
      return ret;
    }


    private void SetupForm_Load(object sender, EventArgs e)
    {
      comboBox_action.DataSource = GenerateActionList();
      comboBox_action.DisplayMember = "ActionName";
      comboBox_action.ValueMember = "ActionID";

      textBox_user.Text = _settings.User;
      textBox_passw.Text = _settings.Password;
      textBox_pluginname.Text = _settings.PluginName;
      textBox_char.Text = _settings.InstantChar.ToString();
      listBox_history.Items.AddRange(_settings.SearchHistory.ToArray());
      checkBox_filter.Checked = _settings.MusicFilter;
      checkBox_time.Checked = _settings.Time;
      checkBox_nowplaying.Checked = _settings.ShowNowPlaying;
      checkBox_useplayer.Checked = _settings.UseYouTubePlayer;
      checkBox_extremfilter.Checked = _settings.UseExtremFilter;
      comboBox_videoquality.SelectedIndex = _settings.VideoQuality;
      checkBox_sms.Checked = _settings.UseSMSStyleKeyBoard;
      comboBox_action.SelectedValue = (int)_settings.InstantAction;
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

    private void tabPage3_Click(object sender, EventArgs e)
    {

    }

    private void checkBox_time_CheckedChanged(object sender, EventArgs e)
    {

    }

  }

  class ActionEntry
  {
    private string actionName;
    private int actionID;

    public string ActionName
    {
      get
      {
        return actionName;
      }
    }

    public int ActionID
    {
      get
      {
        return actionID;
      }
    }

    public ActionEntry(string Name, int ID)
    {

      this.actionName = Name;
      this.actionID = ID;
    }
  }

}