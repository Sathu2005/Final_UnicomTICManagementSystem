using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Class1
    {
        using System.Data.SQLite;

public class Test
    {
        public void Connect()
        {
            var conn = new SQLiteConnection("Data Source=unicomtic.db");
            conn.Open();
        }
    }

}
}
