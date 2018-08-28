using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameClient
{
    class Client
    {
        private TcpClient client;

        public void Start()
        {
            client = new TcpClient("10.20.38.92", 5000);

            var listenerThread = new Thread(Send);
            listenerThread.Start("Admin");

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
                    Console.WriteLine($"Client text: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Send(object text)
        {
            string message = "";

            try
            {
                // Change to work with our code
                while (!message.Equals("quit"))
                {
                    NetworkStream networkStream = client.GetStream();
                    message = (string)text;
                    var binaryWriter = new BinaryWriter(networkStream);
                    binaryWriter.Write(message);
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
