using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace YouTubePlayer
{
    public partial class FlashControl : UserControl
    {
        public FlashControl()
        {
            InitializeComponent();
        }
        public AxShockwaveFlashObjects.AxShockwaveFlash Player
        {
            get { return axShockwaveFlash1; }
        }
    }

}
