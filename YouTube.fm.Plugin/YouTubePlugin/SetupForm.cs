using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
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
      _settings.InitialSearch = textBox_startup.Text;
      _settings.InitialCat = comboBox_startup.SelectedIndex;
      _settings.MusicFilter = checkBox_filter.Checked;
      _settings.Time = checkBox_time.Checked;
      _settings.ShowNowPlaying = checkBox_nowplaying.Checked;
      _settings.UseYouTubePlayer = checkBox_useplayer.Checked;
      _settings.UseExtremFilter = checkBox_extremfilter.Checked;
      _settings.VideoQuality = comboBox_videoquality.SelectedIndex;
      _settings.UseSMSStyleKeyBoard = checkBox_sms.Checked;
      _settings.DownloadFolder = textBox_downloaddir.Text;
      _settings.FanartDir = textBox_fanartdir.Text;
      _settings.LoadOnlineFanart = checkBox1.Checked;
      _settings.Region = cmb_region.Text;
      _settings.OldStyleHome = chk_oldstyle.Checked;
      try
      {
      }
      catch (Exception)
      {
        _settings.InstantChar = 0;
      }

      foreach (string s in listBox_history.Items)
      {
        _settings.SearchHistory.Add(s);
      }
      if (radioButton1.Checked)
        _settings.InitialDisplay = 1;
      if (radioButton2.Checked)
        _settings.InitialDisplay = 2;
      if (radioButton3.Checked)
        _settings.InitialDisplay = 3;

      _settings.MainMenu.Items.Clear();
      foreach (ListViewItem item in list_startpage.Items)
      {
        _settings.MainMenu.Items.Add((SiteItemEntry)item.Tag);
      }
      _settings.Save();
      this.Close();
    }

    private ArrayList GenerateActionList()
    {
      ArrayList ret = new ArrayList();
      string[] names = Enum.GetNames(typeof(Action.ActionType));
      int[] values = (int[])Enum.GetValues(typeof(Action.ActionType));
      for (int i = 0; i < names.Length; i++)
      {
        ret.Add(new ActionEntry(names[i], values[i]));
      }
      return ret;
    }


    private void SetupForm_Load(object sender, EventArgs e)
    {
      loading = true;

      textBox_user.Text = _settings.User;
      textBox_passw.Text = _settings.Password;
      textBox_pluginname.Text = _settings.PluginName;
      listBox_history.Items.AddRange(_settings.SearchHistory.ToArray());
      checkBox_filter.Checked = _settings.MusicFilter;
      checkBox_time.Checked = _settings.Time;
      checkBox_nowplaying.Checked = _settings.ShowNowPlaying;
      checkBox_useplayer.Checked = _settings.UseYouTubePlayer;
      checkBox_extremfilter.Checked = _settings.UseExtremFilter;
      comboBox_videoquality.SelectedIndex = _settings.VideoQuality;
      checkBox_sms.Checked = _settings.UseSMSStyleKeyBoard;
      textBox_downloaddir.Text = _settings.DownloadFolder;
      textBox_fanartdir.Text = _settings.FanartDir;
      checkBox1.Checked = _settings.LoadOnlineFanart;
      chk_oldstyle.Checked = _settings.OldStyleHome;
      switch (_settings.InitialDisplay)
      {
        case 1:
          radioButton1.Checked = true;
          break;
        case 2:
          radioButton2.Checked = true;
          break;
        case 3:
          radioButton3.Checked = true;
          break;
        default:
          break;
      }
      foreach (KeyValuePair<string, string> valuePair in _settings.Regions)
      {
        cmb_region.Items.Add(valuePair.Key);
      }
      cmb_region.Items.Add("Ask");
      cmb_region.Text = _settings.Region;
      comboBox_startup.Items.AddRange(_settings.OldCats.ToArray());
      comboBox_startup.SelectedIndex = _settings.InitialCat;
      textBox_startup.Text = _settings.InitialSearch;

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

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton1.Checked)
      {
        radioButton2.Checked = false;
        comboBox_startup.Enabled = true;
        textBox_startup.Enabled = false;
        radioButton3.Checked = false;
      }
    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton2.Checked)
      {
        radioButton1.Checked = false;
        radioButton3.Checked = false;
        comboBox_startup.Enabled = false;
        textBox_startup.Enabled = true;

      }
    }

    private void radioButton3_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton3.Checked)
      {
        comboBox_startup.Enabled = false;
        textBox_startup.Enabled = false;

        radioButton1.Checked = false;
        radioButton2.Checked = false;
      }
    }

    private void tabPage3_Click(object sender, EventArgs e)
    {

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

    private void panel1_Paint(object sender, PaintEventArgs e)
    {

    }

    private void btn_tree_add_Click(object sender, EventArgs e)
    {

    }
  }

  class ActionEntry
  {
    private string actionName;
    private int actionID;

    public string ActionName
    {
      get
      {
        return actionName;
      }
    }

    public int ActionID
    {
      get
      {
        return actionID;
      }
    }

    public ActionEntry(string Name, int ID)
    {

      this.actionName = Name;
      this.actionID = ID;
    }
  }

}