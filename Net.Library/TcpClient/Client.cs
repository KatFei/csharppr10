using System;
using System.Text;
using System.Net.Sockets;

namespace SomeProject.Library.Client
{
    public class Client
    {
        public TcpClient tcpClient;

        public OperationResult ReceiveMessageFromServer()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                NetworkStream stream = tcpClient.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                tcpClient.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }

        public OperationResult SendingManager(SendingType type)
        {
            try
            {
                //if (!tcpClient.Connected)  
                //tcpClient = new TcpClient("127.0.0.1", 8080);
                
                NetworkStream stream = tcpClient.GetStream();
                byte[] data = null; 
                if(type == SendingType.Msg)
                    data = System.Text.Encoding.UTF8.GetBytes("msg");
                else
                    if (type == SendingType.File)
                        data = System.Text.Encoding.UTF8.GetBytes("file");
                stream.Write(data, 0, data.Length);
                stream.Close();
                
                return new OperationResult(Result.OK, "");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
        public OperationResult SendMessageToServer(string message)
        {
            try
            {
                //проверка в SendingManager (или здесь) что соединение уже есть
                tcpClient = new TcpClient("127.0.0.1", 8080);
                SendingManager(SendingType.Msg);
                //if (SendingManager(SendingType.Msg).Result == Result.OK)
                //{
                    NetworkStream stream = tcpClient.GetStream();
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                    tcpClient.Close();
                    return new OperationResult(Result.OK, "");
                //}
                //else
                //    return new OperationResult(Result.Fail, e.Message);
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
        public OperationResult SendFileToServer(string path)
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                //сообщаем что дальше будет файл
                SendingManager(SendingType.File);
                //посылаем расширение файла
                NetworkStream stream = tcpClient.GetStream();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(path);
                stream.Write(data, 0, data.Length);
                stream.Close();
                //посылаем длину файла
                //deserialize file BinaryFormatter

                //посылаем файл

                tcpClient.Close();
                return new OperationResult(Result.OK, "");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
    }
}
