using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeProject.Library.Server
{
    public class Server
    {
        TcpListener serverListener;
        TcpClient client = null;
        // перенести в Turn On
        bool fileInfNext;
        bool fileNext;
        bool msgNext;
        bool sysNext;
        //максимально возможное число соединений 
        int connectionsMax = 5;
        //текущее число  соединений
        int connectionsOn;
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

                    client = serverListener.AcceptTcpClientAsync().Result;

                    byte[] data = new byte[1];
                    NetworkStream stream = client.GetStream();
                    int packageType = stream.Read(data, 0, 1);
                    if (data[0] == 0)
                    {//проверка на fail!
                        result = await ReceiveMessageFromClient();
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
                        result = await ReceiveFileFromClient();
                        LogToClient(result.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }
        public async Task TurnOnListener2()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Start();
                fileInfNext = false; 
                fileNext = false;
                msgNext = false;
                sysNext = true;
                string filename = "";
                while (true)
                {
                    //выбирать между получением сообщения и получением файла
                    //считывание типа из OperationResult
                    
                    OperationResult result = null;
                    if (sysNext || fileInfNext || msgNext) { result = await ReceiveMessageFromClient(); Console.WriteLine("Method: ReceiveMessageFromClient"); }
                    else if (fileNext)
                    {
                        result = await ReceiveFileFromClient(filename);
                        LogToClient("Method: ReceiveFileFromClient for file:" + filename);
                    }


                    if (result != null) { 
                    if (result.Result == Result.Fail)
                            LogToClient("Unexpected error: " + result.Message);
                    else
                    {
                        //OperationResult resultIn = await ReceiveMessageFromClient();
                        
                        if (sysNext)
                        {
                            
                            sysNext = false;

                            if (result.Type == SendingType.Msg){
                                if (fileInfNext) {
                                        sysNext = false;
                                    }
                                else
                                    msgNext = true; sysNext = false;
                                    //fileNext = false; // ошибка- файла нет, вместо него msg
                                    //fileInfNext = false;
                                }
                            else{ 
                                fileInfNext = true;
                                    LogToClient("prepare to upload file");
                                sysNext = true;
                            }
                        }
                        else if (fileInfNext)
                        {
                            fileNext = true;
                            filename = result.Message;
                                LogToClient("prepare to upload file " + filename);
                            fileInfNext = false;
                            sysNext = false;
                        }
                        else if (msgNext)
                        {
                                LogToClient("New message from client: " + result.Message);
                            msgNext = false;
                            sysNext = true;
                        }
                        else if (fileNext)
                        {

                                LogToClient("Uploading file: " + filename);
                                LogToClient("File: " + filename + " uploaded");
                            fileInfNext = false;
                            fileNext = false;
                        }
                            //? еще  проверки?
                    }
                    }      
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }//бывшый 1
        public async void LogToClient(string msg)
        {
            Console.WriteLine(msg);
            await SendMessageToClient(msg);
        }
        public async Task<OperationResult> ReceiveMessageFromClient()
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

        public async Task<OperationResult> ReceiveFileFromClient(string filename = null)
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
        public async Task<OperationResult> ReceiveFileFromClient2(string filename)
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                Console.WriteLine("Uploading file >> ");
                //получаем расширение файла
                //TcpListener listen = new TcpListener(11000);
                //listen.Start();
                //client = listen.AcceptTcpClient();
                TcpClient client = serverListener.AcceptTcpClientAsync().Result; // serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                int bufferSize = 1024;
                int bytesRead = 0;
                int allBytesRead = 0;
                byte[] length = new byte[4];
                //получаем кол-во байт в получаемом файле
                bytesRead = stream.Read(length, 0, 4);
                int dataLength = BitConverter.ToInt32(length, 0);

                int bytesLeft = dataLength;
                byte[] data = new byte[dataLength];
                //Deserialize file
                //byte[] recievedFile = new byte[0];


                //do
                //{
                //    int bytes = stream.Read(data, 0, data.Length);
                //    //recievedFile.(data, bytes);
                //}
                //while (stream.DataAvailable);

                             

                while (bytesLeft > 0)
                {

                    int nextPacketSize = (bytesLeft > bufferSize) ? bufferSize : bytesLeft;
                    bytesRead = stream.Read(data, allBytesRead, nextPacketSize);
                    allBytesRead += bytesRead;
                    bytesLeft -= bytesRead;

                }
                // создаем файл методом
                ///File.WriteAllBytes
                //сохранение файла  (можно вынести в отдельный метод - SaveFileOnServer)
                filesTotal = Interlocked.Increment(ref filesTotal);
                string dirName = DateTime.Today.ToString("yyyy-MM-dd");
                FileInfo file = new FileInfo(serverPath + dirName + "\\" + filesTotal +"_" +filename);
                file.Directory.Create(); // если папка существует, метод ничего не делает
                File.WriteAllBytes(file.FullName, data); //File.WriteAllBytes("D:\\server\\"+ filename, data);


                data = System.Text.Encoding.UTF8.GetBytes("Server:  file received");
                stream.Write(data, 0, data.Length);
                stream.Close();
                client.Close();


                return new OperationResult(Result.OK, "recievedFile.ToString()",SendingType.File); 
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        } //бывшый 1

        public async Task<OperationResult> SendMessageToClient(string message)  //OperationResult
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