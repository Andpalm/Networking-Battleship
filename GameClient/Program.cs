using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    class Program
    {
        static void Main(string[] args)
        {
                string inputUsername = getInput("Username: ");
                string inputPassword = getInput("Password: ");
                validateLogin(inputUsername, inputPassword);
        }

        static string getInput(string text)
        {
            Console.Write(text);

            return Console.ReadLine();
        }

        static void validateLogin(string username, string password)
        {
            // send to server

            // get result and print to console
        }
    }
}
