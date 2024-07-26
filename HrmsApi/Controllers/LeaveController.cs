using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HrmsApi.Models;
using HrmsApi.Data;


namespace HrmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        //private readonly ILeaveService _leaveService;
        public LeaveController(ApplicationDbContext db)
        {
            this.db = db;
            //_leaveService = leaveService;
        }
        [HttpPost]
        [Route("AddLeaveRequest")]
        public IActionResult AddProduct(LeaveRequest l)
        {
            db.LeaveRequest.Add(l);
            db.SaveChanges();
            return Ok("Request Added Succesfully");
        }

        [HttpGet]
        [Route("/GetAllLeaveRequests")]
        public IActionResult GetAllRequest()
        {
            var data = db.LeaveRequest.ToList();
            return Ok(data);
        }

        // GET: api/LeaveRequest/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeaveRequest(int id)
        {
            var leaveRequest = await db.LeaveRequest.FindAsync(id);
            if (leaveRequest == null)
            {
                return NotFound("Leave Request not found");
            }
            return Ok(leaveRequest);
        }

        [HttpPut("approve")]
        public async Task<IActionResult> ApproveLeaveRequest(LeaveRequest l)
        {
            var existingLeave = await db.LeaveRequest.FindAsync(l.sr);

            if (existingLeave == null)
            {
                return NotFound("Leave Request not found");
            }

            existingLeave.uid = l.uid;
            existingLeave.uemail = l.uemail;
            existingLeave.uname = l.uname;
            existingLeave.leavefromdate = l.leavefromdate;
            existingLeave.leavetodate = l.leavetodate;
            existingLeave.reason = l.reason;
            existingLeave.applyleavedays = l.applyleavedays;
            existingLeave.balleavedays = l.balleavedays;
            existingLeave.absentleavedays = l.absentleavedays;
            existingLeave.lstatus = l.lstatus;

            db.LeaveRequest.Update(existingLeave);
            await db.SaveChangesAsync();

            return Ok("Leave Request updated successfully");
        }

        // PUT: api/LeaveRequest/reject/{id}
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> RejectLeaveRequest(int id)
        {
            var existingLeave = await db.LeaveRequest.FindAsync(id);


            if (existingLeave == null)
            {
                return NotFound("Leave Request not found");
            }

            existingLeave.lstatus = "Rejected";

            db.LeaveRequest.Update(existingLeave);
            await db.SaveChangesAsync();

            return Ok("Leave Request updated successfully");
        }

        // GET: api/LeaveRequest/leaveDates/{uid}
        [HttpGet("leaveDates/{uid}")]
        public async Task<IActionResult> GetLeaveDates(int uid)
        {
            var leaveRequests = await db.LeaveRequest
                .Where(lr => lr.uid == uid && lr.lstatus == "Approved")
                .Select(lr => new { lr.leavefromdate, lr.leavetodate })
                .ToListAsync();

            if (leaveRequests == null || !leaveRequests.Any())
            {
                return NotFound("No approved leave requests found for the employee.");
            }

            return Ok(leaveRequests);
        }

        [HttpGet("leaveDatesForMonth/{uid}")]
        public async Task<IActionResult> GetLeaveDatesForMonth(int year, int month, int uid)
        {
            var leaveDates = await db.LeaveRequest
                .Where(l => l.uid == uid && l.lstatus == "Approved" && l.leavefromdate.Year == year && l.leavefromdate.Month == month)
                .Select(l => new
                {
                    l.leavefromdate,
                    l.leavetodate
                })
                .ToListAsync();

            return Ok(leaveDates);
        }

        [HttpGet("DetailsForLeave/{uid}")]
        public async Task<IActionResult> GetDetailsForLeave(int uid)
        {
            var detailsforleave = await db.DetailsForLeave.Where(l => l.uid == uid).ToListAsync();
            return Ok(detailsforleave);
        }

        [HttpGet("HasLeaveRequests/{uid}")]
        public async Task<IActionResult> HasLeaveRequests(int uid)
        {
            var hasLeaveRequests = db.LeaveRequest.Any(lr => lr.uid == uid);
            return Ok(hasLeaveRequests);
        }

        [HttpGet("GetUserLeaveData/{uid}")]
        public async Task<IActionResult> GetUserLeaveData(int uid)
        {
            var userLeaveData = db.LeaveRequest
                                 .Where(lr => lr.uid == uid)
                                 .OrderByDescending(lr => lr.leavefromdate)
                                 .Select(lr => new UserLeaveData
                                 {
                                     balleavedays = lr.balleavedays,
                                     leavefromdate = lr.leavefromdate
                                 })
                                 .FirstOrDefault();
            return Ok(userLeaveData);
        }

        public class UserLeaveData
        {
            public int balleavedays { get; set; }
            public DateTime leavefromdate { get; set; }
            // Other properties if needed
        }

        [HttpGet("approvedLeavesForMonth/{uid}")]
        public IActionResult GetApprovedLeavesForMonth(int uid, int year, int month)
        {
            var leaves = db.LeaveRequest
                .Where(l => l.uid == uid && l.lstatus == "Approved" && l.leavefromdate.Year == year && l.leavefromdate.Month == month)
                .Select(l => new LeaveDate
                {
                    LeaveFromDate = l.leavefromdate,
                    LeaveToDate = l.leavetodate,
                    IsApproved = true
                })
                .ToList();
            return Ok(leaves);
        }

        [HttpGet("pendingLeavesForMonth/{uid}")]
        public IActionResult GetPendingLeavesForMonth(int uid, int year, int month)
        {
            var leaves = db.LeaveRequest
                .Where(l => l.uid == uid && l.lstatus == "Pending" && l.leavefromdate.Year == year && l.leavefromdate.Month == month)
                .Select(l => new LeaveDate
                {
                    LeaveFromDate = l.leavefromdate,
                    LeaveToDate = l.leavetodate,
                    IsPending = true
                })
                .ToList();
            return Ok(leaves);
        }

        [HttpGet("rejectedLeavesForMonth/{uid}")]
        public IActionResult GetRejectedLeavesForMonth(int uid, int year, int month)
        {
            var leaves = db.LeaveRequest
                .Where(l => l.uid == uid && l.lstatus == "Rejected" && l.leavefromdate.Year == year && l.leavefromdate.Month == month)
                .Select(l => new LeaveDate
                {
                    LeaveFromDate = l.leavefromdate,
                    LeaveToDate = l.leavetodate,
                    IsRejected = true
                })
                .ToList();
            return Ok(leaves);
        }

        [HttpGet]
        [Route("/GetLeaveRequests/{uid}")]
        public IActionResult GetAllRequest(int uid)
        {
            var data = db.LeaveRequest.Where(l => l.uid == uid).ToList();
            return Ok(data);
        }

        [HttpPut("updateLeave/{id}")]
        public async Task<IActionResult> UpdateLeave(int id, LeaveRequest LeaveRequest)
        {
            if (id != LeaveRequest.uid)
            {
                return BadRequest();
            }

            db.Entry(LeaveRequest).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!db.LeaveRequest.Any(e => e.uid == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

    }
    public class LeaveDate
    {
        public DateTime LeaveFromDate { get; set; }
        public DateTime LeaveToDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPending { get; set; }
        public bool IsRejected { get; set; }
    }

}
