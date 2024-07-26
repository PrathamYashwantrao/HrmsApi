using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using HrmsApi.Data;
using HrmsApi.Models;

namespace HrmsApi.Repository
{
    public class TaskService:taskInterface
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }



        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
            {
                return false;
            }

            string hashedPassword = HashPassword(password, user.Salt);
            return user.Password == hashedPassword;
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] saltedPasswordBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(saltedPasswordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }



        //task adding





        //for getting batches and students in dropdown

        public List<UserDto> GetUsersByBatch(string batch)
        {
            var users = _context.Users
                          .Where(u => u.Role == "Trainee" && u.Batch == batch)
                          .Select(u => new UserDto
                          {
                              Id = u.Id,
                              Username = u.Username,
                              Batch = u.Batch,
                              Email = u.Email,
                              FullName = u.FullName,
                              Role = u.Role
                          })
                          .ToList();

            return users;
        }

        public List<string> GetAllBatches()
        {
            return _context.Users.Select(u => u.Batch).Distinct().ToList();
        }



        //display task for student
        public async Task<IEnumerable<Student>> GetStudentTasksByName(string name)
        {
            return await _context.Students
                .Include(s => s.StudentBatch)
                .Where(s => s.Name == name)
                .ToListAsync();
        }


        //submit task

        //public async Task<bool> SubmitTaskAsync(int taskId, IFormFile file)
        //{
        //    var task = await _context.StudentBatches.FindAsync(taskId);
        //    if (task == null)
        //    {
        //        return false;
        //    }

        //    var filePath = Path.Combine("Uploads", Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

        //    // Ensure the directory exists
        //    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        //    // Save the file
        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(stream);
        //    }

        //    task.SubmittedFilePath = filePath;
        //    task.IsSubmitted = true;

        //    _context.StudentBatches.Update(task);
        //    await _context.SaveChangesAsync();

        //    return true;
        //}

    }
}
