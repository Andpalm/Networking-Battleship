using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            var thread = new Thread(server.Run);
            thread.Start();
            thread.Join();
        }
    }
}
