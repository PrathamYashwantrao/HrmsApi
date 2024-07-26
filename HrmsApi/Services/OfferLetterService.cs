using HrmsApi.Data;
using HrmsApi.Models;

namespace HrmsApi.Services
{
    public class OfferLetterService : IOfferLetterService
    {
        private readonly ApplicationDbContext _context;

        public OfferLetterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OfferLetter> GenerateOfferLetterAsync(OfferLetterViewModel model)
        {
            var offerLetter = new OfferLetter
            {
                Name = model.Name,
                Role = model.Role,
                Email = model.Email,
                DateOfJoining = model.DateOfJoining,
                Salary = model.Salary,
                Hra = model.Hra,
                TravelAllowance = model.TravelAllowance,
                Bonus = model.Bonus,
                SpecialAllowance = model.SpecialAllowance,
                Medical = model.Medical,
                Pf = model.Pf,
                IsSent = false
            };

            _context.OfferLetters.Add(offerLetter);
            await _context.SaveChangesAsync();
            return offerLetter;
        }

        private string GenerateOfferLetterContent(OfferLetterViewModel model)
        {
            // Implementation depends on your specific template
            // You may want to use different templates for different roles
            string template = model.Role == "Trainee" ?
                File.ReadAllText("Templates/OfferLetterTemplate.html") :
                File.ReadAllText("Templates/OfferLetterOtherTemplate.html");

            // Replace placeholders in the template with actual values
            // This is a simplified version, you'll need to replace all placeholders
            template = template.Replace("{Name}", model.Name)
                               .Replace("{Role}", model.Role)
                               .Replace("{DateOfJoining}", model.DateOfJoining.ToString("MMMM d, yyyy"));

            if (model.Role != "Trainee")
            {
                template = template.Replace("{Salary}", model.Salary?.ToString("C"))
                                   .Replace("{Hra}", model.Hra?.ToString("C"))
                                   // ... replace other placeholders
                                   ;
            }

            return template;
        }
    }
}