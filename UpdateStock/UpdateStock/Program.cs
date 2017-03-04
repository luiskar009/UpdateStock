using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Configuration;


namespace UpdateStock
{
    class Program
    {
        static void Main(string[] args)
        {
            /* MySql Connection */

            MySqlConnection connMySql = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySqlDB"].ConnectionString);


            /* SqlConnection */

            String a = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;

            SqlConnection connSqlServer = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString);
        }

    }
}
