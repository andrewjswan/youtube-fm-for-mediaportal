using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace TestClient
{
  public partial class Client : Form
  {
    public Settings Settings { get; set; }

    public Client()
    {
      InitializeComponent();
      Settings = Settings.Load();
      checkBox1.Checked = Settings.TopMost;
      comboBox1.SelectedIndex = 0;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      TcpClient socketForServer;
      try
      {
        socketForServer = new TcpClient(Settings.Host, Settings.Port);
      }
      catch(Exception ex)
      {
        label1.ForeColor = Color.Red;
        label1.Text = string.Format("Failed to connect to server at {0}:{1}", Settings.Host, Settings.Port);
        return;
      }
      NetworkStream networkStream = socketForServer.GetStream();
      System.IO.StreamReader streamReader =
        new System.IO.StreamReader(networkStream);
      System.IO.StreamWriter streamWriter =
        new System.IO.StreamWriter(networkStream);
      try
      {
        string outputString;
        // read the data from the host and display it
        {
          string mes = "";
          switch (comboBox1.SelectedIndex)
          {
            case 0:
              mes = "PLAY:";
              break;
            case 1:
              mes = "SEARCH:";
              break;
            case 2:
              mes = "ARTISTVIDEOS:";
              break;
          }
          outputString = streamReader.ReadLine();
          streamWriter.WriteLine(mes + textBox1.Text);
          label1.ForeColor = Color.Black;
          label1.Text = string.Format("Message sent !");
          streamWriter.Flush();
        }
      }
      catch
      {
        label1.ForeColor = Color.Red;
        label1.Text = string.Format("Exception reading from Server");
      }
      // tidy up
      networkStream.Close();
      socketForServer.Close();
    }

    private void Client_Load(object sender, EventArgs e)
    {

    }

    private void btn_config_Click(object sender, EventArgs e)
    {
      Hide();
      SettingsForm dlg = new SettingsForm();
      dlg.Settings = Settings;
      dlg.ShowDialog();
      Show();
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      TopMost = checkBox1.Checked;
      Settings.TopMost = checkBox1.Checked;
      Settings.Save();
    }

    private void Client_DragEnter(object sender, DragEventArgs e)
    {
      string[] t = e.Data.GetFormats();
      string s = e.Data.GetData(DataFormats.Text).ToString();
      if (string.IsNullOrEmpty(s))
      {
        s = e.Data.GetData("UniformResourceLocator").ToString();
      }
      if(!string.IsNullOrEmpty(s))
      {
        e.Effect = DragDropEffects.All;
      }
      else
      {
        e.Effect = DragDropEffects.None;
      }
    }

    static public string getIDSimple(string googleID)
    {
      if (string.IsNullOrEmpty(googleID))
        return null;
      string id = "";
      if (googleID.Contains("video_id"))
      {
        Uri erl = new Uri(googleID);
        string[] param = erl.Query.Substring(1).Split('&');
        foreach (string s in param)
        {
          if (s.Split('=')[0] == "video_id")
          {
            id = s.Split('=')[1];
          }
        }
      }
      else if (googleID.Contains("video:"))
      {
        int lastVideo = googleID.LastIndexOf("video:");
        if (googleID.IndexOf(":", lastVideo + 6) != -1)
          id = googleID.Substring(lastVideo + 6, googleID.IndexOf(":", lastVideo + 6) - (lastVideo + 6));
        else
          id = googleID.Substring(lastVideo + 6);
      }
      else if (googleID.Contains("v="))
      {
        Uri erl = new Uri(googleID);
        string[] param = erl.Query.Substring(1).Split('&');
        foreach (string s in param)
        {
          if (s.Split('=')[0] == "v")
          {
            id = s.Split('=')[1];
          }
        }
      }
      return id;
    }

    private void Client_DragDrop(object sender, DragEventArgs e)
    {
      string s = e.Data.GetData(DataFormats.Text).ToString();
      if (string.IsNullOrEmpty(s))
      {
        s = e.Data.GetData("UniformResourceLocator").ToString();
      }
      if (!string.IsNullOrEmpty(s))
      {
        e.Effect = DragDropEffects.Move;
        try
        {
          string id = getIDSimple(s);
          if (!string.IsNullOrEmpty(id))
          {
            textBox1.Text = id;
            comboBox1.SelectedIndex = 0;
            button1.PerformClick();
            return;
          }
        }
        catch (Exception)
        {

        }
        comboBox1.SelectedIndex = 1;
        textBox1.Text = s;
      }
      else
      {
        e.Effect = DragDropEffects.None;
      }

    }
  }

}

