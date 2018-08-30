using Newtonsoft.Json;
using Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class ClientHandler
    {
        public TcpClient client;
        private Server server;
        public string userName;
        public bool playing;

        public ClientHandler(TcpClient client, Server server, string userName)
        {
            this.client = client;
            this.server = server;
            this.userName = userName;
            playing = false;
        }

        public void Run()
        {
            try
            {
                string message = "";
                while (!message.Equals("quit"))
                {
                    NetworkStream networkStream = client.GetStream();
                    string messageJson = new BinaryReader(networkStream).ReadString();
                    Message messageInformation = JsonConvert.DeserializeObject<Message>(messageJson);
                    message = messageInformation.Text;

                    //server.Broadcast(this, message);
                    server.ResponseToClient(this, messageInformation);
                    Console.WriteLine(message);
                }

                server.DisconnectClient(this);
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
