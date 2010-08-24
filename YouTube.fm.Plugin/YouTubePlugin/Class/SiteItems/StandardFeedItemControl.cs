using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace YouTubePlugin.Class.SiteItems
{
  public partial class StandardFeedItemControl : UserControl
  {
    private bool loading = false;
    private SiteItemEntry _entry = new SiteItemEntry();

    public StandardFeedItemControl()
    {
      InitializeComponent();
    }

    public void SetEntry(SiteItemEntry entry)
    {
      loading = true;
      _entry = entry;
      cmb_time.Items.Clear();
      cmb_standardfeed.Items.Clear();
      cmb_region.Items.Clear();
      cmb_time.Items.Add("");
      foreach (string s in Youtube2MP._settings.TimeList)
      {
        cmb_time.Items.Add(s);
      }

      cmb_standardfeed.Items.AddRange(Youtube2MP._settings.Cats.ToArray());
      cmb_standardfeed.SelectedIndex = 0;

      cmb_region.Items.Add("");
      foreach (KeyValuePair<string, string> valuePair in Youtube2MP._settings.Regions)
      {
        cmb_region.Items.Add(valuePair.Key);
      }

      _entry.PharseSettings(entry.ConfigString);
      txt_title.Text = _entry.GetValue("title");
      cmb_region.SelectedItem = _entry.GetValue("region");
      cmb_time.SelectedItem = _entry.GetValue("time");
      cmb_standardfeed.SelectedItem = _entry.GetValue("feed");
      loading = false;
    }

    private void txt_title_TextChanged(object sender, EventArgs e)
    {
      if(!loading)
      {
        _entry.SetValue("title", txt_title.Text);
        _entry.SetValue("region", (string)cmb_region.SelectedItem);
        _entry.SetValue("time", (string)cmb_time.SelectedItem);
        _entry.SetValue("feed", (string)cmb_standardfeed.SelectedItem);
        _entry.SetValue("feedint", cmb_standardfeed.SelectedIndex.ToString());
        _entry.Title = _entry.GetValue("title");
        _entry.ConfigString = _entry.GetConfigString();
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(_entry.GetValue("feed")))
      {
        string s = _entry.GetValue("feed");
        if (!string.IsNullOrEmpty(_entry.GetValue("time")))
        {
          s += " : " + _entry.GetValue("time");
        }
        if (!string.IsNullOrEmpty(_entry.GetValue("region")))
        {
          s += " ( " + _entry.GetValue("region") + " )";
        }
        txt_title.Text = s;
      }
    }

  }
}
