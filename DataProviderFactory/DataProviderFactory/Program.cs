using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;     //for reading settings from *.config files

//ADO.NET classes for working with databases.
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

//Exploring the ADO.NET Data Provider Factory Model
//Uses a *.config file to chacnge settings (e.g. database connection strings) without application recompilation.
//Accesses the AutoLot database

namespace DataProviderFactory
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("***** Fun with Data Provider Factories ******\n");

            //Get Connection string/provider from *.config.
            string dp = ConfigurationManager.AppSettings["provider"];
            string cnStr = ConfigurationManager.ConnectionStrings["AutoLotSqlProvider"].ConnectionString;

            //get the factory provider
            DbProviderFactory df = DbProviderFactories.GetFactory(dp);

            //now get the connection object
            using (DbConnection cn = df.CreateConnection())
            {
                Console.WriteLine("Your connection object is a : {0}", cn.GetType().Name);
                cn.ConnectionString = cnStr;
                cn.Open();

                if (cn is SqlConnection)
                {
                    Console.WriteLine("\nSql Server Version: {0}.\n", ((SqlConnection)cn).ServerVersion);
                }

                //make command object
                DbCommand cmd = df.CreateCommand();
                Console.WriteLine("Your connection object is a : {0}", cmd.GetType().Name);
                cmd.Connection = cn;
                cmd.CommandText = "Select * From Inventory";

                //print out the table with data reader
                using (DbDataReader dr = cmd.ExecuteReader())
                {
                    Console.WriteLine("Your connection object is a : {0}", dr.GetType().Name);
                    Console.WriteLine("\n****** Current Inventory *****");
                    while (dr.Read())
                    {
                        //using the indexer method syntax (e.g. []) to access a colun in a certain record
                        Console.WriteLine("-> Car #{0} is a {1}.", dr["CarID"], dr["Make"].ToString());

                        //OR
                        //using the zero-based indexer way.
                        Console.WriteLine("***** Record *****");
                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            Console.WriteLine("{0} = {1} ", dr.GetName(i), dr.GetValue(i).ToString());
                        }
                        Console.WriteLine();
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
