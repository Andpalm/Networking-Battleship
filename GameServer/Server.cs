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
        List<ClientHandler> loggedinUsers = new List<ClientHandler>();
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
                    NetworkStream networkStream = client.GetStream();
                    var message = new BinaryReader(networkStream).ReadString();
                    Message messageInformation = JsonConvert.DeserializeObject<Message>(message);

                    var clientHandler = new ClientHandler(client, this, messageInformation.UserName);
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
                case AllActions.InGame:
                    InGame(loggedinUsers, message);
                    break;
                default:
                    break;
            }
        }

        private void InGame(List<ClientHandler> loggedinUsers, Message message)
        {
            int playerindex;
            int notplayingIndex;

            if (loggedinUsers[0].userName == message.UserName && loggedinUsers[0].playing)
            {
                playerindex = 0;
                notplayingIndex = 1;
            }
            else
            {
                playerindex = 1;
                notplayingIndex = 0;
            }

            if (loggedinUsers[playerindex].userName == message.UserName)
            {
                if (message.Text == "1" || message.Text == "2" || message.Text == "3" )
                {
                    message.SticksLeft -= int.Parse(message.Text);

                    if (message.SticksLeft < 1)
                    {
                        message.Text = $"{message.UserName} picked the last stick and lost! Congratulations {loggedinUsers[notplayingIndex].userName}!";
                        message.Action = AllActions.GameOver;
                        string messageJson = JsonConvert.SerializeObject(message);

                        foreach (var item in loggedinUsers)
                        {
                            NetworkStream networkStream = item.client.GetStream();
                            var binaryWriter = new BinaryWriter(networkStream);
                            binaryWriter.Write(messageJson);
                            binaryWriter.Flush();
                        }
                    }
                    else
                    {
                        loggedinUsers[playerindex].playing = false;
                        loggedinUsers[notplayingIndex].playing = true;

                        message.Text = $"{message.SticksLeft} sticks left! It´s {loggedinUsers[notplayingIndex].userName}:s turn.";
                        string messageJson = JsonConvert.SerializeObject(message);
                        foreach (var item in loggedinUsers)
                        {
                            NetworkStream networkStream = item.client.GetStream();
                            var binaryWriter = new BinaryWriter(networkStream);
                            binaryWriter.Write(messageJson);
                            binaryWriter.Flush();
                        }
                    }
                }
                else
                {

                }
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
                    loggedinUsers.Add(client);

                    if (loggedinUsers.Count == 2)
                    {
                        StartGame(loggedinUsers);
                    }
                    else
                    {
                        message.Text += " : Waiting for another user...";
                    }
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

        private void StartGame(List<ClientHandler> loggedinUsers)
        {
            string gametext1 = $"{loggedinUsers[0].userName} it´s your turn. Choose how many sticks(1-3) you want to remove from the pile. The one who takes the last stick loses.";
            string gametext2 = $"It´s {loggedinUsers[0].userName}:s turn. The one who takes the last stick loses.";

            loggedinUsers[0].playing = true;
            loggedinUsers[1].playing = false;

            Message message1 = new Message() { UserName = loggedinUsers[0].userName, Action = AllActions.InGame, Text = gametext1, SticksLeft = 21 };
            Message message2 = new Message() { UserName = loggedinUsers[1].userName, Action = AllActions.InGame, Text = gametext2, SticksLeft = 21 };
            string messagetosend1 = JsonConvert.SerializeObject(message1);
            string messagetosend2 = JsonConvert.SerializeObject(message2);


            NetworkStream networkStream = loggedinUsers[0].client.GetStream();
            var binaryWriter = new BinaryWriter(networkStream);
            binaryWriter.Write(messagetosend1);
            binaryWriter.Flush();

            NetworkStream networkStream1 = loggedinUsers[1].client.GetStream();
            var binaryWriter1 = new BinaryWriter(networkStream1);
            binaryWriter1.Write(messagetosend2);
            binaryWriter1.Flush();

        }

        private void Signup(ClientHandler client, Message message)
        {
            using (var db = new HippoContext())
            {
                var user = new User() { UserName = message.UserName, Password = message.Text };
                var users = db.User
                    .Where(username => username.UserName == message.UserName)
                    .ToList();

                if (users.Count == 0)
                {
                    db.User.Add(user);
                    db.SaveChanges();
                    message.Text = "User created";
                    message.Action = AllActions.Startgame;
                    loggedinUsers.Add(client);
                    if (loggedinUsers.Count == 2)
                    {
                        StartGame(loggedinUsers);
                    }
                    else
                    {
                        message.Text += " : Waiting for another user...";
                    }
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
            loggedinUsers.Remove(client);
            clients.Remove(client);
            Console.WriteLine($"User: x left.");
            Broadcast(client, $"User: x left.");
        }
    }
}
