using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NegusEventsApi.Models;
using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.Ticket;
using NegusEventsApi.Models.User;
using ZstdSharp;

namespace NegusEventsApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task CreateUserAsync(Users user)
        {
            if (user != null && user.Role == "Organizer")
            {
                user.Status = "Pending";
                user.Events = new List<EventDetail>();
               
            }

            if (user != null && user.Role == "Attendee")
            {
                user.Tickets = new List<TicketDetail>();

            }
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            await _userRepository.AddUserAsync(user);
            
        }
        public async Task ApproveOrgannizer (Users user)
        {
            if (user != null && user.Role == "Organizer")
            {
                user.Status = "Approved";
                await _userRepository.ApproveOrgannizer(user);
            }

        }

        public async Task ResetPasswordAsync(string id, Users user, string token, string hashedPassword)    
        {
            var negusToken = "NEGUSTOKEN";

            if (token == negusToken)
            {
                user.Password = hashedPassword;
                await _userRepository.UpdateUserAsync(id, user);
            }
            else
            {
                throw new Exception ("Token is not correct Access");
            }


        }
        public async Task<Users> GetByEmailAsync(string email) { 
            var userE = await _userRepository.GetUserByEmailAsync(email);
            return userE;
        }
        public async Task<List<Users>> GetPendingOrganizers(){ 
            var userE = await _userRepository.GetPendignOrganizers();
            return userE;
        }
        
        public async Task<List<Users>> GetApprovedOrganizers()
        { 
            var userE = await _userRepository.GetApprovedOrganizers();
            return userE;
        }

        public async Task<Users> GetByIdAsync(string id){
            return await _userRepository.GetUserByIdAsync(id);
        }
        public async Task<Users> GetOrganizerByIdAsync(string id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }
    }

    }

