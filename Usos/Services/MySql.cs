using MySql.Data.MySqlClient;

namespace Usos.Services
{
    internal class Conexion
    {
        public static MySqlConnection CreateConexion()
        {
            string servidor = "localhost";
            string bd = "test";
            string user = "root";
            string passwd = "";

            string connectionString =
                $"Server={servidor};Database={bd};User={user};Password={passwd};";

            return new MySqlConnection(connectionString);
        }

        public static void MySqlOperation(MySqlConnection conexion, string sqlQuery) //fabryka polaczen/ operacji
        {
            conexion.Open();
            using MySqlCommand cmd = new MySqlCommand(sqlQuery, conexion);
            int rows = cmd.ExecuteNonQuery(); //to musi zostac dostowane bo z tego pomoca nie zrobimy selecta
        }

    }
}

/*
 * Musimy stowrzyc jakas inicjalizacje bazy danych - idk czy to w sumie potrzebne
 * Takie rzeczy jak update insert i delete beda wykonywane za pomoca odpowiednich serwisow i wywolan 
 * 
 * 
 */

