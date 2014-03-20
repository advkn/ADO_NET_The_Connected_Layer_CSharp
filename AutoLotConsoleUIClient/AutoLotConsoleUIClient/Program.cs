using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoLotConnectedLayer;
using System.Configuration;
using System.Data;


/*Logic for the Console based UI that utilizes the AutoLotConnectedLayer.dll library
 for accesing and performing operations on the AutoLot database.*/
namespace AutoLotConsoleUIClient
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("***** The AutoLot Console UI *****\n");

            //Get connection string from the App.config file
            string connStr = ConfigurationManager.ConnectionStrings["AutoLotSqlProvider"].ConnectionString;

            //flag for the quit option in the user interface menu
            bool userDone = false;

            //intial user command on startup
            string userCommand = "";

            //Create the InventoryDAL object
            InventoryDAL invDAL = new InventoryDAL();
            invDAL.OpenConnection(connStr);

            //keep asking for input until user presses the Q key.
            try
            {
                ShowInstructions();
                do
                {
                    Console.Write("\nPlease enter your command: ");
                    userCommand = Console.ReadLine();
                    Console.WriteLine();
                    switch (userCommand.ToUpper())
                    {
                        case "I":
                            InsertNewCar(invDAL);
                            break;
                        case "U":
                            UpdateCarPetName(invDAL);
                            break;
                        case "D":
                            DeleteCar(invDAL);
                            break;
                        case "L":
                            ListInventory(invDAL);
                            break;
                        case "S":
                            ShowInstructions();
                            break;
                        case "P":
                            LookUpPetName(invDAL);
                            break;
                        case "R":
                            ProcessCustomerCreditRisk(invDAL);
                            break;
                        case "Q":
                            userDone = true;
                            break;
                    }

                } while (!userDone);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                invDAL.CloseConnection();
            }
        }

        //Prints the console UI option menu.
        private static void ShowInstructions()
        {
            Console.WriteLine("I: Inserts a new car.");
            Console.WriteLine("U: Updates an existing car.");
            Console.WriteLine("D: Deletes an existing car.");
            Console.WriteLine("L: Lists the current inventory.");
            Console.WriteLine("S: Shows these instructions.");
            Console.WriteLine("P: Looks up pet name.");
            Console.WriteLine("R: Process cutomer credit risk.");
            Console.WriteLine("Q: Quits program.");
        }

        //List the entire inventory
        private static void ListInventory(InventoryDAL invDAL)
        {
            //Get the list of the linventory
            DataTable dt = invDAL.GetAllInventoryAsDataTable();

            //pass DataTable to helper function to display
            DisplayTable(dt);
        }

        private static void DisplayTable(DataTable dt)
        {
            //Print out the column names
            for (int curCol = 0; curCol < dt.Columns.Count; curCol++)
            {
                Console.Write(dt.Columns[curCol].ColumnName + "\t");
            }
            Console.WriteLine("\n---------------------------------------");

            //print the DataTable
            //first vist each row
            for(int curRow = 0; curRow < dt.Rows.Count; curRow++) 
            {
                //then print the data in each column of each row
                for (int curCol = 0; curCol < dt.Columns.Count; curCol++)
                {
                    Console.Write(dt.Rows[curRow][curCol].ToString() +"\t");
                }
                Console.WriteLine();
            }
        }


        //Delete a car from the database
        private static void DeleteCar(InventoryDAL invDAL)
        {
            //Get the id of car to delete
            Console.Write("Enter ID of car to delete: ");
            int id = int.Parse(Console.ReadLine());

            //use a try/catch in case you have a referential integrity violation
            try
            {
                invDAL.DeleteCar(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //Database transaction test
        private static void ProcessCustomerCreditRisk(InventoryDAL invDAL)
        {
            Console.WriteLine("***** A Simple Transaction Example *****\n");

            //Variables that decide whether the transaction commits or is rolled back.
            bool throwEx = true;
            string userAnswer = string.Empty;
            int customerID;

            Console.WriteLine("Please enter the customer id you want to process: ");
            customerID = int.Parse(Console.ReadLine());
            Console.WriteLine("Do you want to throw an exception: (Y or N): ");
            userAnswer = Console.ReadLine();
            if (userAnswer.ToLower() == "n")
            {
                throwEx = false;
            }

            //process the customer
            invDAL.ProcessCreditRisks(throwEx, customerID);

            Console.WriteLine("Check CreditRisks table for results");
            Console.ReadLine();
        }


        //Insert a new car into the database
        private static void InsertNewCar(InventoryDAL invDAL)
        {
            //First get the user data.
            int newCarID;
            string newCarColor;
            string newCarMake;
            string newCarPetName;

            Console.Write("Enter Car ID: ");
            newCarID = int.Parse(Console.ReadLine());

            Console.Write("Enter Car Color: ");
            newCarColor = Console.ReadLine();

            Console.Write("Enter Car Make: ");
            newCarMake = Console.ReadLine();

            Console.Write("Enter Pet Name: ");
            newCarPetName = Console.ReadLine();

            //now pass to the data access library
            invDAL.InsertAuto(newCarID, newCarColor, newCarMake, newCarPetName);
        }

        //Update an existing car's Pet Name
        private static void UpdateCarPetName(InventoryDAL invDAL)
        {
            //First get user data
            int carID;
            string newCarPetName;

            Console.Write("Enter Car ID: ");
            carID = int.Parse(Console.ReadLine());

            Console.Write("Enter new Pet Name: ");
            newCarPetName = Console.ReadLine();

            //now pass to the data access library to operate on
            invDAL.UpdateCarPetName(carID, newCarPetName);
        }


        //Query a car's Pet Name by the car id.
        private static void LookUpPetName(InventoryDAL invDAL)
        {
            //Get the id of the car you want to look up
            Console.Write("Enter id of the car to look up: ");
            int id = int.Parse(Console.ReadLine());
            Console.WriteLine("Pet Name of {0} is {1}", id, invDAL.LookUpPetName(id));
        }

    }
}
