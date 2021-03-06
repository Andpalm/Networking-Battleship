﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Protocol;

namespace GameClient
{
    class Client
    {
        private TcpClient client;
        public string UserName { get; set; }
        public AllActions ClientAction { get; set; }
        public Message Protocol { get; set; } = new Message();


        public void Start()
        {
            Console.Write("Enter name: ");
            UserName = Console.ReadLine();
            Startup("");
            client = new TcpClient("10.20.38.155", 5000);

            var listenerThread = new Thread(Send);
            listenerThread.Start();

            var senderThread = new Thread(Listen);
            senderThread.Start();

            senderThread.Join();
            listenerThread.Join();
        }

        public void Listen()
        {
            string message = "";

            try
            {
                while (true)
                {
                    NetworkStream networkStream = client.GetStream();
                    message = new BinaryReader(networkStream).ReadString();

                    Protocol = JsonConvert.DeserializeObject<Message>(message);
                    ClientAction = Protocol.Action;
                    switch (ClientAction)
                    {
                        case AllActions.Startup:
                            Startup(Protocol.Text);
                            break;
                        case AllActions.Signup:
                            SignUp();
                            break;
                        case AllActions.Login:
                            LogIn();
                            break;
                        case AllActions.Startgame:
                            StartGame(Protocol.Text);
                            break;
                        case AllActions.InGame:
                            InGame(Protocol.Text);
                            break;
                        case AllActions.GameOver:
                            Console.WriteLine(Protocol.Text);
                            break;
                        default:
                            break;
                    }
                    // Parse json and display text...
                    //Console.WriteLine($"{messageInformation.UserName}: {messageInformation.Text}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void InGame(string message)
        {
            Console.WriteLine(message);
        }

        private void StartGame(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Game Starts");
        }

        private void LogIn()
        {
            Console.WriteLine($"{UserName}: Write your password:");
        }

        private void Startup(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Signup [S]");
            Console.WriteLine("Login [L]");
        }

        private void SignUp()
        {
            Console.WriteLine($"{UserName}: Enter the password you want to use:");
        }

        public void Send()
        {
            string message = "";

            try
            {
                // Change to work with our code
                while (!message.Equals("quit"))
                {
                    NetworkStream networkStream = client.GetStream();


                    //Console.Write("Input: ");

                    message = Console.ReadLine();

                    Protocol.UserName = UserName;
                    Protocol.Text = message;
                    Protocol.Action = ClientAction;

                    var jsonProtocol = JsonConvert.SerializeObject(Protocol);

                    var binaryWriter = new BinaryWriter(networkStream);
                    binaryWriter.Write(jsonProtocol);
                    binaryWriter.Flush();
                }

                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
