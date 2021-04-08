using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace ParallelTCPServer
{
    public class ServerObject
    {
        static TcpListener tcpListener; 
        List<ClientObject> clients = new List<ClientObject>(); 

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
            Console.WriteLine($"Active clinet: {clients.Count}.");
        }
        protected internal void RemoveConnection(string id)
        {

            ClientObject client = clients.FirstOrDefault(c => c.Id == id);

            if (client != null)
                clients.Remove(client);
            Console.WriteLine($"Active clinet: {clients.Count}.");
        }

        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            foreach (ClientObject i in clients)
            {
                if (i.Id != id)
                {
                    i.Stream.Write(data, 0, data.Length); 
                }
            }
        }
        protected internal void BroadcastMessage_(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            foreach (ClientObject i in clients)
            {
                if (i.Id == id) 
                {
                    i.Stream.Write(data, 0, data.Length);
                }
            }
        }
        protected internal void Disconnect()
        {
            tcpListener.Stop();

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); 
            }
            Console.WriteLine($"Active clinet: {clients.Count}.");
            Environment.Exit(0); 
        }
    }
}
