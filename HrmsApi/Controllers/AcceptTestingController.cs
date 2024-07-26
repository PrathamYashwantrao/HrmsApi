using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HrmsApi.Data;
using HrmsApi.Models;

namespace HrmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcceptTestingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AcceptTestingController(ApplicationDbContext context)
        {
            _context = context;
        }

        //// Endpoint to accept a task
        //[HttpPost("AcceptTask/{taskId}")]
        //public async Task<IActionResult> AcceptTask(int taskId)
        //{
        //    try
        //    {
        //        var task = await _context.SubmittedTasks.FindAsync(taskId);
        //        if (task == null)
        //        {
        //            return NotFound(new { Message = $"Task with ID {taskId} not found." });
        //        }

        //        task.IsAccepted = true;
        //        task.IsRejected = false; // Ensure task is not marked as rejected
        //        _context.SubmittedTasks.Update(task);
        //        await _context.SaveChangesAsync();

        //        return Ok(new { Message = $"Task with ID {taskId} has been accepted." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = $"Error accepting task: {ex.Message}" });
        //    }
        //}

        //// Endpoint to reject a task
        //[HttpPost("RejectTask/{taskId}")]
        //public async Task<IActionResult> RejectTask(int taskId)
        //{
        //    try
        //    {
        //        var task = await _context.SubmittedTasks.FindAsync(taskId);
        //        if (task == null)
        //        {
        //            return NotFound(new { Message = $"Task with ID {taskId} not found." });
        //        }

        //        task.IsAccepted = false; // Ensure task is not marked as accepted
        //        task.IsRejected = true;
        //        _context.SubmittedTasks.Update(task);
        //        await _context.SaveChangesAsync();

        //        return Ok(new { Message = $"Task with ID {taskId} has been rejected." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = $"Error rejecting task: {ex.Message}" });
        //    }
        //}

        //// Endpoint to update score for a task
        //[HttpPost("UpdateScore/{taskId}")]
        //public async Task<IActionResult> UpdateScore(int taskId, [FromBody] int score)
        //{
        //    try
        //    {
        //        var taskToUpdate = await _context.SubmittedTasks.FindAsync(taskId);
        //        if (taskToUpdate == null)
        //        {
        //            return NotFound(new { Message = $"Task with ID {taskId} not found." });
        //        }

        //        if (!taskToUpdate.IsAccepted)
        //        {
        //            return BadRequest(new { Message = "Score can only be updated for accepted tasks." });
        //        }

        //        taskToUpdate.Score = score;
        //        _context.SubmittedTasks.Update(taskToUpdate);
        //        await _context.SaveChangesAsync();

        //        return Ok(new { Message = $"Score updated successfully for Task ID {taskId}." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = $"Failed to update score for Task ID {taskId}: {ex.Message}" });
        //    }
        //}


        [HttpPost("UpdateScore/{taskId}")]
        public IActionResult UpdateScore(int taskId, [FromBody] int score)
        {
            try
            {
                var taskToUpdate = _context.SubmittedTasks.FirstOrDefault(t => t.TaskId == taskId);
                if (taskToUpdate == null)
                {
                    return NotFound(new { Message = $"Task with TaskId {taskId} not found." });
                }

                taskToUpdate.Score = score;

                _context.SaveChanges(); // Save changes to the database


				var studentsUpdate = _context.Students.FirstOrDefault(t => t.ID == taskId);

				studentsUpdate.Score = score;
                _context.SaveChanges();

				return Ok(new { Message = $"Score updated successfully for TaskId {taskId}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Failed to update score for TaskId {taskId}: {ex.Message}" });
            }
        }


        // Endpoint to get all submitted tasks
        [HttpGet("GetAllSubmittedTasks")]
        public async Task<ActionResult<IEnumerable<SubmittedTask>>> GetAllSubmittedTasks()
        {
            try
            {
                var tasks = await _context.SubmittedTasks.ToListAsync();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Failed to retrieve submitted tasks: {ex.Message}" });
            }
        }


        [HttpPost("AcceptTask/{taskId}")]
        public async Task<IActionResult> AcceptTask(int taskId)
        {
            try
            {
                var student = await _context.Students.FindAsync(taskId);

                if (student == null)
                {
                    return NotFound($"Student with ID {taskId} not found.");
                }

                student.Status = "Accepted";
                _context.Students.Update(student);
                await _context.SaveChangesAsync();


				var submitTask = _context.SubmittedTasks.FirstOrDefault(s=>s.TaskId==taskId);

				if (submitTask == null)
				{
					return NotFound($"Student with ID {taskId} not found.");
				}

				submitTask.Status = "Accepted";
				_context.SubmittedTasks.Update(submitTask);
				await _context.SaveChangesAsync();

				return Ok(new { Message = $"Task for student {student.Name} accepted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Failed to accept task: {ex.Message}" });
            }
        }

        [HttpPost("RejectTask/{taskId}")]
        public async Task<IActionResult> RejectTask(int taskId)
        {
            try
            {
                var student = await _context.Students.FindAsync(taskId);

                if (student == null)
                {
                    return NotFound($"Student with ID {taskId} not found.");
                }

                student.Status = "Rejected"; // Update status to Rejected
                _context.Students.Update(student);
                await _context.SaveChangesAsync();


				var submitTask = _context.SubmittedTasks.FirstOrDefault(s => s.TaskId == taskId);

				if (submitTask == null)
				{
					return NotFound($"Student with ID {taskId} not found.");
				}

				submitTask.Status = "Rejected";
				_context.SubmittedTasks.Update(submitTask);
				await _context.SaveChangesAsync();

				return Ok(new { Message = $"Task for student {student.Name} rejected successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Failed to reject task: {ex.Message}" });
            }
        }



        [HttpGet("GetStudentTasksByName/{name}")]
        public async Task<ActionResult<IEnumerable<StudentWithBatchDto>>> GetStudentTasksByName(string name)
        {
            var students = await _context.Students
                .Include(s => s.StudentBatch)
                .Where(s => s.Name == name && (s.Status == "Rejected" || s.Status == "Pending" || s.Status == "Assigned")) // Filter out submitted tasks
                .Select(s => new StudentWithBatchDto
                {
                    ID = s.ID,
                    Name = s.Name,
                    Batch = s.StudentBatch.Batch,
                    Description = s.StudentBatch.Description,
                    AttachmentPath = s.StudentBatch.AttachmentPath,
                    IsSubmitted = s.IsSubmitted,
                    Status = s.Status,
                    CreatedAt = s.StudentBatch.CDate
                })
                .ToListAsync();

            if (students == null || students.Count == 0)
            {
                return NotFound();
            }

            return Ok(students);
        }
    }
}
