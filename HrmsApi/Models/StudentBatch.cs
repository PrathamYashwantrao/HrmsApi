using System.Text.Json.Serialization;


namespace HrmsApi.Models
{
    public class StudentBatch
    {
        public int ID { get; set; }
        public string Batch { get; set; }
        public string Description { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public string AttachmentPath { get; set; }
        public DateTime CDate { get; set; }
    }

    public class Student
    {
        public int ID { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public int StudentBatchId { get; set; }

        [JsonIgnore]
        public StudentBatch StudentBatch { get; set; }

        //for display
        public bool IsSubmitted { get; set; }
        public string Status { get; set; } = "Assigned";
        public int? Score { get; set; }

    }

    public class StudentBatchDto
    {
        public string Batch { get; set; }
        public string Description { get; set; }
        public List<string> Students { get; set; } = new List<string>();
        public string AttachmentPath { get; set; }
        public DateTime CDate { get; set; }

    }

    public class TaskModel
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
    }


    // DTOs/StudentWithBatchDto.cs
    public class StudentWithBatchDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Batch { get; set; }
        public string Description { get; set; }

        public string AttachmentPath { get; set; }

        public bool IsSubmitted { get; set; }

        public string Status { get; set; }

        public int? Score { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CommitmentStatus { get; set; } = "";
        public string LateReason { get; set; } = "";

    }


}
