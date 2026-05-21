using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Windows.Forms;

public class DatabaseConnection
{
    // Updated with your database name: fastfooddb
    private static string connString = "server=localhost;database=fastfooddb;uid=root;pwd=root2456;";

    public static MySqlConnection GetConnection()
    {
        MySqlConnection conn = new MySqlConnection(connString);
        return conn;
    }
}