using MentalHealthSystem_Onos_J.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace MentalHealthSystem_Onos_J.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
        public List<Client>    ClientList    { get; set; } = new();
        public List<Counselor> CounselorList { get; set; } = new();
        public List<Session>   SessionList   { get; set; } = new();

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string clientSql = @"
                    SELECT ClientID, FirstName, MiddleName, LastName, ContactInfo 
                    FROM Client
                    WHERE (@SearchTerm IS NULL OR FirstName LIKE '%' + @SearchTerm + '%'
                       OR LastName LIKE '%' + @SearchTerm + '%'
                       OR ContactInfo LIKE '%' + @SearchTerm + '%')";

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
                                MiddleName = reader["MiddleName"] == DBNull.Value ? "N/A" : reader["MiddleName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                ContactInfo = reader["ContactInfo"] == DBNull.Value ? "N/A" : reader["ContactInfo"].ToString()
                            });
                        }
                    }
                }
                

                string counselorSql = @"
                    SELECT CounselorID, FirstName, MiddleName, LastName, Specialty
                    FROM Counselor
                    WHERE (@SearchTerm IS NULL OR FirstName LIKE '%' + @SearchTerm + '%'
                       OR LastName LIKE '%' + @SearchTerm + '%'
                       OR Specialty LIKE '%' + @SearchTerm + '%')"; ;
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
                                MiddleName = reader["MiddleName"] == DBNull.Value ? "N/A" : reader["MiddleName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Specialty = reader["Specialty"] == DBNull.Value ? "N/A" : reader["Specialty"].ToString()
                            });
                        }
                    }
                }
                

                string sessionSql = @"
                    SELECT * FROM (
                        SELECT
                            s.SessionID,
                            s.SessionDate,
                            s.ClientID,
                            s.CounselorID,
                            cl.FirstName + ' ' + cl.LastName AS ClientFullName,
                            co.FirstName + ' ' + co.LastName AS CounselorFullName
                        FROM Session s
                        INNER JOIN Client cl ON s.ClientID = cl.ClientID
                        INNER JOIN Counselor co ON s.CounselorID = co.CounselorID
                    ) AS SessionView
                    WHERE (@SearchTerm IS NULL 
                       OR ClientFullName LIKE '%' + @SearchTerm + '%'
                       OR CounselorFullName LIKE '%' + @SearchTerm + '%')
                    ORDER BY SessionDate DESC";

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
                                ClientID = Convert.ToInt32(reader["ClientID"]),
                                CounselorID = Convert.ToInt32(reader["CounselorID"]),
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
