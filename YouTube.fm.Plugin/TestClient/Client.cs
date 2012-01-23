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
    public Client()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      TcpClient socketForServer;
      try
      {
        socketForServer = new TcpClient("localHost", 18944);
      }
      catch
      {
        listBox1.Items.Add(string.Format("Failed to connect to server at {0}:999", "localhost"));
        Console.WriteLine(
          "Failed to connect to server at {0}:999", "localhost");
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
          outputString = streamReader.ReadLine();
          listBox1.Items.Add(outputString);
          streamWriter.WriteLine(textBox1.Text);
          listBox1.Items.Add("Client Message");
          streamWriter.Flush();
        }
      }
      catch
      {
        listBox1.Items.Add("Exception reading from Server");
      }
      // tidy up
      networkStream.Close();
      socketForServer.Close();
    }
  }
}

