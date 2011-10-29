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
  public partial class DiscoControl : UserControl
  {
    private bool loading = false;
    private SiteItemEntry _entry = new SiteItemEntry();

    public DiscoControl()
    {
      InitializeComponent();
    }

    public void SetEntry(SiteItemEntry entry)
    {
      loading = true;
      _entry = entry;
      cmb_region.Items.Clear();
      cmb_region.Items.Add("");
      foreach (KeyValuePair<string, string> valuePair in Youtube2MP._settings.Regions)
      {
        cmb_region.Items.Add(valuePair.Key);
      }
      cmb_region.SelectedItem = _entry.GetValue("region");
      loading = false;
    }

    private void cmb_region_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!loading)
      {
        _entry.SetValue("region", (string)cmb_region.SelectedItem);
        //_entry.Title = _entry.GetValue("title");
        _entry.ConfigString = _entry.GetConfigString();
      }
    }
  }
}
