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
  public partial class LastFmTopTracksControl : UserControl
  {
    private bool loading = false;
    private SiteItemEntry _entry = new SiteItemEntry();

    public LastFmTopTracksControl()
    {
      InitializeComponent();
    }

    public void SetEntry(SiteItemEntry entry)
    {
      loading = true;
      _entry = entry;

      txt_title.Text = _entry.GetValue("title");
      cmb_country.SelectedItem = entry.GetValue("country");
      loading = false;
    }

    private void txt_title_TextChanged(object sender, EventArgs e)
    {
      if (!loading)
      {
        _entry.SetValue("title", txt_title.Text);
        _entry.SetValue("country", (string) cmb_country.SelectedItem);
        _entry.Title = _entry.GetValue("title");
        _entry.ConfigString = _entry.GetConfigString();
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      txt_title.Text = "Top Tracks :" + cmb_country.Text;
    }



  }
}
