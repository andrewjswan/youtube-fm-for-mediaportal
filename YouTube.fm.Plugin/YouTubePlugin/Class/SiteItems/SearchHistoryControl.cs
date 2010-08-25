using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace YouTubePlugin.Class.SiteItems
{
  public partial class SearchHistoryControl : UserControl
  {
    private bool loading = false;
    private SiteItemEntry _entry = new SiteItemEntry();

    public SearchHistoryControl()
    {
      InitializeComponent();
    }

    public void SetEntry(SiteItemEntry entry)
    {
      loading = true;
      _entry = entry;
      int i = 0;
      int.TryParse(_entry.GetValue("items"), out i);
      numericUpDown1.Value = i;
      if (numericUpDown1.Value < 2)
        numericUpDown1.Value = 2;
      if (_entry.GetValue("flat") == "true")
        checkBox1.Checked = true;
      else
        checkBox1.Checked = false; 

      txt_title.Text = _entry.GetValue("title");
      loading = false;
      if (string.IsNullOrEmpty(txt_title.Text))
        txt_title.Text = "Search History";
    }

    private void txt_title_TextChanged(object sender, EventArgs e)
    {
      if (!loading)
      {
        _entry.SetValue("title", txt_title.Text);
        _entry.SetValue("items", numericUpDown1.Value.ToString());
        _entry.SetValue("flat", checkBox1.Checked ? "true" : "false");

        _entry.Title = _entry.GetValue("title");
        _entry.ConfigString = _entry.GetConfigString();
      }
    }

  }
}
