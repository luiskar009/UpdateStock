using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using System.IO;

namespace UpdateStock
{
    class Program
    {

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                                                   ///
        ///                                               Main Program                                                        ///
        ///                                                                                                                   ///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        static void Main(string[] args)
        {
            /* MySql Connection */

            String connStringMySql = ConfigurationManager.ConnectionStrings["MySqlDB"].ConnectionString;


            /* SqlConnection */

            String connStringSqlServer = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;

            /* Save the articules to update stock in a list */

            List<String> listArticules = createListArticules(connStringMySql);

            /* Update table InventarioTablas */

            updateInventoryTable(connStringMySql, connStringSqlServer, listArticules);

            /* Save the id_products to update stock in a list */

            List<String> listIdProducts = createListIdProducts(connStringMySql);

            /* Update Prestashop stock table */

            updateStock(connStringMySql, listIdProducts);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                                                   ///
        ///                             Create a List with the articules to update the stock                                  ///
        ///                                                                                                                   ///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static List<String> createListArticules(String connStringMySql)
        {
            List<String> list = new List<string>();
            String path = $"{Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))}UpdateStock";
            if (!(Directory.Exists(path)))
                Directory.CreateDirectory(path);
            try
            {
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
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter($@"{path}\\Error-{DateTime.Now.ToString("dd-MM-yyyy")}.txt", true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }

            return list;
        }//createList

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                                                   ///
        ///                                 Update table InventarioTabla with stock                                           ///
        ///                                                                                                                   ///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void updateInventoryTable(String connStringMySql, String connStringSqlServer, List<String> list)
        {
            String path = $"{Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))}UpdateStock";
            if (!(Directory.Exists(path)))
                Directory.CreateDirectory(path);
            try
            {
                using (SqlConnection conn = new SqlConnection(connStringSqlServer))
                {
                    foreach (String element in list)
                    {
                        SqlCommand cmd = new SqlCommand($"SELECT UnidadesStock FROM ocartacp WHERE Articulo = '{element}'", conn);
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            rdr.Read();
                            String Stock = rdr[0].ToString();
                            using (MySqlConnection conn2 = new MySqlConnection(connStringMySql))
                            {
                                MySqlCommand cmd2 = new MySqlCommand($"UPDATE InventarioTablas SET Stock = '{Stock}' WHERE Articulo = '{element}';", conn2);
                                MySqlCommand cmd3 = new MySqlCommand($"UPDATE InventarioTablas SET xxxFechaHoraActualizacion = now() WHERE Articulo = '{element}';", conn2);
                                if (conn2.State == ConnectionState.Closed)
                                    conn2.Open();
                                cmd2.ExecuteNonQuery();
                                cmd3.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter($@"{path}\\Error-{DateTime.Now.ToString("dd-MM-yyyy")}txt", true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }//updateInventoryTable

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                                                   ///
        ///                             Create a List with the id_products to update the stock                                  ///
        ///                                                                                                                   ///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static List<String> createListIdProducts(String connStringMySql)
        {
            List<String> list = new List<string>();
            String path = $"{Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))}UpdateStock";
            if (!(Directory.Exists(path)))
                Directory.CreateDirectory(path);
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStringMySql))
                {
                    MySqlCommand cmd = new MySqlCommand("SELECT id_products FROM InventarioTablas", conn);
                    conn.Open();
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(rdr["id_products"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter($@"{path}\\Error-{DateTime.Now.ToString("dd-MM-yyyy")}.txt", true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }

            return list;
        }//createListIdProducts

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                                                   ///
        ///                                       Update Prestashop Articules Stock                                           ///
        ///                                                                                                                   ///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void updateStock(String connStringMySql, List<String> list)
        {
            String path = $"{Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))}UpdateStock";
            if (!(Directory.Exists(path)))
                Directory.CreateDirectory(path);
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStringMySql.ToString()))
                {
                    foreach (String element in list)
                    {
                        MySqlCommand cmd = new MySqlCommand($"SELECT Stock FROM InventarioTablas WHERE id_product = '{element}'", conn);
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            rdr.Read();
                            String Stock = rdr[0].ToString();
                            if (conn.State == ConnectionState.Open)
                                conn.Close();
                            MySqlCommand cmd2 = new MySqlCommand($"UPDATE ps_stock_available SET quantity = '{Stock}' WHERE id_product  = '{element}'", conn);
                            if (conn.State == ConnectionState.Closed)
                                conn.Open();
                            cmd2.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter($@"{path}\\Error-{DateTime.Now.ToString("dd-MM-yyyy")}.txt", true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }//updateStock
    }
}
