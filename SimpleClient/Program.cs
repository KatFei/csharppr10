using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SimpleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Loopback, 8080);
            if (client.Connected)
            {
                using (StreamWriter writer = new StreamWriter(client.GetStream()))
                using (StreamReader reader = new StreamReader(client.GetStream()))
                {
                    Console.Write("Введите сообщение: ");
                    string message = Console.ReadLine();
                    writer.WriteLine(message);
                    writer.Flush();
                    message = reader.ReadLine();
                    Console.WriteLine(message);
                }
                client.Close();
            }
            else
            {
                Console.WriteLine("Невозможно установить соединение!");
            }
            Console.ReadKey();
        }
    }
}
