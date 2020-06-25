using System;
using System.Windows.Forms;
using SomeProject.Library.Client;
using SomeProject.Library;

namespace SomeProject.TcpClient
{
    public partial class ClientMainWindow : Form
    {
        /// <summary> Клиент TCP </summary>
        Client client;
        public ClientMainWindow()
        {
            InitializeComponent();
            //lblPath.UseCompatibleTextRendering();
            client = new Client();
            //создать client.Connect()
            client.DataFromServerRecieved += new EventHandler<string>(this.OnDataFromServerRecieved);
        }

        private void OnMsgBtnClick(object sender, EventArgs e)
        {
            //client = new Client();
            //client.ListenForData();
            Result res = client.SendMessageToServer(textBox.Text).Result.Result;
            if(res == Result.OK)
            {
                //textBox.Text = "";
                labelRes.Text = "Message was sent succefully!";
            }
            else
            {
                labelRes.Text = "Cannot send the message to the server.";
            }
            timer.Interval = 2000;
            timer.Start();
        }

        private void OnDataFromServerRecieved(object sender, string msg)
        {
            //client = new Client(); // перенести в конструктор ClientMainWindow
            
            if (msg != "")
            {
                textBox.Text += Environment.NewLine + msg;
                labelRes.Text = "Message from server recieved!";
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
            if ((lblPath.Text != "") && (lblPath.Text != "No file chosen"))
            {
                //client = new Client();
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
            dlgOpenFile.ShowDialog();
            //проверять что файл выбран
            if (dlgOpenFile.FileName != "") { 
            string path = dlgOpenFile.FileName;
            lblPath.Text = path;
            }
        }
    }
}
