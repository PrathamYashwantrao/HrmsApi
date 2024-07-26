using HrmsApi.Data;
using HrmsApi.Models;
using HrmsApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        private readonly AuthRepo repo;
        public AuthController(AuthRepo repo, ApplicationDbContext _context)
        {
            this.repo = repo;

            this._context = _context;
        }




        [HttpPost]
        [Route("Register")]
        public IActionResult Register(userLogin u)
        {
            repo.Register(u);
            return Ok("user added successfully");
        }


        [HttpPost]
        [Route("Login")]
        public IActionResult Login(string Email, string password)
        {
            try
            {
                userLogin user = repo.Login(Email, password);

                if (user != null)
                {
                    // Authentication successful
                    return Ok(user);
                }
                else
                {
                    // Authentication failed
                    return Unauthorized("Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("fetchEmployees")]
        public IActionResult FetchAllProduct()
        {
            var data = repo.fetchEmp();
            return Ok(data); ;
        }

        [HttpDelete]
        [Route("delEmpById/{id}")]
        public IActionResult deleteEmpById(int id)
        {
            repo.deleteEmpById(id);
            return Ok("employee Deleted Successfully");
        }


        [HttpGet("GetUsersByRole")]
        public IActionResult GetUsersByRole(string role)
        {
            var users = _context.userLogin.Where(u => u.urole == role).Select(u => u.uname).ToList();
            return Ok(users);
        }

    }
}
