using HrmsApi.Models;

namespace HrmsApi.Repository
{
    public interface taskInterface
    {

        Task<User> AddUserAsync(User user);
        Task<User> GetUserByUsernameAsync(string username);

        //login

        Task<bool> ValidateUserCredentialsAsync(string username, string password);



        ////task
          List<string> GetAllBatches();
        List<UserDto> GetUsersByBatch(string batch);




        //display

        Task<IEnumerable<Student>> GetStudentTasksByName(string name);



        //submit task
   //     Task<bool> SubmitTaskAsync(int taskId, IFormFile file);
    }
}
