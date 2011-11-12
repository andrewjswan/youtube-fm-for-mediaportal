using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubePlugin.Class
{
  public class SiteItemEntry
  {
    Dictionary<string, string> setting = new Dictionary<string, string>();

    public SiteItemEntry()
    {
      ConfigString = "";
      Provider = "";
      Title = "";
      StartItem = 1;
    }

    public SiteItemEntry Copy()
    {
      SiteItemEntry newEntry=new SiteItemEntry();
      foreach (KeyValuePair<string, string> keyValuePair in setting)
      {
        newEntry.SetValue(keyValuePair.Key, keyValuePair.Value);
      }
      newEntry.Parent = this.Parent;
      newEntry.ParentFolder = this.ParentFolder;
      newEntry.Provider = this.Provider;
      newEntry.StartItem = this.StartItem;
      newEntry.Title = this.Title;
      newEntry.Url = this.Url;
      return newEntry;
    }

    public int StartItem { get; set; }
    public string Url { get; set; }
    public string Provider { get; set; }
    public string Title { get; set; }
    public string ConfigString { get; set; }
    public string ParentFolder
    {
      get { return GetValue("ParentFolder"); }
      set { SetValue("ParentFolder", value); }
    }
    public SiteItemEnumerator Parent { get; set; }


    public string GetValue(string en)
    {
      if (setting.Count < 1)
        PharseSettings(ConfigString);
      if (setting.ContainsKey(en))
        return setting[en];
      return "";
    }

    public void SetValue(string en, string val)
    {
      if (val == null)
        return;
      if (setting.ContainsKey(en))
        setting[en] = val;
      else
        setting.Add(en, val);
    }

    public void PharseSettings(string param)
    {
      setting.Clear();
      string[] separat = { "|" };
      foreach (string s in param.Split(separat, StringSplitOptions.RemoveEmptyEntries))
      {
        SetValue(s.Split('=')[0], s.Split('=')[1]);
      }
    }

    public string GetConfigString()
    {
      string s = "";
      foreach (KeyValuePair<string, string> keyValuePair in setting)
      {
        s += keyValuePair.Key + "=" + keyValuePair.Value + "|";
      }
      return s;
    }
  }
}
