using NegusEventsApi.Models.User;

namespace NegusEventsApi.Services
{
    public interface IUserService
    {
        Task<Users> GetByIdAsync(string id);
        Task<Users> GetByEmailAsync(string email);
        Task<List<Users>> GetPendingOrganizers();
        Task<List<Users>> GetApprovedOrganizers();
        Task ApproveOrgannizer(Users user);
        //Task UpdateUserAsync(string id, Users User);
        Task ResetPasswordAsync(string id, Users User, string token, string hashedPassword);
        Task CreateUserAsync(Users user);
        Task<Users> GetOrganizerByIdAsync(string id);

    }
}
