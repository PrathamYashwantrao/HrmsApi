using HrmsApi.Data;
using HrmsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.IO.Image;
using System.Net.Mail;
using System.Net;

namespace HrmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayslipController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadsFolder = "uploads"; // Relative path to your uploads folder

        public PayslipController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("generate")]
        public IActionResult GeneratePayslip(string email, string month, int year)
        {
            var joiningDetails = _context.JoiningForm.FirstOrDefault(j => j.Uemail == email);
            var leaveDetails =_context.LeaveRequest .Join(_context.userLogin,
            l => l.uemail,
            u => u.uemail,
            (l, u) => new { LeaveRequest = l, User = u })
        .AsEnumerable() // Client-side evaluation
        .Where(x => x.User.uemail == email && x.LeaveRequest.lstatus == "Approved" &&
                    x.LeaveRequest.leavefromdate.Month == DateTime.ParseExact(month, "MMMM", CultureInfo.InvariantCulture).Month &&
                    x.LeaveRequest.leavefromdate.Year == year)
        .Sum(x => (x.LeaveRequest.leavetodate - x.LeaveRequest.leavefromdate).Days + 1);

            var offerLetterDetails = _context.OfferLetters.FirstOrDefault(o => o.Email == email);

            if (joiningDetails == null || offerLetterDetails == null)
            {
                return NotFound();
            }

            int totalDaysInMonth = DateTime.DaysInMonth(year, DateTime.ParseExact(month, "MMMM", CultureInfo.InvariantCulture).Month);
            decimal dailySalary = (offerLetterDetails.BasicSalary + offerLetterDetails.Hra.GetValueOrDefault() + offerLetterDetails.TravelAllowance.GetValueOrDefault() + offerLetterDetails.Bonus.GetValueOrDefault() + offerLetterDetails.SpecialAllowance.GetValueOrDefault() + offerLetterDetails.Medical.GetValueOrDefault()) / totalDaysInMonth;
            decimal leaveDeduction = dailySalary * leaveDetails;
            decimal netSalary = (offerLetterDetails.BasicSalary + offerLetterDetails.Hra.GetValueOrDefault()) + offerLetterDetails.TravelAllowance.GetValueOrDefault() + offerLetterDetails.Bonus.GetValueOrDefault() + offerLetterDetails.SpecialAllowance.GetValueOrDefault() + offerLetterDetails.Medical.GetValueOrDefault() - leaveDeduction - offerLetterDetails.ProfessionalTax - offerLetterDetails.Tds;

            var payslipData = new PayslipData
            {
                EmployeeName = joiningDetails.Fullname,
                Designation = joiningDetails.Designation,
                BankName = joiningDetails.Bankname,
                EmployeeNo = joiningDetails.Jdid.ToString(),
                IFSCCode = joiningDetails.Ifsccode,
                DateOfJoining = joiningDetails.Dateofjoining,
                BankAccountNo = joiningDetails.Accountnumber,
                DaysPaid = totalDaysInMonth - leaveDetails,
                PAN = joiningDetails.Pan,
                DaysInMonth = totalDaysInMonth,
                Aadhar = joiningDetails.Aadharno,
                UAN = "NA",
                BalanceLeave = joiningDetails.Totalyearsofexperience,
                Basic = offerLetterDetails.BasicSalary,
                HRA = offerLetterDetails.Hra.GetValueOrDefault(),
                TravelAllowance = offerLetterDetails.TravelAllowance.GetValueOrDefault(),
                Bonus = offerLetterDetails.Bonus.GetValueOrDefault(),
                SpecialAllowance = offerLetterDetails.SpecialAllowance.GetValueOrDefault(),
                MedicalReimbursement = offerLetterDetails.Medical.GetValueOrDefault(),
                ProfessionalTax = offerLetterDetails.ProfessionalTax,
                TDS = offerLetterDetails.Tds,
                NetSalary = netSalary
            };
            byte[] pdfBytes;
            // Generate PDF and save to uploads folder
            string fileName = $"Payslip_{email}_{month}_{year}.pdf";
            string filePath = Path.Combine("uploads", fileName);
            using (var stream = new MemoryStream())
            {
                using (var writer = new PdfWriter(filePath))
                {
                    using (var pdf = new PdfDocument(writer))
                    {

                        var document = new iText.Layout.Document(pdf);

                        // Add header with logo
                        var logoUrl = "https://masstechbusiness.com/wp-content/uploads/2021/12/Masstech-Logo-Resize.png";
                        var logoImage = new Image(ImageDataFactory.Create(logoUrl))
                            .SetWidth(100)
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                        var headerTable = new Table(1).UseAllAvailableWidth();
                        headerTable.AddCell(new Cell().Add(logoImage).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                        headerTable.AddCell(new Cell().Add(new Paragraph("Masstech Business Solutions")
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(18).SetBold()));
                        headerTable.AddCell(new Cell().Add(new Paragraph($"Payslip for the Month of {month} {year}")
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(14)));
                        document.Add(headerTable);
                        document.Add(new Paragraph("\n"));

                        // Add employee details table
                        var employeeDetailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 25, 25, 25, 25 })).UseAllAvailableWidth();
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("Name of Employee:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.EmployeeName)));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("Designation:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.Designation)));

                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("Bank Name:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.BankName)));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("Employee No:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.EmployeeNo)));

                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("IFSC Code:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.IFSCCode)));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("Date of Joining:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.DateOfJoining.ToString("dd-MM-yyyy"))));

                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("Bank Account No:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.BankAccountNo)));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("Days Paid:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.DaysPaid.ToString())));

                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("PAN:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.PAN)));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("Days in Month:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.DaysInMonth.ToString())));

                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("Aadhar:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.Aadhar)));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("UAN:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.UAN)));

                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph("Balance Leave:").SetBold()));
                        employeeDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.BalanceLeave.ToString())));
                        document.Add(employeeDetailsTable);
                        document.Add(new Paragraph("\n"));

                        // Add salary details table
                        var salaryDetailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 33, 33, 33, 33 })).UseAllAvailableWidth();
                        salaryDetailsTable.AddHeaderCell(new Cell().Add(new Paragraph("Gross Salary").SetBold()));
                        salaryDetailsTable.AddHeaderCell(new Cell().Add(new Paragraph("Amount").SetBold()));
                        salaryDetailsTable.AddHeaderCell(new Cell().Add(new Paragraph("Deduction").SetBold()));
                        salaryDetailsTable.AddHeaderCell(new Cell().Add(new Paragraph("Amount").SetBold()));

                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("Basic")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.Basic.ToString("C"))));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("")));

                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("HRA")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.HRA.ToString("C"))));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("Professional Tax")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.ProfessionalTax.ToString("C"))));

                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("Travel Allowance")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.TravelAllowance.ToString("C"))));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("TDS")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.TDS.ToString("C"))));

                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("Bonus")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.Bonus.ToString("C"))));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("")));

                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("Special Allowance")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.SpecialAllowance.ToString("C"))));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("")));

                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("Medical Reimbursement")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph(payslipData.MedicalReimbursement.ToString("C"))));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("")));

                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("Gross Salary")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph((payslipData.Basic + payslipData.HRA + payslipData.TravelAllowance + payslipData.Bonus + payslipData.SpecialAllowance + payslipData.MedicalReimbursement).ToString("C"))));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("Total Deduction")));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph((payslipData.ProfessionalTax + payslipData.TDS).ToString("C"))));
                        salaryDetailsTable.AddCell(new Cell().Add(new Paragraph("")));

                        document.Add(salaryDetailsTable);

                        // Add net salary
                        document.Add(new Paragraph("\n"));
                        document.Add(new Paragraph($"Net Salary Paid: {payslipData.NetSalary:C}")
                            .SetTextAlignment(TextAlignment.RIGHT).SetBold());

                        document.Add(new Paragraph("\n"));
                        document.Add(new Paragraph("This is a computerised generated salary slip and does not require authentication")
                            .SetTextAlignment(TextAlignment.CENTER).SetFontSize(10).SetItalic());

                        document.Close();
                        pdfBytes = stream.ToArray();
                    }
                }
            }
            var emailTo = joiningDetails.Uemail; // Replace with actual recipient email address
            var emailSubject = "Payslip for " + month + " " + year;
            var emailBody = "Please find attached your payslip for the month.";

            SendEmailWithAttachment(emailTo, emailSubject, emailBody, pdfBytes);


            return Ok(payslipData);
        }
        private void SendEmailWithAttachment(string to, string subject, string body, byte[] attachment)
        {
            using (var client = new SmtpClient("smtp.gmail.com") // Replace with your SMTP server
            {
                Port = 587, // Replace with your SMTP port
                Credentials = new NetworkCredential("prathamyrao47@gmail.com", "npsxpsbhhoqqmpgc"), // Replace with your email and password
                EnableSsl = true,
            })
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("swapnaliasjadhav@gmail.com"), // Replace with your email
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);

                using (var stream = new MemoryStream(attachment))
                {
                    mailMessage.Attachments.Add(new Attachment(stream, "Payslip.pdf", "application/pdf"));
                    client.Send(mailMessage);
                }
            }
        }


        
       

           // Relative path to your uploads folder

        [HttpGet("GetpayslipByemailAlL")]
        public IActionResult GetPayslipByEmailAll(string email)
        {
            // Assuming _uploadsFolder is the folder where PDF files are stored
            var pdfFiles = Directory.GetFiles(_uploadsFolder, "*.pdf")
                                    .Select(Path.GetFileName)
                                    .Where(fileName => fileName.Contains(email))  // Filter by email
                                    .ToList();

            return Ok(pdfFiles);
        }

        [HttpGet("GetPayslipContent")]
        public async Task<IActionResult> GetPayslipContent(string payslipFileName)
        {
            if (string.IsNullOrEmpty(payslipFileName))
            {
                return BadRequest("Payslip file name is required.");
            }

            var filePath = Path.Combine(_uploadsFolder, payslipFileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", payslipFileName); // Adjust the content type based on your file type
        }
    }
}


