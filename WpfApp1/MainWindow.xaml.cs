using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Text.Json;
using System.IO;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        string jsonURI = "C:\\Users\\roman.luciak\\Downloads\\zdrojovy_dokument\\zdrojovy_dokument.json";
        string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"D:\\04 Práca doma\\CODIUM\\WPFApp\\WpfApp1\\WpfApp1\\Database1.mdf\";Integrated Security=True";
        SqlConnection connection;
        List<TiposMessage> tiposMessages;
        
        public MainWindow()
        {

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InsertEvent();
            InsertMessage();
            InsertOdd();
            //clearAllTables();
        }

        void InsertOdd()
        {
            // ProviderEventID ; ProviderOddsID ; OddsName ; OddsRate ; Status
            // 1499420517 ; 764331995 ; Home ; 1.981 ; suspended
            int ProviderEventID = 555555;
            int ProviderOddsID = 111;
            string OddsName = "Home";
            float OddsRate = 1.5f;
            string Status = "suspended";

            string query =
                "IF EXISTS (SELECT ProviderEventID, ProviderOddsID FROM Odds WHERE ProviderEventID = @ProviderEventID AND ProviderOddsID = @ProviderOddsID) " +
                "UPDATE Odds SET OddsRate = @OddsRate, Status = @Status WHERE ProviderEventID = @ProviderEventID AND ProviderOddsID = @ProviderOddsID " +
                "ELSE " +
                "INSERT INTO Odds VALUES (@ProviderEventID, @ProviderOddsID, @OddsName, @OddsRate, @Status)";

            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProviderEventID", ProviderEventID);
                    command.Parameters.AddWithValue("@ProviderOddsID", ProviderOddsID);
                    command.Parameters.AddWithValue("@OddsName", OddsName);
                    command.Parameters.AddWithValue("@OddsRate", OddsRate);
                    command.Parameters.AddWithValue("@Status", Status);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        void InsertEvent()
        {
            // ProviderEventID ; EventName ; EventDate
            // 120324104 ; Team B vs. Team D ; 2022-10-19T02:30:00
            int ProviderEventID = 120324104;
            string EventName = "Team A vs. Team A";
            string EventDate = DateTime.Now.ToString("s");//"2023-10-19T02:30:00";

            string query =
                "IF EXISTS (SELECT ProviderEventID FROM Events WHERE ProviderEventID = @ProviderEventID) " +
                "UPDATE Events SET EventDate = @EventDate WHERE ProviderEventID = @ProviderEventID " +
                "ELSE " +
                "INSERT INTO Events VALUES (@ProviderEventID, @EventName, @EventDate)";

            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProviderEventID", ProviderEventID);
                    command.Parameters.AddWithValue("@EventName", EventName);
                    command.Parameters.AddWithValue("@EventDate", EventDate);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        void InsertMessage()
        {
            // MessageID ; GeneratedDate ; ProviderEventID
            // b72340cf-edb1-47ee-b479-6558fbbf7455 ; 2022-10-10T12:44:44.9314727+02:00 ; 120324104
            string MessageID = "b72340cf-edb1-47ee-b479-6558fbbf7455";
            string GeneratedDate = DateTimeOffset.Now.ToString("O");//"2023-10-10T12:44:44.9314727+02:00";
            int ProviderEventID = 120324104;

            //string query = "INSERT INTO Messages VALUES (@MessageID, @GeneratedDate, @ProviderEventID)";

            string query =
                "IF EXISTS (SELECT ProviderEventID FROM Messages WHERE ProviderEventID = @ProviderEventID) " +
                "UPDATE Messages SET MessageID = @MessageID, GeneratedDate = @GeneratedDate WHERE ProviderEventID = @ProviderEventID " +
                "ELSE " +
                "INSERT INTO Messages VALUES (@MessageID, @GeneratedDate, @ProviderEventID)";

            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MessageID", MessageID);
                    command.Parameters.AddWithValue("@GeneratedDate", GeneratedDate);
                    command.Parameters.AddWithValue("@ProviderEventID", ProviderEventID);
                    command.ExecuteNonQuery();
                }
            }
        }

        void readTiposFile()
        {
            FileStream stream = File.OpenRead(jsonURI);

            tiposMessages = new List<TiposMessage>();
            tiposMessages = JsonSerializer.Deserialize<List<TiposMessage>>(stream);

            stream.Close();
        }

        void clearAllTables()
        {
            using (connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "TRUNCATE TABLE Events";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }

                query = "TRUNCATE TABLE Messages";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }

                query = "TRUNCATE TABLE Odds";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }

                query = "TRUNCATE TABLE TestTable";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

    }
}