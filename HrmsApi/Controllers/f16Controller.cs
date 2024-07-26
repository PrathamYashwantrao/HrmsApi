using HrmsApi.Data;
using HrmsApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace HrmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class f16Controller : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment environment;
        public f16Controller(ApplicationDbContext db , IWebHostEnvironment environment)
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


        [HttpPost]

        public IActionResult f16from(f16 f)
        {


            db.f16.Add(f);
            db.SaveChanges();
            return Ok("form uploaded Successful");
        }

        [HttpGet]

        public IActionResult fetchf16forms()
        {
            var d2 = db.f16.ToList();
            return Ok(d2);
        }

    }
}
