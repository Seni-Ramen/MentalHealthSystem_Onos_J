using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MentalHealthSystem_Onos_J.Model;

namespace MentalHealthSystem_Onos_J.Pages.DeletePage
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty(SupportsGet = true)]
        public string Target { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string DisplayName { get; set; }
        public bool HasSession { get; set; }

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");

            using SqlConnection conn = new(connString);
            conn.Open();

            HasSession = false;

            if (Target == "Client")
            {
                string checkSql = "SELECT COUNT(*) FROM Session WHERE ClientID=@Id";
                using SqlCommand checkCmd = new(checkSql, conn);
                checkCmd.Parameters.AddWithValue("@Id", Id);

                int count = (int)checkCmd.ExecuteScalar();
                HasSession = count > 0;
            }
            else if (Target == "Counselor")
            {
                string checkSql = "SELECT COUNT(*) FROM Session WHERE CounselorID=@Id";
                using SqlCommand checkCmd = new(checkSql, conn);
                checkCmd.Parameters.AddWithValue("@Id", Id);

                int count = (int)checkCmd.ExecuteScalar();
                HasSession = count > 0;
            }

            string sql = Target switch
            {
                "Client" => "SELECT FirstName + ' ' + LastName FROM Client WHERE ClientID=@Id",
                "Counselor" => "SELECT FirstName + ' ' + LastName FROM Counselor WHERE CounselorID=@Id",
                "Session" => "SELECT 'Session on ' + CONVERT(varchar, SessionDate, 100) FROM Session WHERE SessionID=@Id",
                _ => ""
            };

            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@Id", Id);

            var result = cmd.ExecuteScalar();
            DisplayName = result?.ToString() ?? "Record";
        }

        public IActionResult OnPost()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");

            using SqlConnection conn = new(connString);
            conn.Open();

            string sql = Target switch
            {
                "Client" => "DELETE FROM Client WHERE ClientID=@Id",
                "Counselor" => "DELETE FROM Counselor WHERE CounselorID=@Id",
                "Session" => "DELETE FROM Session WHERE SessionID=@Id",
                _ => ""
            };

            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@Id", Id);

            cmd.ExecuteNonQuery();

            return RedirectToPage("/ReadPage/Index");
        }
    }
}
