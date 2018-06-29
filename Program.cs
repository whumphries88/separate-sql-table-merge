using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

namespace sql_table_merge
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> ReportColumns = new List<string>();
            string repColsString = "[";
            string colQuery = @"SELECT * from [dbo].aaa";


            string sqlXMConn = @"Data Source=aaa;Initial Catalog=aa;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            string sqlBLconn = @"Data Source=bbb;Initial Catalog=bb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            SqlConnection conn = new SqlConnection(sqlXMConn);
            conn.Open();

            //string getCOLS = "SELECT * FROM [dbo].[aaa04_File3_v2]"

            SqlCommand cmd = new SqlCommand(colQuery, conn);
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read();
                var columns = new List<string>();

                for(int i = 0; i < reader.FieldCount; i++)
                {
                    string colName = reader.GetName(i);
                    columns.Add(reader.GetName(i));
                    repColsString += colName;
                }



                repColsString = String.Join("],[", columns);
                repColsString = "[" + repColsString + "]";
                Console.WriteLine(repColsString);
            }


            //SqlCommand cmd = new SqlCommand(colQuery, conn);
            //using(var reader = cmd.ExecuteReader())
            //{
            //    reader.Read();
            //    var table = reader.GetSchemaTable();
            //    foreach (DataColumn dc in table.Columns)
            //    {
            //        Console.WriteLine(dc.ColumnName);
            //        ReportColumns.Add(dc.ColumnName);
            //        repColsString += dc.ColumnName + "],[";
            //    }

            //    //repColsString = String.Join("],[", ReportColumns);
            //    //repColsString = "[" + ReportColumns + "]";
            //    repColsString = repColsString.Substring(0, repColsString.Length - 2);
            //    Console.WriteLine(repColsString);
            //}

            var insertReport = "INSERT INTO [bbb].[dbo].[bb-04_File3_v2](" + repColsString + ") " +
                                   "SELECT " + repColsString + " from [bbb].[dbo].[bb-04]";
            SqlConnection conn2 = new SqlConnection(sqlBLconn);

            //using(var cmd2 = new SqlCommand(insertReport, conn2))
            //{
            //    conn2.Open();
            //    //cmd2.Parameters.AddWithValue("@placeholder", repColsString);
            //    //cmd2.Parameters.AddWithValue("@placeholder2", repColsString);
            //    cmd2.ExecuteNonQuery();
            //}


            //UNCOMMENT THIS TO BUILD DB
            Console.Write("Enter the Database Name: ");
            string name = Console.ReadLine();
            string buildDB = "CREATE DATABASE " + name + " CONTAINMENT = NONE ON PRIMARY (NAME = N'" + name + @"', FILENAME = N'D:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\"
                + name + ".mdf', SIZE = 39936KB , FILEGROWTH = 10%)";
            SqlCommand cmd = new SqlCommand(buildDB, conn);
            cmd.ExecuteNonQuery();
            Console.WriteLine("DB " + name + " created.");

            List<string> tables = new List<string>();
            DataTable t = conn.GetSchema("Tables");

            foreach (DataRow row in t.Rows)
            {
                string tablename = (string)row[2];
                tables.Add(tablename);
            }

            

            foreach (string table in tables)
            {
                Console.WriteLine(table);
            }

            Console.WriteLine("\n" + "Enter Base Report Name: ");
            string baseReport = Console.ReadLine();


            foreach (string table in tables)
            {
                if(table == baseReport){
                    //Console.WriteLine("Table Found");
                }
            }

            string BaseReportTableCreationPath = @"\Financial Reporting\sql-table-merge\SQL\BaseReportTableCreationPath.sql";
            string BaseReportTableCreationScript = File.ReadAllText(BaseReportTableCreationPath);

            Server server = new Server(new ServerConnection(conn));
            server.ConnectionContext.ExecuteReader(colQuery);

            server.ConnectionContext.ExecuteNonQuery(BaseReportTableCreationScript);

            Console.ReadLine();

            conn.Close();
        }
    }
}
