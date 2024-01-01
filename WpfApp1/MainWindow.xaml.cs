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
using System.Diagnostics;
using System.ComponentModel;

namespace WpfApp1
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        string jsonURI = "C:\\Users\\roman.luciak\\Downloads\\zdrojovy_dokument\\zdrojovy_dokument.json";
        string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"D:\\04 Práca doma\\CODIUM\\WPFApp\\WpfApp1\\WpfApp1\\Database1.mdf\";Integrated Security=True";
        //SqlConnection Connection;
        Queue<TiposMessage> TiposMessages;
        float StartCount;
        Stopwatch StopwatchPocess;

        public MainWindow()
        {

            this.DataContext = this;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            clearAllTables();

            readTiposFile();
            // Duration before {00:00:26.4640061}

            //foreach (TiposMessage tiposMessage in TiposMessages)
            //{
            //    InsertTiposMessage(tiposMessage);
            //}

            StopwatchPocess = new Stopwatch();
            Task.Run(() => durationTimer());
            Task.Run(() => ParallelProcess());


        }

        void ParallelProcess()
        {
            StopwatchPocess.Start();

            object queueLock = new object();

            int threadCount = 4;

            Thread[] threads = new Thread[threadCount];


            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    while (true)
                    {
                        TiposMessage tiposMessage;
                        lock (queueLock)
                        {
                            if (TiposMessages.Count == 0)
                            {
                                break;
                            }

                            tiposMessage = TiposMessages.Dequeue();
                            InsertTiposMessage(tiposMessage);
                        }
                    }
                });

                threads[i].Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            StopwatchPocess.Stop();

        }

        void InsertTiposMessage(TiposMessage tiposMessage)
        {
            SqlConnection connection;
            using (connection = new SqlConnection(connectionString))
            {
                //Connection.Open();
                if (connection.State == ConnectionState.Closed) 
                {
                    connection.Open();
                }

                InsertMessage(connection, tiposMessage.MessageID, tiposMessage.GeneratedDate, tiposMessage.Event.ProviderEventID);
                InsertEvent(connection, tiposMessage.Event.ProviderEventID, tiposMessage.Event.EventName, tiposMessage.Event.EventDate.ToString("s"));

                foreach (var odd in tiposMessage.Event.OddsList)
                {
                    InsertOdd(connection,tiposMessage.Event.ProviderEventID, odd.ProviderOddsID, odd.OddsName, odd.OddsRate, odd.Status);
                }

                connection.Close();
            }

            ProgressValue = (StartCount - TiposMessages.Count) / StartCount * 100;
        }

        void InsertOdd(SqlConnection connection, int ProviderEventID, int ProviderOddsID, string OddsName, float OddsRate, string Status)
        {
            // ProviderEventID ; ProviderOddsID ; OddsName ; OddsRate ; Status
            // 1499420517 ; 764331995 ; Home ; 1.981 ; suspended
            //int ProviderEventID = 555555;
            //int ProviderOddsID = 111;
            //string OddsName = "Home";
            //float OddsRate = 1.5f;
            //string Status = "suspended";

            string query =
                "IF EXISTS (SELECT ProviderEventID, ProviderOddsID FROM Odds WHERE ProviderEventID = @ProviderEventID AND ProviderOddsID = @ProviderOddsID) " +
                "UPDATE Odds SET OddsRate = @OddsRate, Status = @Status WHERE ProviderEventID = @ProviderEventID AND ProviderOddsID = @ProviderOddsID " +
                "ELSE " +
                "INSERT INTO Odds VALUES (@ProviderEventID, @ProviderOddsID, @OddsName, @OddsRate, @Status)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProviderEventID", ProviderEventID);
                command.Parameters.AddWithValue("@ProviderOddsID", ProviderOddsID);
                command.Parameters.AddWithValue("@OddsName", OddsName);
                command.Parameters.AddWithValue("@OddsRate", OddsRate);
                command.Parameters.AddWithValue("@Status", Status);
                command.ExecuteNonQuery();
            }
        }

        void InsertEvent(SqlConnection connection, int ProviderEventID, string EventName, string EventDate)
        {
            // ProviderEventID ; EventName ; EventDate
            // 120324104 ; Team B vs. Team D ; 2022-10-19T02:30:00
            //int ProviderEventID = 120324104;
            //string EventName = "Team A vs. Team A";
            //string EventDate = DateTime.Now.ToString("s");//"2023-10-19T02:30:00";

            string query =
                "IF EXISTS (SELECT ProviderEventID FROM Events WHERE ProviderEventID = @ProviderEventID) " +
                "UPDATE Events SET EventDate = @EventDate WHERE ProviderEventID = @ProviderEventID " +
                "ELSE " +
                "INSERT INTO Events VALUES (@ProviderEventID, @EventName, @EventDate)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProviderEventID", ProviderEventID);
                command.Parameters.AddWithValue("@EventName", EventName);
                command.Parameters.AddWithValue("@EventDate", EventDate);
                command.ExecuteNonQuery();
            }
        }

        void InsertMessage(SqlConnection connection, string MessageID, string GeneratedDate, int ProviderEventID)
        {
            // MessageID ; GeneratedDate ; ProviderEventID
            // b72340cf-edb1-47ee-b479-6558fbbf7455 ; 2022-10-10T12:44:44.9314727+02:00 ; 120324104
            //string MessageID = "b72340cf-edb1-47ee-b479-6558fbbf7455";
            //string GeneratedDate = DateTimeOffset.Now.ToString("O");//"2023-10-10T12:44:44.9314727+02:00";
            //int ProviderEventID = 120324104;

            //string query = "INSERT INTO Messages VALUES (@MessageID, @GeneratedDate, @ProviderEventID)";

            string query =
                "INSERT INTO Messages VALUES (@MessageID, @GeneratedDate, @ProviderEventID)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@MessageID", MessageID);
                command.Parameters.AddWithValue("@GeneratedDate", GeneratedDate);
                command.Parameters.AddWithValue("@ProviderEventID", ProviderEventID);
                command.ExecuteNonQuery();
            }
        }

        void readTiposFile()
        {
            FileStream stream = File.OpenRead(jsonURI);

            List<TiposMessage> _TiposMessages = new List<TiposMessage>();
            _TiposMessages = JsonSerializer.Deserialize<List<TiposMessage>>(stream);

            stream.Close();

            TiposMessages = new Queue<TiposMessage>(_TiposMessages);
            StartCount = TiposMessages.Count;
        }

        void clearAllTables()
        {
            SqlConnection connection;
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

                connection.Close();
            }
        }


        async void durationTimer()
        {
            while(TiposMessages.Count != 0)
            {
                DurationValue = StopwatchPocess.ToString();
                await Task.Delay(25);
            }
        }

        private float progressValue;
        public float ProgressValue
        {
            get { return progressValue; }
            set
            {
                progressValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressValue"));
            }
        }

        private string durationValue;
        public string DurationValue
        {
            get { return durationValue; }
            set
            {
                durationValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DurationValue"));
            }
        }
    }
}