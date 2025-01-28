using NegusEventsApi.Models.User;

namespace NegusEventsApi.Models.User
{
    public interface IUserRepository
    {
        Task<IEnumerable<Users>> GetAllUsersAsync();
        Task<Users> GetUserByIdAsync(string id);
        Task AddUserAsync(Users User);
        Task UpdateUserAsync(string id, Users User);
        Task DeleteUserAsync(string id);
        Task ApproveOrgannizer(Users user);
        Task<Users> GetUserByEmailAsync(string email);
        Task<List<Users>> GetPendignOrganizers();
        Task<List<Users>> GetApprovedOrganizers();
        Task<Users> GetOrganizerByIdAsync(string id);
        Task<Users> GetAttendeeByIdAsync(string id);
    }
}
