using System;
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


        public void Start()
        {
            Console.Write("Enter name: ");
            UserName = Console.ReadLine();
            Startup();
            client = new TcpClient("localhost", 5000);

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

                    Message messageInformation = JsonConvert.DeserializeObject<Message>(message);
                    ClientAction = messageInformation.Action;
                    switch (ClientAction)
                    {
                        case AllActions.Startup:
                            Startup();
                            break;
                        case AllActions.Signup:
                            SignUp();
                            break;
                        case AllActions.Login:
                            LogIn();
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

        private void LogIn()
        {
            Console.WriteLine($"{UserName}: Write your password:");
        }

        private void Startup()
        {
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

                    var messageInformation = new Message() { UserName = UserName, Text = message, Action = ClientAction };

                    var jsonProtocol = JsonConvert.SerializeObject(messageInformation);

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
