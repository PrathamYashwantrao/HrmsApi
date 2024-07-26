using HrmsApi.Data;
using HrmsApi.Models;
using HrmsApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HrmsApi.Services
{
    public class AuthService : AuthRepo
    {
        private readonly ApplicationDbContext db;
        public AuthService(ApplicationDbContext db)
        {
            this.db = db;
        }


        public void Register(userLogin u)
        {
            db.userLogin.Add(u);
            db.SaveChanges();
        }


        public userLogin? Login(string Email, string password)
        {
            try
            {
                // Find user by email and password
                var user = db.userLogin.FirstOrDefault(u => u.uemail == Email && u.upass == password);

                return user;

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                throw new Exception("Error during login", ex);
            }
        }

        public List<JoiningForm> fetchEmp()
        {

            return db.JoiningForm.ToList();
        }

        public void deleteEmpById(int id)
        {
            var data = db.JoiningForm.Find(id);
            db.JoiningForm.Remove(data);
            db.SaveChanges();
        }

    }
}
