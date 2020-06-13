using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SimpleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 8080);
            listener.Start();

            Console.WriteLine("Сервер запущен!");

            TcpClient client = listener.AcceptTcpClient();

            StreamReader reader = new StreamReader(client.GetStream());
            string message = reader.ReadLine();
            Console.WriteLine("Сообщение от клиента: " + message);

            Console.Write("Введите сообщение: ");
            message = Console.ReadLine();

            StreamWriter writer = new StreamWriter(client.GetStream());
            writer.WriteLine(message);
            writer.Flush();

            writer.Close();
            reader.Close();
            client.Close();
            listener.Stop();

            Console.ReadKey();
        }
    }
}
