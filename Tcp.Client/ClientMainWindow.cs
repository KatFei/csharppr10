using System;
using System.Windows.Forms;
using SomeProject.Library.Client;
using SomeProject.Library;

namespace SomeProject.TcpClient
{
    public partial class ClientMainWindow : Form
    {
        public ClientMainWindow()
        {
            InitializeComponent();
        }

        private void OnMsgBtnClick(object sender, EventArgs e)
        {
            Client client = new Client();
            Result res = client.SendMessageToServer(textBox.Text).Result;
            if(res == Result.OK)
            {
                textBox.Text = "";
                labelRes.Text = "Message was sent succefully!";
            }
            else
            {
                labelRes.Text = "Cannot send the message to the server.";
            }
            timer.Interval = 2000;
            timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            labelRes.Text = "";
            timer.Stop();
        }

        private void butSendFile_Click(object sender, EventArgs e)
        {
            if ((lblPath.Text != "") && (lblPath.Text != "No file choosen"))
            {
                Client client = new Client();
                Result res = client.SendFileToServer(lblPath.Text).Result;
                if (res == Result.OK)
                {
                    lblPath.ResetText();
                    labelRes.Text = "File was sent succefully!";
                }
                else
                {
                    labelRes.Text = "Cannot send the file to the server.";
                }
                timer.Interval = 2000;
                timer.Start();
            }
        }

        private void butBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string path = openFileDialog1.FileName;
            lblPath.Text = path;
        }
    }
}
