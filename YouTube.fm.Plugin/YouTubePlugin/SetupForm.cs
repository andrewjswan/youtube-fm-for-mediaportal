using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using ConsoleApplication2.com.amazon.webservices;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;

using MediaPortal.GUI.Library;
using YouTubePlugin.Class;
using YouTubePlugin.Class.Artist;
using YouTubePlugin.Class.SiteItems;
using Action = MediaPortal.GUI.Library.Action;


namespace YouTubePlugin
{
  public partial class SetupForm : Form
  {
    public Settings _settings;
    private bool loading = true;
    private SiteItemEntry selectedSiteitem;

    public SetupForm()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      _settings.User = textBox_user.Text;
      _settings.Password = textBox_passw.Text;
      _settings.PluginName = textBox_pluginname.Text;
      _settings.SearchHistory.Clear();
      _settings.StartUpOpt = comboBox_startup.SelectedIndex;
      _settings.LayoutItem = cmb_view_item.SelectedIndex;
      _settings.LayoutArtist = cmb_view_artist.SelectedIndex;
      _settings.LayoutVideo = cmb_view_video.SelectedIndex;
      _settings.MusicFilter = checkBox_filter.Checked;
      _settings.ShowNowPlaying = checkBox_nowplaying.Checked;
      _settings.UseExtremFilter = checkBox_extremfilter.Checked;
      _settings.VideoQuality = comboBox_videoquality.SelectedIndex;
      _settings.DownloadFolder = textBox_downloaddir.Text;
      _settings.FanartDir = textBox_fanartdir.Text;
      _settings.LoadOnlineFanart = checkBox1.Checked;
      _settings.CacheDir = txt_cachedir.Text;
      _settings.LastFmUser = txt_lastfm_user.Text;
      _settings.LastFmPass = txt_lastfm_pass.Text;
      _settings.LastFmNowPlay = chk_lastfm_nowplay.Checked;
      _settings.LastFmSubmit = chk_lastfm_submit.Checked;


      foreach (string s in listBox_history.Items)
      {
        _settings.SearchHistory.Add(s);
      }

      _settings.MainMenu.Items.Clear();
      foreach (ListViewItem item in list_startpage.Items)
      {
        _settings.MainMenu.Items.Add((SiteItemEntry)item.Tag);
      }
      _settings.Save();
      this.Close();
    }



    private void SetupForm_Load(object sender, EventArgs e)
    {
      loading = true;

      textBox_user.Text = _settings.User;
      textBox_passw.Text = _settings.Password;
      textBox_pluginname.Text = _settings.PluginName;
      listBox_history.Items.AddRange(_settings.SearchHistory.ToArray());
      checkBox_filter.Checked = _settings.MusicFilter;
      checkBox_nowplaying.Checked = _settings.ShowNowPlaying;
      checkBox_extremfilter.Checked = _settings.UseExtremFilter;
      comboBox_videoquality.SelectedIndex = _settings.VideoQuality;
      cmb_view_item.SelectedIndex = _settings.LayoutItem;
      cmb_view_artist.SelectedIndex = _settings.LayoutArtist;
      cmb_view_video.SelectedIndex = _settings.LayoutVideo;
      textBox_downloaddir.Text = _settings.DownloadFolder;
      textBox_fanartdir.Text = _settings.FanartDir;
      checkBox1.Checked = _settings.LoadOnlineFanart;
      txt_cachedir.Text = _settings.CacheDir;
      txt_lastfm_user.Text = _settings.LastFmUser;
      txt_lastfm_pass.Text = _settings.LastFmPass;
      chk_lastfm_nowplay.Checked = _settings.LastFmNowPlay;
      chk_lastfm_submit.Checked = _settings.LastFmSubmit;

      //foreach (KeyValuePair<string, string> valuePair in _settings.Regions)
      //{
      //  cmb_region.Items.Add(valuePair.Key);
      //}
      //cmb_region.Items.Add("Ask");
      //cmb_region.Text = _settings.Region;
      //comboBox_startup.Items.AddRange(_settings.OldCats.ToArray());
      comboBox_startup.SelectedIndex = _settings.StartUpOpt;

      foreach (KeyValuePair<string, ISiteItem> siteItem in Youtube2MP.SiteItemProvider)
      {
        cmb_providers.Items.Add(siteItem.Value.Name);
      }
      cmb_providers.SelectedIndex = 0;
      list_startpage.Items.Clear();

      foreach (SiteItemEntry entry in _settings.MainMenu.Items)
      {
        ListViewItem listViewItem = new ListViewItem(entry.Title);
        listViewItem.Tag = entry;
        if (entry.Provider == "Folder")
          listViewItem.BackColor = Color.Cyan;
        listViewItem.Selected = true;
        list_startpage.Items.Add(listViewItem);
      }
      loading = false;

      list_startpage.SelectedIndices.Add(0);

      lst_artists.Items.Clear();
      List<ArtistItem> artistItems = ArtistManager.Instance.GetArtists("");
      foreach (ArtistItem artistItem in artistItems)
      {
        lst_artists.Items.Add(artistItem);
      }
      label5.Text = "Total artist count :" + lst_artists.Items.Count.ToString();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void button3_Click(object sender, EventArgs e)
    {
      listBox_history.Items.Clear();
    }

    private void button4_Click(object sender, EventArgs e)
    {
      listBox_history.Items.Add(textBox1.Text);
      textBox1.Text = "";
    }

    private void button5_Click(object sender, EventArgs e)
    {
      List<string> sel = new List<string>();
      foreach (string s in listBox_history.SelectedItems)
      {
        sel.Add(s);
      }
      foreach (string s in sel)
      {
        listBox_history.Items.Remove(s);
      }

    }

 

    private void checkBox_time_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void button_getdir_Click(object sender, EventArgs e)
    {
      folderBrowserDialog1.SelectedPath = textBox_downloaddir.Text;
      if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
      {
        textBox_downloaddir.Text = folderBrowserDialog1.SelectedPath;
      }
    }

    private void button6_Click(object sender, EventArgs e)
    {
      folderBrowserDialog1.SelectedPath = textBox_downloaddir.Text;
      if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
      {
        textBox_fanartdir.Text = folderBrowserDialog1.SelectedPath;
      }
    }

    private void btn_add_provider_Click(object sender, EventArgs e)
    {
      ISiteItem siteItem = Youtube2MP.SiteItemProvider[cmb_providers.SelectedItem.ToString()];
      ListViewItem listViewItem = new ListViewItem(siteItem.Name);
      SiteItemEntry entry = new SiteItemEntry() { Provider = siteItem.Name, Title = siteItem.Name };
      if (siteItem.Name == "Folder")
        listViewItem.BackColor = Color.Cyan;
      listViewItem.Tag = entry;
      listViewItem.Selected = true;
      list_startpage.Items.Add(listViewItem);
    }

    private void list_startpage_SelectedIndexChanged(object sender, EventArgs e)
    {
      panel1.Controls.Clear();
      if (selectedSiteitem != null)
      {
        selectedSiteitem.ParentFolder = (string)cmb_folder.SelectedItem;
        selectedSiteitem.ConfigString = selectedSiteitem.GetConfigString();
      }

      cmb_folder.Items.Clear();
      cmb_folder.Items.Add("");

      foreach (ListViewItem item in list_startpage.Items)
      {
        SiteItemEntry siteItemEntry = item.Tag as SiteItemEntry;
        if (siteItemEntry != null && Youtube2MP.SiteItemProvider[siteItemEntry.Provider].GetType() == typeof (Folder))
        {
          cmb_folder.Items.Add(siteItemEntry.Title);
        }
      }
      if (list_startpage.SelectedItems.Count > 0 && !loading)
      {
        SiteItemEntry entry = list_startpage.SelectedItems[0].Tag as SiteItemEntry;
        selectedSiteitem = entry;
        cmb_folder.SelectedItem = selectedSiteitem.ParentFolder;
        ISiteItem siteItem = Youtube2MP.SiteItemProvider[entry.Provider];
        siteItem.Configure(entry);
        if (siteItem.ConfigControl != null)
          panel1.Controls.Add(siteItem.ConfigControl);
      }
      foreach (ListViewItem listViewItemitem in list_startpage.Items)
      {
        listViewItemitem.Text = ((SiteItemEntry)listViewItemitem.Tag).Title;
      }
      BuildTree();
    }

    private void BuildTree()
    {
      treeV.Nodes.Clear();
      foreach (ListViewItem item in list_startpage.Items)
      {
        SiteItemEntry siteItemEntry = item.Tag as SiteItemEntry;
        if (string.IsNullOrEmpty(siteItemEntry.ParentFolder))
        {
          if (siteItemEntry.Provider == "Folder")
            AddSubtree(treeV.Nodes.Add(siteItemEntry.Title), siteItemEntry.Title);
          else
            treeV.Nodes.Add(siteItemEntry.Title);
        }
      }
      treeV.ExpandAll();
    }


    private void AddSubtree(TreeNode node, string parent)
    {
      if (string.IsNullOrEmpty(parent))
        return;
      foreach (ListViewItem item in list_startpage.Items)
      {
        SiteItemEntry siteItemEntry = item.Tag as SiteItemEntry;
        if (!string.IsNullOrEmpty(siteItemEntry.ParentFolder) && siteItemEntry.ParentFolder == parent)
        {
          if (siteItemEntry.Title != parent)
            AddSubtree(node.Nodes.Add(siteItemEntry.Title), siteItemEntry.Title);
        }
      }
    }



    private void btn_del_provider_Click(object sender, EventArgs e)
    {
      if (list_startpage.SelectedItems.Count > 0)
      {
        list_startpage.Items.Remove(list_startpage.SelectedItems[0]);
      }
    }

    private void btn_up_Click(object sender, EventArgs e)
    {
      if (list_startpage.SelectedItems.Count < 1)
        return;
      ListViewItem listViewItem = list_startpage.SelectedItems[0];
      int idx = list_startpage.Items.IndexOf(listViewItem);
      if (idx < 1)
        return;
      list_startpage.Items.Remove(listViewItem);
      list_startpage.Items.Insert(idx - 1, listViewItem);
    }

    private void btn_down_Click(object sender, EventArgs e)
    {
      if (list_startpage.SelectedItems.Count < 1)
        return;
      ListViewItem listViewItem = list_startpage.SelectedItems[0];
      int idx = list_startpage.Items.IndexOf(listViewItem);
      if (idx > list_startpage.Items.Count - 2)
        return;
      list_startpage.Items.Remove(listViewItem);
      list_startpage.Items.Insert(idx + 1, listViewItem);
    }

    private void button7_Click(object sender, EventArgs e)
    {
      if (list_startpage.SelectedItems.Count > 0)
      {
        SiteItemEntry entry = list_startpage.SelectedItems[0].Tag as SiteItemEntry;
        FormItemList dlg = new FormItemList(Youtube2MP.GetList(entry));
        dlg.ShowDialog();
      }
    }

    private void button8_Click(object sender, EventArgs e)
    {
      if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        List<ArtistItem> artistItems = ArtistManager.Instance.GetArtists("");

        var serializer = new XmlSerializer(typeof(List<ArtistItem>));
        using (TextWriter writer = new StreamWriter(saveFileDialog1.FileName))
        {
          serializer.Serialize(writer, artistItems);
          writer.Close();
        }
      }
    }

    private void lst_artists_SelectedIndexChanged(object sender, EventArgs e)
    {
      if(lst_artists.SelectedItem!=null)
      {
        ArtistItem item = lst_artists.SelectedItem as ArtistItem;
        if(!string.IsNullOrEmpty(item.Img_url))
        {
          pictureBox1.ImageLocation = item.Img_url;
          pictureBox1.Visible = true;
        }
        else
        {
          pictureBox1.Visible = false;
        }
      }
    }

    private void button9_Click(object sender, EventArgs e)
    {
      if(openFileDialog1.ShowDialog()==System.Windows.Forms.DialogResult.OK && File.Exists(openFileDialog1.FileName))
      {
        
        var serializer = new XmlSerializer(typeof(List<ArtistItem>));
        var fs = new FileStream(openFileDialog1.FileName, FileMode.Open);
        List<ArtistItem> artistItems = (List<ArtistItem>)serializer.Deserialize(fs);
        fs.Close();
        foreach (ArtistItem artistItem in artistItems)
        {
          ArtistManager.Instance.AddArtist(artistItem);
        }

        lst_artists.Items.Clear();
        artistItems = ArtistManager.Instance.GetArtists("");
        foreach (ArtistItem artistItem in artistItems)
        {
          lst_artists.Items.Add(artistItem);
        }
        label5.Text = "Total artist count :" + lst_artists.Items.Count.ToString();
      }
    }


    private void btn_lastfm_test_Click(object sender, EventArgs e)
    {
      try
      {
        LastProfile profile = new LastProfile();
        if (profile.Login(txt_lastfm_user.Text, txt_lastfm_pass.Text))
          MessageBox.Show("Login OK!");
        else
          MessageBox.Show("Invalid login data or no connection !");
      }
      catch (Exception exception)
      {
         MessageBox.Show(exception.Message);
      }
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start("http://code.google.com/p/youtube-fm-for-mediaportal/");
      }
      catch (Exception)
      {
      }
    }

    private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start("http://forum.team-mediaportal.com/mediaportal-plugins-47/youtube-fm-youtube-music-videos-updated-04-10-2011-a-49148/");
      }
      catch (Exception)
      {
      }
    }

    private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start("http://www.youtube.com/");
      }
      catch (Exception)
      {
      }
    }

    private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start("http://www.last.fm/home");
      }
      catch (Exception)
      {
      }
    }

    private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start("http://www.billboard.com");
      }
      catch (Exception)
      {
      }
    }

    private void button10_Click(object sender, EventArgs e)
    {
      folderBrowserDialog1.SelectedPath = txt_cachedir.Text;
      if(folderBrowserDialog1.ShowDialog()==DialogResult.OK)
      {
        txt_cachedir.Text = folderBrowserDialog1.SelectedPath;
      }
    }

    private void btn_empty_cache_Click(object sender, EventArgs e)
    {
      string[] files = Directory.GetFiles(txt_cachedir.Text, "youtubevideos-*.jpg");
      foreach (string file in files)
      {
        try
        {
          File.Delete(file);
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
      }
      MessageBox.Show("Operation done !");
    }

    private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start("http://www.htbackdrops.com");
      }
      catch (Exception)
      {
      }
    }




  }

}