namespace HrmsApi.Models
{
    public class SubmitTask
    {
    }

    public class StudentTask
    {
        public int Id { get; set; }
        public string Batch { get; set; }
        public string Description { get; set; }
        public string AttachmentPath { get; set; }
    }


    public class SubmittedTask
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string Batch { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string StudentName { get; set; }
        public int? Score { get; set; }
        public string Status { get; set; } = "Pending";
        public string CommitmentStatus { get; set; } = "";
        public string LateReason { get; set; } = "";
    }

    public class StudentTasksModel
    {
        public int Id { get; set; }
        public string Batch { get; set; }
        public string Description { get; set; }
        public string AttachmentPath { get; set; }
        public bool IsSubmitted { get; set; }
    }

    public class TaskSubmissionDto
    {
        public int TaskId { get; set; }
        public IFormFile File { get; set; }

        public string LateReason { get; set; }
        public string CommitmentStatus { get; set; }
    }
    public class TopPerformerDto
    {
        public string Name { get; set; }
        public string Batch { get; set; }
        public int? TotalScore { get; set; }
    }

}
