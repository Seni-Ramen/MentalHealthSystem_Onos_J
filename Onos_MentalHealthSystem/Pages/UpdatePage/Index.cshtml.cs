using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MentalHealthSystem_Onos_J.Model;
using Microsoft.Data.SqlClient;

namespace MentalHealthSystem_Onos_J.Pages.UpdatePage
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty(SupportsGet = true)]
        public string Target { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty] public Client CurrentClient { get; set; } = new();
        [BindProperty] public Counselor CurrentCounselor { get; set; } = new();
        [BindProperty] public Session CurrentSession { get; set; } = new();

        public List<Client> ClientOpts { get; set; } = new();
        public List<Counselor> CounselorOpts { get; set; } = new();

        public IndexModel(IConfiguration configuration) => _configuration = configuration;

        public void OnGet()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection conn = new(connString);
            conn.Open();

            if (Target == "Client")
            {
                LoadClient(conn);
            }
            else if (Target == "Counselor")
            {
                LoadCounselor(conn);
            }
            else if (Target == "Session") 
            { 
                LoadSession(conn); LoadLists(conn); 
            }
        }

        private void LoadClient(SqlConnection conn)
        {
            string sql = "SELECT * FROM Client WHERE ClientID = @Id";
            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@Id", Id);
            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                CurrentClient.ClientID = (int)reader["ClientID"];
                CurrentClient.FirstName = reader["FirstName"].ToString();
                CurrentClient.LastName = reader["LastName"].ToString();
                CurrentClient.ContactInfo = reader["ContactInfo"]?.ToString();
            }
        }

        private void LoadCounselor(SqlConnection conn)
        {
            string sql = "SELECT * FROM Counselor WHERE CounselorID = @Id";
            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@Id", Id);
            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                CurrentCounselor.CounselorID = (int)reader["CounselorID"];
                CurrentCounselor.FirstName = reader["FirstName"].ToString();
                CurrentCounselor.LastName = reader["LastName"].ToString();
                CurrentCounselor.Specialty = reader["Specialty"]?.ToString();
            }
        }

        private void LoadSession(SqlConnection conn)
        {
            string sql = "SELECT * FROM Session WHERE SessionID = @Id";
            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@Id", Id);
            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                CurrentSession.SessionID = (int)reader["SessionID"];
                CurrentSession.SessionDate = (DateTime)reader["SessionDate"];
                CurrentSession.ClientID = (int)reader["ClientID"];
                CurrentSession.CounselorID = (int)reader["CounselorID"];
            }
        }

        private void LoadLists(SqlConnection conn)
        {
            string cSql = "SELECT ClientID, FirstName + ' ' + LastName AS Name FROM Client";
            using (SqlCommand cmd = new(cSql, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
                while (reader.Read()) ClientOpts.Add(new Client { 
                    ClientID = (int)reader["ClientID"], FirstName = reader["Name"].ToString() 
                });

            string coSql = "SELECT CounselorID, FirstName + ' ' + LastName AS Name FROM Counselor";
            using (SqlCommand cmd = new(coSql, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
                while (reader.Read()) CounselorOpts.Add(new Counselor { CounselorID = (int)reader["CounselorID"], FirstName = reader["Name"].ToString() });
        }

        public IActionResult OnPost()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection conn = new(connString);
            conn.Open();
            string sql = Target switch
            {
                "Client" => "UPDATE Client SET FirstName=@FirstName, LastName=@LastName, ContactInfo=@ContactInfo WHERE ClientID=@ClientID",
                "Session" => "UPDATE Session SET SessionDate=@SessinDate, ClientID=@ClientID, CounselorID=@CounselorID WHERE SessionID=@SessionID",
                _ => ""
            };

            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@FN", CurrentClient.FirstName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LN", CurrentClient.LastName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CI", CurrentClient.ContactInfo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SD", CurrentSession.SessionDate);
            cmd.Parameters.AddWithValue("@CID", CurrentSession.ClientID);
            cmd.Parameters.AddWithValue("@COID", CurrentSession.CounselorID);

            cmd.ExecuteNonQuery();
            return RedirectToPage("/ReadPage");
        }
    }
}