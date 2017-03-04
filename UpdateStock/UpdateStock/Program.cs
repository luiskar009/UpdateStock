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

            String connStringMySql = ConfigurationManager.ConnectionStrings["MySqlDB"].ConnectionString;


            /* SqlConnection */

            String connStringSqlServer = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;

            /* Save the articules to update stock in a list */

            createList(connStringMySql);
        }

        public static List<String> createList(String connStringMySql)
        {
            List<String> list = new List<string>();
            using (MySqlConnection conn = new MySqlConnection(connStringMySql.ToString()))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT Articulo FROM InventarioTablas", conn);
                conn.Open();
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(rdr["Articulo"].ToString());
                    }
                }
            }
            return list;
        }

    }
}
