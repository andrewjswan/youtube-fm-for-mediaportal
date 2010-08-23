using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace YouTubePlugin.Class.SiteItems
{
  public partial class GenericSiteItemControl : UserControl
  {
    private bool loading = false;
    private SiteItemEntry _entry = new SiteItemEntry();
    Dictionary<string, string> setting = new Dictionary<string, string>();

    public GenericSiteItemControl()
    {
      InitializeComponent();
    }

    public void SetEntry(SiteItemEntry entry)
    {
      _entry = entry;
      cmb_time.Items.Clear();
      cmb_standardfeed.Items.Clear();
      cmb_region.Items.Clear();
      cmb_time.Items.Add("");
      foreach (string s in Youtube2MP._settings.TimeList)
      {
        cmb_time.Items.Add(s);
      }
      cmb_standardfeed.Items.Add("");
      cmb_standardfeed.Items.AddRange(Youtube2MP._settings.Cats.ToArray());

      cmb_region.Items.Add("");
      foreach (KeyValuePair<string, string> valuePair in Youtube2MP._settings.Regions)
      {
        cmb_region.Items.Add(valuePair.Key);
      }

     
      loading = false;
      PharseSettings(entry.ConfigString);
      txt_title.Text = GetValue("title");
      cmb_region.SelectedItem = GetValue("region");
      cmb_time.SelectedItem = GetValue("time");
      cmb_standardfeed.SelectedItem = GetValue("feed");
      loading = false;
    }

    private void txt_title_TextChanged(object sender, EventArgs e)
    {
      if(!loading)
      {
        SetValue("title", txt_title.Text);
        SetValue("region", (string)cmb_region.SelectedItem);
        SetValue("time", (string)cmb_time.SelectedItem);
        SetValue("feed", (string)cmb_standardfeed.SelectedItem);
        _entry.ConfigString = GetConfigString();
      }
    }


    public string GetValue(string en)
    {
      if (setting.ContainsKey(en))
        return setting[en];
      return "";
    }

    public void SetValue(string en, string val)
    {
      if (string.IsNullOrEmpty(val))
        return;
      if (setting.ContainsKey(en))
        setting[en] = val;
      else
        setting.Add(en, val);
    }

    public void PharseSettings(string param)
    {
      setting.Clear();
      string[] separat = {"|"};
      foreach (string s in param.Split(separat, StringSplitOptions.RemoveEmptyEntries))
      {
        SetValue(s.Split('=')[0], s.Split('=')[1]);
      }
    }

    public string GetConfigString()
    {
      string s = "";
      foreach (KeyValuePair<string, string> keyValuePair in setting)
      {
        s += keyValuePair.Key + "=" + keyValuePair.Value + "|";
      }
      return s;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if(!string.IsNullOrEmpty(GetValue("feed")))
      {
        string s = GetValue("feed");
        if(!string.IsNullOrEmpty(GetValue("time")))
        {
          s += " : "+GetValue("time");
        }
        if(!string.IsNullOrEmpty(GetValue("region")))
        {
          s += " ( " + GetValue("region") + " )";
        }
        txt_title.Text = s;
      }
    }

  }
}
