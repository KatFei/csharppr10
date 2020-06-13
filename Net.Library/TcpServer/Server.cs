using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SomeProject.Library.Server
{
    public class Server
    {
        TcpListener serverListener;

        public Server()
        {
            serverListener = new TcpListener(IPAddress.Loopback, 8080);
        }

        public bool TurnOffListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Stop();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn off listener: " + e.Message);
                return false;
            }
        }

        public async Task TurnOnListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Start();
                while (true)
                {
                    //выбирать между получением сообщения и получением файла
                    //считывание типа из OperationResult?
                    OperationResult result = await ReceiveMessageFromClient();
                    if (result.Result == Result.Fail)
                        Console.WriteLine("Unexpected error: " + result.Message);
                    else
                        Console.WriteLine("New message from client: " + result.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }

        public async Task<OperationResult> ReceiveMessageFromClient()
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                StringBuilder recievedMessage = new StringBuilder();
                TcpClient client = serverListener.AcceptTcpClient();

                byte[] data = new byte[256];
                NetworkStream stream = client.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                client.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }


        public async Task<OperationResult> ReceiveFileFromClient()
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                //Deserialize file
                byte[] recievedFile = new byte[0];
                TcpClient client = serverListener.AcceptTcpClient();
                //как получить  кол-во байт в получаемом файле
                byte[] data = new byte[256];
                NetworkStream stream = client.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    //recievedFile.(data, bytes);
                }
                while (stream.DataAvailable);
                stream.Close();
                client.Close();
                // создаем файл методом
                ///File.WriteAllBytes
                //отправлять имя файла
                return new OperationResult(Result.OK, "recievedFile.ToString()"); 
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        public OperationResult SendMessageToClient(string message)
        {
            try
            {
                TcpClient client = serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
            return new OperationResult(Result.OK, "");
        }
    }
}