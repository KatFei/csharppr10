using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace SomeProject.Library.Client
{
    public class Client
    {
        public TcpClient tcpClient;
        TcpListener clientListener;
        //событие получения сообщения от сервера
        
        public event EventHandler<string> DataFromServerRecieved;
        public event EventHandler<string> RecievingDataFailed;
        // подписать событие на обработчик из кода реализации ClientMainWindow

        public Client()
        {
            //clientListener = new TcpListener("127.0.0.1", 8080);
            //tcpClient = new TcpClient("127.0.0.1", 8080);
            //tcpClient.Client.Listen();
            //ListenForData();
            //tcpClient keep alive
        }
        public async Task ListenForData()
        {
            try
            {
                if (clientListener != null)
                    clientListener.Start();

                while (true)
                {
                    //выбирать между получением сообщения и получением файла
                    //считывание типа из OperationResult?

                    OperationResult result =  await ReceiveMessageFromServer();
                    if (result.Result == Result.OK)
                        // raise event "получено сообщение"
                        DataFromServerRecieved(this, "result.Message");
                    //else
                    //    //какую реакцию сделать в случае ошибки? -запись в lbl?
                    //    RecievingDataFailed(this, result.Message);


                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }
        public async Task<OperationResult> ReceiveMessageFromServer()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                //ListenForData();
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
                tcpClient = new TcpClient("127.0.0.1", 8080);
                
                NetworkStream stream = tcpClient.GetStream();
                byte[] data = null; 
                if(type == SendingType.Msg)
                    data = System.Text.Encoding.UTF8.GetBytes("msg");
                else
                    if (type == SendingType.File)
                        data = System.Text.Encoding.UTF8.GetBytes("file");
                stream.Write(data, 0, data.Length);
                stream.Close();
                tcpClient.Close();
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
                SendingManager(SendingType.Msg);
                tcpClient = new TcpClient("127.0.0.1", 8080);
                //SendingManager(SendingType.Msg);
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
                //сообщаем что дальше будет файл
                //SendMessageToServer("Uploading file: " + path);

                //сообщаем что дальше будет файл
                SendingManager(SendingType.File);

                //посылаем расширение файла
                string filename = path.Substring(path.LastIndexOf('\\')+1);
                
                SendMessageToServer(filename);//path.Substring(path.Split('/'));

                //
                tcpClient = new TcpClient("127.0.0.1", 8080);
                            //TcpClient client = new TcpClient();
                            //NetworkStream netStream;
                            //try
                            //{
                            //    client.Connect(new IPEndPoint(ipAddress, port));
                            //}

                            //catch (Exception ex)
                            //{
                            //    Console.WriteLine(ex.Message);
                            //}
                NetworkStream stream = tcpClient.GetStream();

                //посылаем длину файла  
                Console.WriteLine("Сокет соединился c cервером");
                //byte[] msg = Encoding.GetEncoding(1251).GetBytes(filename + "<TheEnd>");
                byte[] data = File.ReadAllBytes(path);
                byte[] dataLength = BitConverter.GetBytes(data.Length);
                byte[] package = new byte[4 + data.Length];
                dataLength.CopyTo(package, 0);
                data.CopyTo(package, 4);
                int bytesSent = 0;
                int bytesLeft = package.Length;
                        //stream.Write(data, 0, data.Length);
                while (bytesLeft > 0)
                {
                    int nextPacketSize = (bytesLeft > 1024) ? 1024 : bytesLeft;
                    stream.Write(package, bytesSent, nextPacketSize);
                    bytesSent += nextPacketSize;
                    bytesLeft -= nextPacketSize;
                }
                stream.Close();
                tcpClient.Close();
                //serialize file with BinaryFormatter
                
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
