using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace YouTubePlugin.Class.SiteItems
{
  public partial class BillboardItemControl : UserControl
  {
    private bool loading = false;
    private SiteItemEntry _entry = new SiteItemEntry();
    public BillboardItemControl()
    {
      InitializeComponent();

    }

    private void txt_title_TextChanged(object sender, EventArgs e)
    {
      if (!loading)
      {
        _entry.SetValue("title", txt_title.Text);
        _entry.SetValue("feed", (string)cmb_standardfeed.SelectedItem);
        _entry.SetValue("feedint", cmb_standardfeed.SelectedIndex.ToString());
        _entry.Title = _entry.GetValue("title");
        _entry.ConfigString = _entry.GetConfigString();
      }
    }

    public void SetEntry(SiteItemEntry entry,string[] entries)
    {
      loading = true;
      _entry = entry;

      cmb_standardfeed.Items.Clear();

      cmb_standardfeed.Items.AddRange(entries);
      cmb_standardfeed.SelectedIndex = 0;

      _entry.PharseSettings(entry.ConfigString);
      txt_title.Text = _entry.GetValue("title");
      cmb_standardfeed.SelectedItem = _entry.GetValue("feed");
      chk_all.Checked = _entry.GetValue("all") == "true";
      loading = false;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      txt_title.Text = chk_all.Checked ? "Billboard" : cmb_standardfeed.Text;
    }

    private void chk_all_CheckedChanged(object sender, EventArgs e)
    {
      _entry.SetValue("all", chk_all.Checked ? "true" : "false");
      cmb_standardfeed.Enabled = !chk_all.Checked;
      _entry.ConfigString = _entry.GetConfigString();
    }
  }
}
