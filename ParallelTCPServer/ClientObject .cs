using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ParallelTCPServer
{
    public struct Goods
    {

        public string country { get; set; }
        public string company { get; set; }
        public string name { get; set; }
        public int count { get; set; }
    };
    public class ClientObject
    {
        List<Goods> goods = new List<Goods>
        {
            new Goods()
            {
                country = "Germany",
                company = "Kögel",
                name = "Trailer",
                count = 130

            },
            new Goods()
            {
                country = "Germany",
                company = "MAN",
                name = "Truck",
                count = 95
            },
            new Goods()
            {
                country = "England",
                company = "Fly",
                name = "Phone",
                count = 3000

            },
            new Goods()
            {
                country = "England",
                company = "Cooke",
                name = "Photo lens",
                count = 347
            },
            new Goods()
            {
                country = "USA",
                company = "Ford",
                name = "Car(Ford mustang)",
                count = 300

            },
            new Goods()
            {
                country = "USA",
                company = "Hewlett-Packard",
                name = "Laptops",
                count = 847
            }
        };
        static int namber = 0;
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        int userName;
        TcpClient client;
        ServerObject server;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                string message;
                userName = ++namber;

                message = $"#{userName} ID: {Id} IP: {((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()} Connected";
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);
                while (true)
                {
                    try
                    {
                        message = GetMessage();

                        if (message == "exit")
                            break;

                        Console.WriteLine($"INPUT #{userName} ID: {Id} IP: {((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}:\n{message}");

                        string m = null;
                        foreach (Goods i in goods)
                        {
                            if (i.country.ToLower() == message.ToLower())
                            {
                                m += $"country: {i.country} company: {i.company} name: {i.name} count: {i.count}\n";
                            }
                        }
                        if (m == null)
                            m = "SORRY. Dont find";
                        Console.WriteLine($"OUTPUT #{userName} ID: {Id} IP: {((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}:\n{m}");
                        server.BroadcastMessage_(m, this.Id);
                    }
                    catch
                    {
                        message = $"ID: {Id} IP: {((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()} Disconnected";
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.BroadcastMessage($"ID: {Id} IP: {((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()} Disconnected", this.Id);
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        private string GetMessage()
        {
            byte[] data = new byte[64]; 
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);
            
            return builder.ToString();
        }

        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
