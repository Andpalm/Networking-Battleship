using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public enum AllActions
    {
        Signup,
        Login,

    }

    public class Message
    {
        public bool Loggedin { get; set; }
        public AllActions Action { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
    }
}
