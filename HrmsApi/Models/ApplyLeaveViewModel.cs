using System.ComponentModel.DataAnnotations;

namespace HrmsApi.Models
{
    public class ApplyLeaveViewModel
    {
        [Required(ErrorMessage = "From Date is required.")]
        [Display(Name = "From Date")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To Date is required.")]
        [Display(Name = "To Date")]
        public DateTime ToDate { get; set; }

        [Required(ErrorMessage = "Reason for leave is required.")]
        [Display(Name = "Reason for Leave")]
        public string Reason { get; set; }

        // Additional properties as needed for balance leaves, absent days, etc.
        public int EmpID { get; set; } // Assuming this comes from the logged-in user or session
        public string Name { get; set; } // Assuming this comes from the logged-in user or session
        public string Email { get; set; } // Assuming this comes from the logged-in user or session
    }
}
