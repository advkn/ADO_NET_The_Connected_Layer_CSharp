using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Necessary for reading values from a *.config file.
using System.Configuration;

/*Required namespaces needed to retrieve the definitions of common interfaces
 and various connection objects.*/
using System.Data;
using System.Data.SqlClient;
using System.Data.Odbc;
using System.Data.OleDb;

namespace MyConnectionFactory
{
    //a list of possible providers
    enum DataProvider
    {
        SqlServer, OleDb, Odbc, None
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("***** Very Simple Connection MyConnectionFactory *****\n");

            //Read the provider key
            string dataProvString = ConfigurationManager.AppSettings["ServerProvider"];

            //Transform string to enum
            DataProvider dp = DataProvider.None;
            if (Enum.IsDefined(typeof(DataProvider), dataProvString))
            {
                dp = (DataProvider)Enum.Parse(typeof(DataProvider), dataProvString);
            }
            else
            {
                Console.WriteLine("Sorry, no provider exists!");
            }

            //get a specific connection
            IDbConnection myCn = GetConnection(dp);

            if(myCn != null) {
                Console.WriteLine("Your connection is {0}", myCn.GetType().Name);
            }


            Console.ReadLine();
        }

        //This method returns a specific connection object based on the valu of a DataProvider enum.
        static IDbConnection GetConnection(DataProvider dp)
        {
            IDbConnection conn = null;
            switch(dp) {
                case DataProvider.SqlServer:
                    conn = new SqlConnection();
                    break;
                case DataProvider.OleDb:
                    conn = new OleDbConnection();
                    break;
                case DataProvider.Odbc:
                    conn = new OdbcConnection();
                    break;
            }

            return conn;
        }
    }
}
