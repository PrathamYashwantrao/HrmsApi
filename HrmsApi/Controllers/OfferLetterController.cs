using HrmsApi.Data;
using HrmsApi.Models;
using HrmsApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HrmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfferLetterController : ControllerBase
    {
        private readonly IOfferLetterService _offerLetterService;
        private readonly ApplicationDbContext _context;
        private readonly string _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        public OfferLetterController(IOfferLetterService offerLetterService, ApplicationDbContext context)
        {
            _offerLetterService = offerLetterService;
            _context = context;

            // Ensure the uploads folder exists
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] OfferLetterDto model)
        {
            if (ModelState.IsValid)
            {
                var offerLetter = FromDto(model);

                if (model.OfferLetterPdf != null && model.OfferLetterPdf.Length > 0)
                {
                    // Ensure the email is valid as a filename
                    var emailFilename = GetSafeFilename($"{model.Email}_offerletter.pdf");
                    var pdfPath = Path.Combine(_uploadsFolder, emailFilename);
                    await System.IO.File.WriteAllBytesAsync(pdfPath, model.OfferLetterPdf);
                    // Store the PDF path in a custom field if needed, or handle as per your requirements
                }

                _context.OfferLetters.Add(offerLetter);
                await _context.SaveChangesAsync();
                return Ok(offerLetter);
            }
            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("GetOfferLetterByEmail")]
        public async Task<IActionResult> GetOfferLetterByEmail(string email)
        {
            var offerLetter = await _context.OfferLetters
                .Where(o => o.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();

            if (offerLetter == null)
            {
                return NotFound();
            }

            // Ensure the email is valid as a filename
            var emailFilename = GetSafeFilename($"{offerLetter.Email}_offerletter.pdf");
            var pdfPath = Path.Combine(_uploadsFolder, emailFilename);

            if (!System.IO.File.Exists(pdfPath))
            {
                return NotFound("PDF file not found.");
            }

            var pdfBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
            return File(pdfBytes, "application/pdf", $"OfferLetter_{email}.pdf");
        }

        // Convert from DTO to OfferLetter
        public static OfferLetter FromDto(OfferLetterDto dto)
        {
            return new OfferLetter
            {
                Id = dto.Id,
                Role = dto.Role,
                Name = dto.Name,
                Email = dto.Email,
                DateOfJoining = dto.DateOfJoining,
                IsSent = dto.IsSent,
                Salary = dto.Salary,
                Designation = dto.Designation,
                Hra = dto.Hra,
                TravelAllowance = dto.TravelAllowance,
                Bonus = dto.Bonus,
                SpecialAllowance = dto.SpecialAllowance,
                Medical = dto.Medical,
                Pf = dto.Pf,
                BasicSalary = dto.BasicSalary,
                ProfessionalTax = dto.ProfessionalTax,
                Tds = dto.Tds,
                // The OfferLetterPdf is not included here as it's not in the OfferLetter class
            };
        }

        // Helper method to create a safe filename
        private static string GetSafeFilename(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }
    }

}