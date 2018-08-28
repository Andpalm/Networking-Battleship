using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    public class Server
    {
        List<ClientHandler> clients = new List<ClientHandler>();
        List<string> messages = new List<string>();
        public void Run()
        {
            var listener = new TcpListener(IPAddress.Any, 5000);
            Console.WriteLine("Server up and running, waiting for messages...");

            try
            {
                listener.Start();

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    var clientHandler = new ClientHandler(client, this);
                    clients.Add(clientHandler);

                    var clientThread = new Thread(clientHandler.Run);
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }
        }

        public void Broadcast(ClientHandler client, string message)
        {
            messages.Add(message);

            lock (messages)
            {

            }

            foreach (var item in clients)
            {
                NetworkStream networkStream = item.client.GetStream();
                var binaryWriter = new BinaryWriter(networkStream);
                binaryWriter.Write(message);
                binaryWriter.Flush();
            }
        }

        public void DisconnectClient(ClientHandler client)
        {
            clients.Remove(client);
            Console.WriteLine($"User: x left.");
            Broadcast(client, $"User: x left.");
        }
    }
}
