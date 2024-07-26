using HrmsApi.Data;
using HrmsApi.Models;
using HrmsApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using HrmsApi.Data;
using HrmsApi.Models;
using HrmsApi.Repository;

namespace HrmsApi.Controllers
{
	public class TestingController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly taskInterface _userRepository;
		private readonly IWebHostEnvironment _environment;

		public TestingController(ApplicationDbContext context, taskInterface userRepository, IWebHostEnvironment environment)
		{
			_context = context;
			_userRepository = userRepository;
			_environment = environment;
		}

		[HttpPost("AddTask")]
		public async Task<IActionResult> PostStudentBatch([FromBody] StudentBatchDto studentBatchDto)
		{
			if (studentBatchDto == null)
			{
				return BadRequest();
			}

			var studentBatch = new StudentBatch
			{
				Batch = studentBatchDto.Batch,
				Description = studentBatchDto.Description,
				AttachmentPath = studentBatchDto.AttachmentPath,
				CDate = studentBatchDto.CDate,
				Students = studentBatchDto.Students?.Select(name => new Student { Name = name }).ToList() ?? new List<Student>()
			};

			_context.StudentBatches.Add(studentBatch);
			await _context.SaveChangesAsync();

			return Ok(studentBatch);
		}

		[HttpGet("GetStudentTasksByName/{name}")]
		public async Task<ActionResult<IEnumerable<StudentWithBatchDto>>> GetStudentTasksByName(string name)
		{
			var students = await _context.Students
				.Include(s => s.StudentBatch)
				.Where(s => s.Name == name && (s.Status == "Rejected" || s.Status == "Pending")) // Filter out submitted tasks
				.Select(s => new StudentWithBatchDto
				{
					ID = s.ID,
					Name = s.Name,
					Batch = s.StudentBatch.Batch,
					Description = s.StudentBatch.Description,
					AttachmentPath = s.StudentBatch.AttachmentPath,
					IsSubmitted = s.IsSubmitted
				})
				.ToListAsync();

			if (students == null || students.Count == 0)
			{
				return NotFound();
			}

			return Ok(students);
		}




		[HttpPost("SubmitTask")]
		public async Task<IActionResult> SubmitTask([FromForm] TaskSubmissionDto model)
		{
			if (model.File == null || model.File.Length == 0)
			{
				return BadRequest("File not provided.");
			}

			if (model.LateReason == null)
			{
				model.LateReason = "";
			}

			var student = await _context.Students
				.Where(s => s.ID == model.TaskId) // Assuming model.StudentId corresponds to Student ID
				.FirstOrDefaultAsync();

			if (student == null)
			{
				return NotFound("Student not found for the given task ID.");
			}

			var studentBatch = await _context.StudentBatches.FindAsync(student.StudentBatchId);
			if (studentBatch == null)
			{
				return NotFound("Student batch not found.");
			}

			var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
			if (!Directory.Exists(uploadDir))
			{
				Directory.CreateDirectory(uploadDir);
			}

			var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.File.FileName)}";
			var filePath = Path.Combine(uploadDir, fileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await model.File.CopyToAsync(stream);
			}

			var submittedTask = new SubmittedTask
			{
				TaskId = model.TaskId,
				Description = studentBatch.Description,
				Batch = studentBatch.Batch,
				StudentName = student.Name,
				FileName = fileName,
				FilePath = filePath,
				CreatedAt = studentBatch.CDate,
				SubmittedAt = DateTime.UtcNow,
				LateReason = model.LateReason,
				CommitmentStatus = model.CommitmentStatus,
			};

			_context.SubmittedTasks.Add(submittedTask);

			// Mark the task as submitted
			student.IsSubmitted = true;
			student.Status = submittedTask.Status;
			_context.Students.Update(student);

			await _context.SaveChangesAsync();

			return Ok(new { Message = "Task submitted successfully." });
		}

		[HttpGet("GetPreviousTasksByName/{name}")]
		public async Task<ActionResult<IEnumerable<StudentWithBatchDto>>> GetPreviousTasksByName(string name)
		{
			var tasks = await _context.Students
				.Include(s => s.StudentBatch)
				.Where(s => s.Name == name)
				.Select(s => new StudentWithBatchDto
				{
					ID = s.StudentBatch.ID,
					Name = s.Name,
					Batch = s.StudentBatch.Batch,
					Description = s.StudentBatch.Description,
					AttachmentPath = s.StudentBatch.AttachmentPath,
					IsSubmitted = s.IsSubmitted,
					Score = s.Score
				})
				.ToListAsync();

			if (tasks == null || tasks.Count == 0)
			{
				return NotFound();
			}

			return Ok(tasks);
		}

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

				return Ok(new { Message = $"Score updated successfully for TaskId {taskId}." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Message = $"Failed to update score for TaskId {taskId}: {ex.Message}" });
			}
		}
        [HttpGet("top-performers")]
        public IQueryable<TopPerformerDto> GetTopPerformers()
        {
            var startDate = DateTime.Now.Date.AddDays(-((int)DateTime.Now.DayOfWeek));
            var endDate = startDate.AddDays(4);

            var topPerformers = _context.SubmittedTasks
                .Where(task => task.SubmittedAt >= startDate && task.SubmittedAt <= endDate)
                .GroupBy(task => new { task.StudentName, task.Batch })
                .Select(g => new
                {
                    Name = g.Key.StudentName,
                    Batch = g.Key.Batch,
                    TotalScore = g.Sum(x => x.Score)
                })
                .OrderByDescending(x => x.TotalScore)
                .Take(3)
                .ToList();

            return topPerformers.Select(tp => new TopPerformerDto
            {
                Name = tp.Name,
                Batch = tp.Batch,
                TotalScore = tp.TotalScore
            }).AsQueryable();
        }



    }

}

