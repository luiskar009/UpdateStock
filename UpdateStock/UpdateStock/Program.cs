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

            /* Program Directory */

            String path = $"{Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))}UpdateStock";
            if (!(Directory.Exists(path)))
                Directory.CreateDirectory(path);
            if (!(Directory.Exists($"{path}\\LOG")))
                Directory.CreateDirectory($"{path}\\LOG");

            /* Inicialize LOG */

            using (StreamWriter writer = new StreamWriter($@"{path}\\LOG\\{DateTime.Now.ToString("dd-MM-yyyy")}.log", true))
            {
                writer.WriteLine("día:" + DateTime.Now.ToString("dd-MM-yyyy") + "  hora:" + DateTime.Now.ToString("HH:mm:ss") + " - Arrancando aplicacion");
            }

            /* Save the articules to update stock in a list */

            List<String> listArticules = createListArticules(connStringMySql, path);

            /* Update table InventarioTablas */

            List<String> listArticulesUpdated = updateInventoryTable(connStringMySql, connStringSqlServer, listArticules, path);

            /* Save the id_products to update stock in a list */

            if (listArticulesUpdated.Count == 0)
            {
                using (StreamWriter writer = new StreamWriter($@"{path}\\LOG\\{DateTime.Now.ToString("dd-MM-yyyy")}.log", true))
                {
                    writer.WriteLine("día:" + DateTime.Now.ToString("dd-MM-yyyy") + "  hora:" + DateTime.Now.ToString("HH:mm:ss") + " - Ningun producto necesita actualizacion. Finalizando Aplicacion");
                }
            }
            else
            {

                List<String> listIdProducts = createListIdProducts(connStringMySql, listArticulesUpdated, path);

                /* Update Prestashop stock table */

                updateStock(connStringMySql, listIdProducts, path);
                using (StreamWriter writer = new StreamWriter($@"{path}\\LOG\\{DateTime.Now.ToString("dd-MM-yyyy")}.log", true))
                {
                    writer.WriteLine("día:" + DateTime.Now.ToString("dd-MM-yyyy") + "  hora:" + DateTime.Now.ToString("HH:mm:ss") + " - Productos actualizados. Finalizando Aplicacion");
                }

            }
            using (StreamWriter writer = new StreamWriter($@"{path}\\LOG\\{DateTime.Now.ToString("dd-MM-yyyy")}.log", true))
            {
                writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                                                   ///
        ///                             Create a List with the articules to update the stock                                  ///
        ///                                                                                                                   ///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static List<String> createListArticules(String connStringMySql, String path)
        {
            List<String> list = new List<string>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStringMySql))
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
                using (StreamWriter writer = new StreamWriter($@"{path}\\Error-{DateTime.Now.ToString("dd-MM-yyyy")}.log", true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }

            return list;
        }//createListAritucles

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                                                   ///
        ///                                 Update table InventarioTabla with stock                                           ///
        ///                                                                                                                   ///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static List<String> updateInventoryTable(String connStringMySql, String connStringSqlServer, List<String> list, String path)
        {
            int i = 0;
            String Stock = "";
            String StockDestino = "";
            List<String> listUpdate = new List<String>();
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
                            Stock = rdr[0].ToString();
                        }
                        using (MySqlConnection conn2 = new MySqlConnection(connStringMySql))
                        {
                            MySqlCommand cmd2 = new MySqlCommand($"SELECT Stock FROM InventarioTablas WHERE Articulo = '{element}';", conn2);
                            if (conn2.State == ConnectionState.Closed)
                                conn2.Open();
                            using (MySqlDataReader rdr2 = cmd2.ExecuteReader())
                            {
                                rdr2.Read();
                                StockDestino = rdr2[0].ToString();
                            }
                            if (Stock == StockDestino)
                            {
                                continue;
                            }
                            else
                            {
                                MySqlCommand cmd3 = new MySqlCommand($"UPDATE InventarioTablas SET Stock = '{Stock}' WHERE Articulo = '{element}';", conn2);
                                MySqlCommand cmd4 = new MySqlCommand($"UPDATE InventarioTablas SET xxxFechaHoraActualizacion = now() WHERE Articulo = '{element}';", conn2);
                                if (conn2.State == ConnectionState.Closed)
                                    conn2.Open();
                                cmd3.ExecuteNonQuery();
                                cmd4.ExecuteNonQuery();
                                i++;
                                listUpdate.Add(element);
                            }
                        }

                    }
                    using (StreamWriter writer = new StreamWriter($@"{path}\\LOG\\{DateTime.Now.ToString("dd-MM-yyyy")}.log", true))
                    {
                        writer.WriteLine("día:" + DateTime.Now.ToString("dd-MM-yyyy") + "  hora:" + DateTime.Now.ToString("HH:mm:ss") + " - Candidatos procesados. Total a subir: " + i);
                    }
                }

            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter($@"{path}\\Error-{DateTime.Now.ToString("dd-MM-yyyy")}.log", true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }

            return listUpdate;
        }//updateInventoryTable


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                                                   ///
        ///                             Create a List with the id_products to update the stock                                ///
        ///                                                                                                                   ///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static List<String> createListIdProducts(String connStringMySql, List<String> listUpdate, String path)
        {
            List<String> list = new List<string>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStringMySql))
                {
                    foreach (String element in listUpdate)
                    {
                        MySqlCommand cmd = new MySqlCommand($"SELECT id_product FROM InventarioTablas WHERE Articulo = '{element}'", conn);
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            rdr.Read();
                            list.Add(rdr[0].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter($@"{path}\\Error-{DateTime.Now.ToString("dd-MM-yyyy")}.log", true))
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

        public static void updateStock(String connStringMySql, List<String> list, String path)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStringMySql))
                {
                    foreach (String element in list)
                    {
                        MySqlCommand cmd = new MySqlCommand($"SELECT Stock, id_product_attribute FROM InventarioTablas WHERE id_product = '{element}'", conn);
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            rdr.Read();
                            String Stock = rdr["Stock"].ToString();
                            String id_product_attribute = rdr["id_product_attribute"].ToString();
                            if (conn.State == ConnectionState.Open)
                                conn.Close();
                            MySqlCommand cmd2 = new MySqlCommand($"UPDATE ps_stock_available SET quantity = '{Stock}' WHERE id_product  = '{element}' AND id_product_attribute = '{id_product_attribute}'", conn);
                            if (conn.State == ConnectionState.Closed)
                                conn.Open();
                            cmd2.ExecuteNonQuery();
                            using (StreamWriter writer = new StreamWriter($@"{path}\\LOG\\{DateTime.Now.ToString("dd-MM-yyyy")}.log", true))
                            {
                                writer.WriteLine("día:" + DateTime.Now.ToString("dd-MM-yyyy") + "  hora:" + DateTime.Now.ToString("HH:mm:ss") + " - Producto actualizado: " + element);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter($@"{path}\\Error-{DateTime.Now.ToString("dd-MM-yyyy")}.log", true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }//updateStock
    }
}


