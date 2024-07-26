using HrmsApi.Data;
using HrmsApi.Model;
using HrmsApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace HrmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JoiningformController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment environment;

        public JoiningformController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            this.db = db;
            this.environment = environment;
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { filePath = path });
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
            {
                { ".txt", "text/plain" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/vnd.ms-word" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".csv", "text/csv" }
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var path = Path.Combine("uploads", fileName);

            if (!System.IO.File.Exists(path))
                return NotFound("File not found.");

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(path), fileName);
        }

        [HttpPost("join")]
        public IActionResult Joiningfrom(JoiningForm j)
        { 
            db.JoiningForm.Add(j);
            db.SaveChanges();
            return Ok("User registered successfully");
        }

        
        [HttpGet]

        public IActionResult fetchforms()
        {
            var d1 = db.JoiningForm.ToList();
            return Ok(d1);
        }
    }
}