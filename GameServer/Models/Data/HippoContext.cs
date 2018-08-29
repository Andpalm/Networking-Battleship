using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace GameServer.Models.Data
{
    class HippoContext : DbContext
    {
        public DbSet<User> User { get; set; }

        // 
        //public HippoContext() : base("Server=.;Database=Hippo;Trusted_Connection=True;")
        public HippoContext() : base("Data Source=.;Initial Catalog=Hippo;Integrated Security=True")
        {

        }
    }

    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
