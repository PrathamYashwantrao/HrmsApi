namespace HrmsApi.Models
{
    public class Commitment
    {
        public int CommitmentId { get; set; }
        public string TraineeName { get; set; }
        public string TraineeBatch { get; set; }
        public string TaskDescription { get; set; }
        public string FilePath { get; set; }
        public DateTime? SubmittedAt { get; set; } = null;

    }
}