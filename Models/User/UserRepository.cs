using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NegusEventsApi.Models.User
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<Users> _userCollection;

        public UserRepository(IOptions<NegusEventsDbSettings> negusDbSettings)
        {
            var client = new MongoClient(negusDbSettings.Value.ConnectionString);
            var database = client.GetDatabase(negusDbSettings.Value.DatabaseName);
            _userCollection = database.GetCollection<Users>("User");
        }

        public async Task<IEnumerable<Users>> GetAllUserAsync()
        {
            return await _userCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Users> GetUserByIdAsync(string id)
        {
            return await _userCollection.Find(f => f.Id == id).FirstOrDefaultAsync();
        }
        public async Task<Users> GetOrganizerByIdAsync(string id)
        {
            return await _userCollection.Find(f => f.Id == id && f.Role == "Organizer").FirstOrDefaultAsync();
        }
        public async Task<Users> GetAttendeeByIdAsync(string id)
        {
            return await _userCollection.Find(f => f.Id == id && f.Role == "Attendee").FirstOrDefaultAsync();
        }
        public async Task AddUserAsync(Users User)
        {
            await _userCollection.InsertOneAsync(User);
        }
        public async Task ApproveOrgannizer(Users user)
        {
            await _userCollection.ReplaceOneAsync(e => e.Id == user.Id, user);
            
        }
        public async Task<Users> GetUserByEmailAsync(string email)
        {
            return await _userCollection.Find(user => user.Email == email).FirstOrDefaultAsync();
        }
        public async Task<List<Users>> GetPendignOrganizers()
        {
            return await _userCollection.Find(user => user.Role == "Organizer" && user.Status == "Pending")
                .ToListAsync();
        }
        public async Task<List<Users>> GetApprovedOrganizers()
        {
            return await _userCollection.Find(user => user.Role == "Organizer" && user.Status == "Approved")
                .ToListAsync();
        }
        public async Task UpdateUserAsync(string id, Users User)
        {
            await _userCollection.ReplaceOneAsync(f => f.Id == id, User);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _userCollection.DeleteOneAsync(f => f.Id == id);
        }

        public Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }
    }
}
