using HrmsApi.Models;

namespace HrmsApi.Repository
{
    public interface AuthRepo
    {

        void Register(userLogin u);
        userLogin? Login(string Email, string password);

        List<JoiningForm> fetchEmp();

        void deleteEmpById(int id);

    }
}
