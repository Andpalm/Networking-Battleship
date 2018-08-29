using GameServer.Models.Data;
using Newtonsoft.Json;
using Protocol;
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
        public AllActions ServerAction { get; set; }
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

        public void ResponseToClient(ClientHandler client, Message message)
        {
            switch (message.Action)
            {
                case AllActions.Startup:
                    Startup(client, message);
                    break;
                case AllActions.Signup:
                    Signup(client, message);
                    break;
                case AllActions.Login:
                    LogIn(client, message);
                    break;
                default:
                    break;
            }
        }

        private void LogIn(ClientHandler client, Message message)
        {
            using (var db = new HippoContext())
            {
                var users = db.User
                    .Where(userinfo => userinfo.UserName == message.UserName && userinfo.Password == message.Text)
                    .ToList();

                if (users.Count == 1)
                {
                    message.Text = "You signed in";
                    message.Action = AllActions.Startgame;
                }
                else
                {
                    message.Text = "User dont exist or the password is wrong";
                    message.Action = AllActions.Startup;
                }
            }

            NetworkStream networkStream = client.client.GetStream();
            var binaryWriter = new BinaryWriter(networkStream);
            string jsonMessage = JsonConvert.SerializeObject(message);
            binaryWriter.Write(jsonMessage);
            binaryWriter.Flush();
        }

        private void Signup(ClientHandler client, Message message)
        {
            using (var db = new HippoContext())
            {
                var user = new User() { UserName = message.UserName, Password = message.Text };
                var users = db.User
                    .Where(username => username.UserName == message.UserName)
                    .ToList();

                if (users.Count == 0 )
                {
                    db.User.Add(user);
                    db.SaveChanges();
                    message.Text = "User created";
                    message.Action = AllActions.Startgame;
                }
                else
                {
                    message.Text = "User allready exists";
                    message.Action = AllActions.Startup;
                }
            }

            NetworkStream networkStream = client.client.GetStream();
            var binaryWriter = new BinaryWriter(networkStream);
            string jsonMessage = JsonConvert.SerializeObject(message);
            binaryWriter.Write(jsonMessage);
            binaryWriter.Flush();
        }

        private void Startup(ClientHandler client, Message message)
        {
            if (message.Text.ToLower() == "s")
            {
                message.Action = AllActions.Signup;
            }
            else if (message.Text.ToLower() == "l")
            {
                message.Action = AllActions.Login;
            }

            NetworkStream networkStream = client.client.GetStream();
            var binaryWriter = new BinaryWriter(networkStream);
            string jsonMessage = JsonConvert.SerializeObject(message);
            binaryWriter.Write(jsonMessage);
            binaryWriter.Flush();
        }

        public void DisconnectClient(ClientHandler client)
        {
            clients.Remove(client);
            Console.WriteLine($"User: x left.");
            Broadcast(client, $"User: x left.");
        }
    }
}
