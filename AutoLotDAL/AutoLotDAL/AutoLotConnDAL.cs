using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

/*This class library is used to interact with the 'AutoLot' database using the Connected types
 of ADO.NET.*/
namespace AutoLotConnectedLayer
{

    /// <summary>
    /// This class defines various members which allow interaction with the 'Inventory'
    /// table of the 'AutoLot' database.
    /// 'DAL' is an acronym for "Data Access Logic"
    /// </summary>
    public class InventoryDAL
    {
        //this member will be used by all methods
       private SqlConnection sqlConn = null;

       public void OpenConnection(string connectionString)
       {
           sqlConn = new SqlConnection();
           sqlConn.ConnectionString = connectionString;
           sqlConn.Open();
       }

        public void CloseConnection() {
            sqlConn.Close();
        }


        //Insert a new vehicle
        public void InsertAuto(int id, string color, string make, string petName)
        {
            //Format and execute the SQL statement, leveraging the use of parameter objects
            string sql = string.Format("Insert Into Inventory" +
                "(CarID, Make, Color, PetName) Values" + "(@CarID, @Make, @Color, @PetName)");

            //execute our connection
            //this command will have internal parameters
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlConn))
            {
                //Fill the params Collection
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@CarID";
                param.Value = id;
                param.SqlDbType = SqlDbType.Int;
                cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Make";
                param.Value = make;
                param.SqlDbType = SqlDbType.Char;
                param.Size = 10;
                cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Color";
                param.Value = color;
                param.SqlDbType = SqlDbType.Char;
                param.Size = 10;
                cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@PetName";
                param.Value = petName;
                param.SqlDbType = SqlDbType.Char;
                param.Size = 10;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
        }

        //Overloaded version of InsertAuto which accepts a strongly typed NewCar type
        public void InsertAuto(NewCar car)
        {
            //Format and execute the SQL statement.
            string sql = string.Format("Insert Into Inventory" +
                "(CarID, Make, Color, PetName) Values" + "('{0}', '{1}', '{2}', '{3}')", 
                car.CarID, car.Make, car.Color, car.PetName);

            //execute our connection
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlConn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        //Invokes the 'GetPetName' stored procedure
        public string LookUpPetName(int carID)
        {
            string carPetName = string.Empty;

            using (SqlCommand cmd = new SqlCommand("GetPetName", this.sqlConn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                //Input param
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@carId";
                param.SqlDbType = SqlDbType.Int;
                param.Value = carID;

                //set the direction (note: default is Input)
                param.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(param);

                //output param
                param = new SqlParameter();
                param.ParameterName = "@petName";
                param.SqlDbType = SqlDbType.Char;
                param.Size = 10;
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);

                //execute the stored procedure
                cmd.ExecuteNonQuery();

                //Return the output param
                carPetName = (string)cmd.Parameters["@petName"].Value;
            }
            return carPetName;
        }


        //Deletion logic. The try/catch logic handles the attempt to delete a Car it is on order.
        public void DeleteCar(int id)
        {
            //Get ID of car to delete, then do so.
            string sql = string.Format("Delete from Inventory where CarID = '{0}'", id);

            using (SqlCommand cmd = new SqlCommand(sql, this.sqlConn))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Exception error = new Exception("Sorry! That car is on order!", ex);
                    throw error;
                }
            }
        }

        
        //Retrieve the car id and update the petname
        public void UpdateCarPetName(int id, string newPetName)
        {
            string sql = string.Format("Update Inventory set PetName = '{0}' where CarID = '{1}'", newPetName, id);

            //execute our connection
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlConn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        //Select everything from the Inventory table, load it into a DataTable object and return that DataTable object.
        public DataTable GetAllInventoryAsDataTable()
        {
            //this will hold the records
            DataTable inv = new DataTable();

            //prep command object
            string sql = "Select * from Inventory";
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlConn))
            {
                SqlDataReader dr = cmd.ExecuteReader();

                //Fill the DataTable with data from the reader and cleanup.
                inv.Load(dr);
                dr.Close();
            }
            return inv;
        }

        //Process customers who are a credit risk.
        //Demonstrates the use of working with database transactions programmatically
        public void ProcessCreditRisks(bool throwEx, int custID)
        {
            //First look up the current name based on the customer ID
            string fName = string.Empty;
            string lName = string.Empty;
            SqlCommand cmdSelect = new SqlCommand(string.Format("Select * from Customers where CustID = {0}", custID),sqlConn);

            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                if (dr.HasRows)
                {
                    dr.Read();
                    fName = (string)dr["FirstName"];
                    lName = (string)dr["LastName"];
                }
                else
                {
                    return;
                }
            }

            //Create the command objects that represent each step of the operation
            //Remove customer from the Customers table.
            SqlCommand cmdRemove = new SqlCommand(string.Format("Delete from Customers where CustID = {0}", custID), sqlConn);

            //Insert "risky" customer into CreditRisks table.
            SqlCommand cmdInsert = new SqlCommand(string.Format("Insert into CreditRisks" +
                "(CustID, FirstName, LastName) Values" +
                "('{0}', '{1}', '{2}')", custID, lName, fName), sqlConn);

            //Initialize the transaction object
            //You will get this from the conection object.
            SqlTransaction tx = null;
            try
            {
                tx = sqlConn.BeginTransaction();

                //Enlist the commands into this transaction
                cmdInsert.Transaction = tx;
                cmdRemove.Transaction = tx;

                //execute the commands
                cmdInsert.ExecuteNonQuery();
                cmdRemove.ExecuteNonQuery();

                //simulate the error
                if (throwEx)
                {
                    throw new Exception("Sorry! Database error! Transaction failed.");
                }

                //Commit
                tx.Commit();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Any error will rollback the transaction
                tx.Rollback();
            }
        }
    }



    //New automobile class
    public class NewCar
    {
        public int CarID { get; set; }
        public string Color { get; set; }
        public string Make { get; set; }
        public string PetName { get; set; }
    }
}
