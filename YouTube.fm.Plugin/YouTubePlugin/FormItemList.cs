using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using YouTubePlugin.Class;

namespace YouTubePlugin
{
  public partial class FormItemList : Form
  {
    public FormItemList(List<GenericListItem> listItems)
    {
      InitializeComponent();
      foreach (GenericListItem item in listItems)
      {
        ListViewItem listViewItem = new ListViewItem(item.Title);
        listViewItem.Tag = item;
        listView1.Items.Add(listViewItem);
      }
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        GenericListItem item = listView1.SelectedItems[0].Tag as GenericListItem;
        pictureBox1.ImageLocation = item.LogoUrl;
      }
    }
  }
}
