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
  public partial class LastFmUserControl : UserControl
  {
    private bool loading = false;
    private SiteItemEntry _entry = new SiteItemEntry();

    public LastFmUserControl()
    {
      InitializeComponent();
    }

    public void SetEntry(SiteItemEntry entry)
    {
      loading = true;
      _entry = entry;

      txt_title.Text = _entry.GetValue("title");
      cmb_type.SelectedItem = entry.GetValue("type");
      loading = false;
    }

    private void txt_title_TextChanged(object sender, EventArgs e)
    {
      if (!loading)
      {
        _entry.SetValue("title", txt_title.Text);
        _entry.SetValue("type", (string)cmb_type.SelectedItem);
        _entry.Title = _entry.GetValue("title");
        _entry.ConfigString = _entry.GetConfigString();
      }
    }
  }
}
