using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameClient
{
    class Program
    {

        static void Main(string[] args)
        {
            //Console.Write("Enter your name: ");
            //string userName = Console.ReadLine();

            //Console.Write("Write message: ");
            //string text = Console.ReadLine();
            var client = new Client();
            var thread = new Thread(client.Start);
            thread.Start();
            thread.Join();
            //validateLogin(getInput("Enter text: "));
        }

        static string getInput(string text)
        {
            Console.Write(text);

            return Console.ReadLine();
        }

        static void validateLogin(string text)
        {
            // send to server

            // get result and print to console
        }
    }
}
