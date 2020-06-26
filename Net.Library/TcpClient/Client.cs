using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SomeProject.Library.Client
{
    public class Client
    {
        public TcpClient tcpClient;
        TcpListener clientListener;
        IPAddress IP = IPAddress.Parse("127.0.0.1");// IPAddress.Loopback;
        IPEndPoint IPendpoint;
        int port = 8080;

        //событие получения сообщения от сервера

        public event EventHandler<string> DataFromServerRecieved;
        public event EventHandler<string> RecievingDataFailed;
        // подписать событие на обработчик из кода реализации ClientMainWindow

        public Client()
        {
            //clientListener = new TcpListener("127.0.0.1", 8080);
            IPendpoint = new IPEndPoint(IP, port);

        }
        //not used
        public async Task ListenToServer() {
            try {
                if (tcpClient != null)
                    while (tcpClient.Connected)
                    {

                        //tcpClient.Client.AcceptAsync();
                        //tcpClient.Client.Receive(data);
                        OperationResult result = await ReceiveMessageFromServer();
                    }

            }
            catch { }
        }
        //not used
        public async Task ListenForData()
        {
            try
            {
                if (tcpClient != null)
                    if (tcpClient.Connected) 
                        //await tcpClient.Connect(IPendpoint);
                // а TcpClient = new TcpClient();

                    while (true)
                    {
                        //await tcpClient.Client.AcceptAsync(); //перенесли в метод ReceiveMessage..

                        OperationResult result = await ReceiveMessageFromServer();
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
                //ListenForData();
                //TcpClient server = new TcpClient((IPEndPoint)tcpClient.Client.AcceptAsync().Result.RemoteEndPoint);
                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                //NetworkStream stream = server.GetStream();
                NetworkStream stream = tcpClient.GetStream();
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                //tcpClient.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }

        
        public async Task<OperationResult> SendMessageToServer(string message)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(IP, port); 
                //tcpClient.Client.ConnectAsync(IPendpoint);//tcpClient = new TcpClient("127.0.0.1", 8080);

                NetworkStream stream = tcpClient.GetStream();
                byte[] data = { 0 };
                stream.Write(data, 0, 1);
                data = System.Text.Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                //Логирование
                OperationResult result = ReceiveMessageFromServer().Result;// ListenToServer();
                if(result.Result == Result.OK)
                {
                    DataFromServerRecieved(this, result.Message);
                }
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
                //отправляем байт 0 или 1
                //SendingManager(SendingType.File);

                //посылаем расширение файла
                string filename = path.Substring(path.LastIndexOf('\\')+1);
                
                //SendMessageToServer(filename);//path.Substring(path.Split('/'));

                tcpClient = new TcpClient();
                tcpClient.Connect(IP, port); //tcpClient.Client.Connect(IPendpoint); //tcpClient = new TcpClient("127.0.0.1", 8080);

                NetworkStream stream = tcpClient.GetStream();
                //посылаем индикатор типа - 1 - файл
                byte[] data = { 1 };
                stream.Write(data, 0,1);
                //создать объект FilePackage

                //посылаем пакет с файлаом
                //Console.WriteLine("Сокет соединился c cервером");


                data = File.ReadAllBytes(path);
                //byte[] dataLength = BitConverter.GetBytes(data.Length);

                //Сериализация:                                 //serialize file with BinaryFormatter
                FilePackage p = new FilePackage(filename, data);
                IFormatter formatter = new BinaryFormatter(); // Модуль форматирования, который будет сериализовать класс
                formatter.Serialize(stream, p); // процесс сериализации

                //Логирование
                OperationResult result = ReceiveMessageFromServer().Result;// ListenToServer();
                if (result.Result == Result.OK)
                {
                    DataFromServerRecieved(this, result.Message);
                }
                stream.Close();

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
