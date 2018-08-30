using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public enum AllActions
    {
        Startup,
        Signup,
        Login,
        Startgame,
        InGame,
        GameOver
    }


    public class Message
    {
        public bool Loggedin { get; set; }
        public AllActions Action { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
        public bool Playing { get; set; }
        public int SticksLeft { get; set; }
    }
}
