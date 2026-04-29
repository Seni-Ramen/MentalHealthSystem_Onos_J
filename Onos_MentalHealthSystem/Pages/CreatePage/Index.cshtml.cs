using MentalHealthSystem_Onos_J.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace MentalHealthSystem_Onos_J.CreatePage
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public List<Client> ClientList { get; set; } = new();
        public List<Counselor> CounselorList { get; set; } = new();
        public List<Session> SessionList { get; set; } = new();

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            LoadAllData();
        }

        public IActionResult OnPost(
            string TargetTable,
            string FirstName, string MiddleName, string LastName,
            string ContactInfo, string Specialty,
            int? ClientID, int? CounselorID, DateTime? SessionDate)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "";

                if (TargetTable == "Client")
                    sql = "INSERT INTO Client (FirstName, MiddleName, LastName, ContactInfo) VALUES (@FirstName, @MiddleName, @LastName, @ContactInfo)";
                else if (TargetTable == "Counselor")
                    sql = "INSERT INTO Counselor (FirstName, MiddleName, LastName, Specialty) VALUES (@FirstName, @MiddleName, @LastName, @Specialty)";
                else if (TargetTable == "Session")
                    sql = "INSERT INTO Session (SessionDate, ClientID, CounselorID) VALUES (@SessionDate, @ClientID, @CounselorID)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (TargetTable == "Client")
                    {
                        command.Parameters.AddWithValue("@FirstName", FirstName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MiddleName", string.IsNullOrEmpty(MiddleName) ? (object)DBNull.Value : MiddleName);
                        command.Parameters.AddWithValue("@LastName", LastName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ContactInfo", ContactInfo ?? (object)DBNull.Value);
                    }
                    else if (TargetTable == "Counselor")
                    {
                        command.Parameters.AddWithValue("@FirstName", FirstName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MiddleName", string.IsNullOrEmpty(MiddleName) ? (object)DBNull.Value : MiddleName);
                        command.Parameters.AddWithValue("@LastName", LastName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Specialty", Specialty ?? (object)DBNull.Value);
                    }
                    else if (TargetTable == "Session")
                    {
                        if (SessionDate == null || SessionDate <= DateTime.Now)
                        {
                            ModelState.AddModelError("SessionDate", "Session date must be in the future.");
                            LoadAllData();
                            return Page();
                        }

                        command.Parameters.AddWithValue("@SessionDate", SessionDate);
                        command.Parameters.AddWithValue("@ClientID", ClientID ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CounselorID", CounselorID ?? (object)DBNull.Value);
                    }

                    command.ExecuteNonQuery();
                }
            }

            LoadAllData();
            return Page();
        }

        private void LoadAllData()
        {
            ClientList.Clear();
            CounselorList.Clear();
            SessionList.Clear();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string clientSql = "SELECT ClientID, FirstName, MiddleName, LastName, ContactInfo FROM Client " +
                    "WHERE (@SearchTerm is NULL OR FirstName LIKE '%' + @SearchTerm + '%' OR LastName LIKE '%' + @SearchTerm + '%'" +
                    "OR ContactInfo LIKE '%' + @SearchTerm + '%')";
                using (SqlCommand cmd = new SqlCommand(clientSql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", (object)SearchTerm ?? DBNull.Value);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ClientList.Add(new Client
                            {
                                ClientID = Convert.ToInt32(reader["ClientID"]),
                                FirstName = reader["FirstName"].ToString(),
                                MiddleName = reader["MiddleName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                ContactInfo = reader["ContactInfo"].ToString()
                            });
                        }
                    }
                }
            
                string counselorSql = "SELECT CounselorID, FirstName, MiddleName, LastName, Specialty FROM Counselor " +
                    "WHERE (@SearchTerm is NULL OR FirstName LIKE '%' + @SearchTerm + '%' OR LastName LIKE '%' + @SearchTerm + '%'" +
                    "OR Specialty LIKE '%' + @SearchTerm + '%')";
                using (SqlCommand cmd = new SqlCommand(counselorSql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", (object)SearchTerm ?? DBNull.Value);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CounselorList.Add(new Counselor
                            {
                                CounselorID = Convert.ToInt32(reader["CounselorID"]),
                                FirstName = reader["FirstName"].ToString(),
                                MiddleName = reader["MiddleName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Specialty = reader["Specialty"].ToString()
                            });
                        }
                    }
                }
                

                string sessionSql = @"SELECT * FROM (SELECT s.SessionID, s.SessionDate, 
                                        c.FirstName + ' ' + c.LastName AS ClientFullName,
                                        co.FirstName + ' ' + co.LastName AS CounselorFullName
                                      FROM Session s
                                      JOIN Client c ON s.ClientID = c.ClientID
                                      JOIN Counselor co ON s.CounselorID = co.CounselorID) AS SessionView
                                      WHERE (@SearchTerm is NULL OR ClientFullName LIKE '%' + @SearchTerm + '%'
                                      OR CounselorFullName LIKE '%' + @SearchTerm + '%') ORDER BY SessionDate DESC";
                using (SqlCommand cmd = new SqlCommand(sessionSql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", (object)SearchTerm ?? DBNull.Value);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SessionList.Add(new Session
                            {
                                SessionID = Convert.ToInt32(reader["SessionID"]),
                                SessionDate = Convert.ToDateTime(reader["SessionDate"]),
                                ClientFullName = reader["ClientFullName"].ToString(),
                                CounselorFullName = reader["CounselorFullName"].ToString()
                            });
                        }
                    }
                }
                
            }
        }
    }
}