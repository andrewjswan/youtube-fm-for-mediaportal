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
  public partial class VideoItemControl : UserControl
  {
    private bool loading = false;
    private SiteItemEntry _entry = new SiteItemEntry();

    public VideoItemControl()
    {
      InitializeComponent();
    }

    public void SetEntry(SiteItemEntry entry)
    {
      loading = true;
      _entry = entry;

      txt_title.Text = _entry.GetValue("title");
      txt_videoId.Text = entry.GetValue("VideoId");
      loading = false;
    }

    private void txt_title_TextChanged(object sender, EventArgs e)
    {
      if (!loading)
      {
        _entry.SetValue("title", txt_title.Text);
        _entry.SetValue("VideoId", txt_videoId.Text);
        _entry.Title = _entry.GetValue("title");
        _entry.ConfigString = _entry.GetConfigString();
      }
    }



  }
}
