using HrmsApi.Data;
using HrmsApi.Models;
using HrmsApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HrmsApi.Data;
using HrmsApi.Models;
using HrmsApi.Repository;

namespace HrmsApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TaskController : ControllerBase
	{

		private readonly taskInterface _userRepository;
		private readonly ILogger<TaskController> _logger;
		private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
		private readonly ApplicationDbContext _context;
		public TaskController(taskInterface userRepository, ILogger<TaskController> logger, ApplicationDbContext context)
		{
			_context = context;
			_userRepository = userRepository;
			_logger = logger;
			if (!Directory.Exists(_uploadFolder))
			{
				Directory.CreateDirectory(_uploadFolder);
			}
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(UserRegistrationDto userDto)
		{
			if (userDto.Password != userDto.ConfirmPassword)
			{
				return BadRequest("Passwords do not match.");
			}

			string salt = GenerateSalt();
			string hashedPassword = HashPassword(userDto.Password, salt);

			User user = new User
			{
				Username = userDto.Username,
				Password = hashedPassword,
				Salt = salt,
				Batch = userDto.Batch,
				Email = userDto.Email,
				FullName = userDto.FullName,
				Role = "Trainee"
			};

			await _userRepository.AddUserAsync(user);

			return Ok("Registration successful.");
		}



		[HttpPost("login")]
		public async Task<IActionResult> Login(UserLoginDto loginDto)
		{
			bool isValidUser = await _userRepository.ValidateUserCredentialsAsync(loginDto.Username, loginDto.Password);
			if (!isValidUser)
			{
				return Unauthorized("Invalid username or password.");
			}

			var user = await _userRepository.GetUserByUsernameAsync(loginDto.Username);

			return Ok(new
			{
				Id = user.Id,
				Username = user.Username,
				Email = user.Email,
				FullName = user.FullName,
				Batch = user.Batch,
				Role = user.Role


			});
		}

		private string GenerateSalt()
		{
			byte[] saltBytes = new byte[16];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(saltBytes);
			}
			return Convert.ToBase64String(saltBytes);
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


		[HttpGet("/batches")]
		public IActionResult GetBatches()
		{
			List<string> batches = _userRepository.GetAllBatches();
			return Ok(batches);
		}

		[HttpGet("/GetUsersByBatch/{batch}")]
		public IActionResult GetUsersByBatch(string batch)
		{
			return Ok(_userRepository.GetUsersByBatch(batch));
		}
	}
}
