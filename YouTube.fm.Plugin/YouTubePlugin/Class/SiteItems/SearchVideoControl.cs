using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace YouTubePlugin.Class.SiteItems
{
  public partial class SearchVideoControl : UserControl
  {
    private bool loading = false;
    private SiteItemEntry _entry = new SiteItemEntry();

    public SearchVideoControl()
    {
      InitializeComponent();
      cmb_sort.SelectedIndex = 0;
    }

    public void SetEntry(SiteItemEntry entry)
    {
      loading = true;
      _entry = entry;
      cmb_time.Items.Clear();
      cmb_time.Items.Add("");
      foreach (string s in Youtube2MP._settings.TimeList)
      {
        cmb_time.Items.Add(s);
      }

      txt_title.Text = _entry.GetValue("title");
      txt_term.Text = _entry.GetValue("term");
      cmb_time.SelectedItem = _entry.GetValue("time");

      int i = 0;
      int.TryParse(_entry.GetValue("sortint"), out i);
      cmb_sort.SelectedIndex = i;
      loading = false;
    }

    private void txt_title_TextChanged(object sender, EventArgs e)
    {
      if (!loading)
      {
        _entry.SetValue("title", txt_title.Text);
        _entry.SetValue("term", txt_term.Text);
        _entry.SetValue("sortint", cmb_sort.SelectedIndex.ToString());
        _entry.SetValue("time", (string)cmb_time.SelectedItem);

        _entry.Title = _entry.GetValue("title");
        _entry.ConfigString = _entry.GetConfigString();
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      txt_title.Text = "Search for :" + txt_term.Text;
    }
  }
}
