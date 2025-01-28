using NegusEventsApi.DTO;
using NegusEventsApi.Models.Ticket;
using System.Threading.Tasks;

namespace NegusEventsApi.Services
{
    public interface ITicketService
    {
        Task<IEnumerable<Tickets>> GetAllTicketsAsync();
        Task<IEnumerable<Tickets>> GetAllTicketsByEventIdAsync(string eventId);
        Task<IEnumerable<Tickets>> GetReservedTicketsByEventIdAsync(string eventId);
        Task<IEnumerable<Tickets>> GetConfirmedTicketsByEventIdAsync(string eventId);
        Task<IEnumerable<Tickets>> GetCancelledTicketsByEventIdAsync(string eventId);
        Task<Tickets> GetTicketByIdAsync(string id);
        Task<bool> CreateTicketReservationAsync(Tickets ticket, string attendeeId, string attendeeName, string attendeeEmail);
        Task UpdateTicketAsync(string ticketId, string userId, Tickets ticket);
        Task DeleteTicketAsync(string id);
        Task CancelTicketAsync(string id, string userId);
        Task<bool> CancelReservationAsync(string userId, string eventId);
        Task<bool> ConfirmReservationAsync(string userId, string ticketId, Int32 amountPaid);
        Task CleanupExpiredReservationsAsync();
        Task<List<Tickets>> GetReservedTicketsAsync(string userId);
        Task<List<Tickets>> GetConfirmedTickets(string userId);
        Task<List<Tickets>> GetValidReservedTicketsAsync(string attendeeId);
        //Task<List<Tickets>> GetConfirmedFromRedisTickets(string attendeeId);

    }
}
