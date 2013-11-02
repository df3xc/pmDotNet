using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using enerGenieDotNet;

namespace PMDLLTestCSharp
{
    public partial class Form1 : Form
    {
        dotNet powerSocket = new dotNet();

        deviceClass device = new deviceClass();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGetDevices_Click(object sender, EventArgs e)
        {
            powerSocket.GetDevList();  // create a list of all devices

            LogBox.AppendText("Found " + powerSocket.deviceList.Count.ToString() + " devices \n");

            if (powerSocket.deviceList.Count >0 ) device = powerSocket.deviceList[0];
        }

        private void btnSetSocket_Click(object sender, EventArgs e)
        {
            try
            {
                powerSocket.toggleSwitchState(device,(int)tBsocket.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSocketOn_Click(object sender, EventArgs e)
        {
            powerSocket.setSwitchState(device, (int)tBsocket.Value, true);
        }

        private void btnSocketOff_Click(object sender, EventArgs e)
        {
            powerSocket.setSwitchState(device, (int)tBsocket.Value, false);
        }

    }
}
