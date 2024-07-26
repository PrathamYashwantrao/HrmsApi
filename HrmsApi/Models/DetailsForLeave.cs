using System.ComponentModel.DataAnnotations;

namespace HrmsApi.Models
{
    public class DetailsForLeave
    {
        [Key]
        public int uid { get; set; }
        public string uname { get; set; }
        public DateOnly doj { get; set; }
    }
}
