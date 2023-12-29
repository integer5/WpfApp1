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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"D:\\04 Práca doma\\CODIUM\\WPFApp\\WpfApp1\\WpfApp1\\Database1.mdf\";Integrated Security=True";
        SqlConnection connection;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InsertEvent();
        }

        void InsertEvent()
        {
            // ProviderEventID ; EventName ; EventDate
            // 120324104 ; Team B vs. Team D ; 2022-10-19T02:30:00
            string ProviderEventID = "120324104";
            string EventName = "Team B vs. Team G";
            string EventDate = "2022-10-19T02:30:00";

            //string query = 
            //    "INSERT INTO Events VALUES (@ProviderEventID, @EventName, @EventDate)" +
            //    "ON DUPLICATE KEY UPDATE @ProviderEventID";

            string query =
                "IF EXISTS (SELECT ProviderEventID FROM Events WHERE ProviderEventID = @ProviderEventID) " +
                "UPDATE Events SET EventName = @EventName, EventDate = @EventDate WHERE ProviderEventID = @ProviderEventID " +
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
            }
        }

        void InsertMessage()
        {
            // MessageID ; GeneratedDate ; ProviderEventID
            // b72340cf-edb1-47ee-b479-6558fbbf7455 ; 2022-10-10T12:44:44.9314727+02:00 ; 120324104
            string MessageID = "b72340cf-edb1-47ee-b479-6558fbbf7455";
            string GeneratedDate = "2022-10-10T12:44:44.9314727+02:00";
            string ProviderEventID = "120324104";

            string query = "INSERT INTO Messages VALUES (@MessageID, @GeneratedDate, @ProviderEventID)";
            
            //string query = "INSERT INTO TestTable VALUES (@Cislo, @Cislo2)";

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

    }
}