using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeProject.Library.Server
{
    public class Server
    {
        TcpListener serverListener;
        //TcpClient client = null;
        List<TcpClient> listConnectedClients = new List<TcpClient>();
        //максимально возможное число соединений 
        int connectionsMax = 2;
        //текущее число  соединений
        int connectionsOn = 0;
        int filesTotal = 0;
        string serverPath = "D:\\server\\";

        public Server()
        {
            serverListener = new TcpListener(IPAddress.Loopback, 8080);
            
            string dirName = DateTime.Today.ToString("yyyy-MM-dd");
            //если сегодня файлы уже записывались
            if(Directory.Exists(serverPath + dirName))
                filesTotal = Directory.GetFiles(serverPath + dirName).Length;
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
                    //считывание типа из OperationResult

                    OperationResult result = null;
                    if (connectionsOn < connectionsMax)
                    {
                        TcpClient client = serverListener.AcceptTcpClientAsync().Result;
                        //проверять нет ли клиента в списке???
                        listConnectedClients.Add(client);
                        Interlocked.Increment(ref connectionsOn);
                        byte[] data = new byte[1];
                        NetworkStream stream = client.GetStream();
                        int packageType = stream.Read(data, 0, 1);
                        if (data[0] == 0)
                        {//проверка на fail!
                            result = await ReceiveMessageFromClient(listConnectedClients.Last());
                            Console.WriteLine(result.Message);
                        }
                        else if (data[0] == 1)
                        {
                            //string filename = "";
                            ////считываем длину имени файла
                            //int nameLength = 10;
                            //data = new byte[nameLength];
                            //int packageLength = stream.Read(data, 0, nameLength);
                            ////считываем длину имя файла + расширение
                            //int fileLength = Int32.Parse(data.ToString());
                            //data = new byte[fileLength];
                            //int packageLength = stream.Read(data, 0, length);
                            result = await ReceiveFileFromClient(listConnectedClients.Last());
                            LogToClient(listConnectedClients.Last(), result.Message);
                        }
                    }
                    else
                        Console.WriteLine("Too many connections. Server is busy");
                    //!!! проверять подключен ли сервер
                    ///закрывать соединение в самих await методах
                    ///Interlocked.Decrement(ref connectionsOn);
                    await CheckConnections();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }
        
        public async Task CheckConnections()
        {
            try
            {
                for(int i= 0; i< listConnectedClients.Count; i++) {
                    //listConnectedClients[i].GetStream().Write();
                    if (!listConnectedClients[i].Connected)
                    {
                        listConnectedClients.RemoveAt(i);
                        Interlocked.Decrement(ref connectionsOn);
                    }    
                }
                //connectionsOn = listConnectedClients.Count;
            }
            catch
            {
                Console.WriteLine("Connections check failed");
            }
        }
        public async void LogToClient(TcpClient client, string msg)
        {
            Console.WriteLine(msg);
            await SendMessageToClient(client, msg);
        }
        public async Task<OperationResult> ReceiveMessageFromClient(TcpClient client)
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                StringBuilder recievedMessage = new StringBuilder();
                //client = serverListener.AcceptTcpClientAsync().Result;

                byte[] data = new byte[256];
                NetworkStream stream = client.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);

                data = System.Text.Encoding.UTF8.GetBytes("Server:  message received");
                stream.Write(data, 0, data.Length);
                stream.Close();
                client.Close();
                Console.WriteLine("Message >> " + recievedMessage.ToString());
                //if (recievedMessage.ToString() == "file")
                //    return new OperationResult(Result.OK, recievedMessage.ToString(), SendingType.File);                
                //else
                    return new OperationResult(Result.OK, "New message from client: " +  recievedMessage.ToString(), SendingType.Msg);

                

            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        public async Task<OperationResult> ReceiveFileFromClient(TcpClient client, string filename = null)
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                Console.WriteLine("Uploading file >> ");
                //получаем расширение файла
                //TcpListener listen = new TcpListener(11000);
                //listen.Start();
                //client = listen.AcceptTcpClient();
                //client = serverListener.AcceptTcpClientAsync().Result; // serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                //Deserialize file
                //Десериализация:
                IFormatter formatter = new BinaryFormatter();
                FilePackage p = (FilePackage)formatter.Deserialize(stream);
                filename = p.Filename;
               
                // создаем файл методом  File.WriteAllBytes
                //сохранение файла  (можно вынести в отдельный метод - SaveFileOnServer)
                filesTotal = Interlocked.Increment(ref filesTotal);
                string dirName = DateTime.Today.ToString("yyyy-MM-dd");
                FileInfo file = new FileInfo(serverPath + dirName + "\\" + filesTotal + "_" + filename);
                file.Directory.Create(); // если папка существует, метод ничего не делает
                File.WriteAllBytes(file.FullName, p.Attachment); //File.WriteAllBytes("D:\\server\\"+ filename, data);


                byte[] data = System.Text.Encoding.UTF8.GetBytes("Server:  file received");
                stream.Write(data, 0, data.Length);
                stream.Close();
                client.Close();


                return new OperationResult(Result.OK, "File " + filename + " uploaded", SendingType.File);
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        public async Task<OperationResult> SendMessageToClient(TcpClient client, string message)  //OperationResult
        {
            try
            {
                //TcpClient client = serverListener.AcceptTcpClient();
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