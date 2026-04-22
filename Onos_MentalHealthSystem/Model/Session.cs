namespace MentalHealthSystem_Onos_J.Model
{
    public class Session
    {
        public int SessionID { get; set; }
        public DateTime SessionDate { get; set; }
        public int CounselorID { get; set; }
        public int ClientID { get; set; }
        public string ClientFullName { get; set; }
        public string CounselorFullName { get; set; }
        public Client client { get; set; }
        public Counselor counselor { get; set; }
    }
}
