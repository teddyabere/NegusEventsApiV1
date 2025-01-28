using NegusEventsApi.DTO;

namespace NegusEventsApi.Models.Ticket
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Tickets>> GetAllTicketsAsync();
        Task<IEnumerable<Tickets>> GetAllTicketsByEventIdAsync(string eventId);
        Task<IEnumerable<Tickets>> GetConfirmedTicketsByEventIdAsync(string eventId);
        Task<IEnumerable<Tickets>> GetReservedTicketsByEventIdAsync(string eventId);
        Task<IEnumerable<Tickets>> GetCancelledTicketsByEventIdAsync(string eventId);
        Task<Tickets> GetTicketByIdAsync(string id);
        Task<Tickets> CheckReservedTicketByUserIdAsync(string userId, string eventId);
        Task<List<Tickets>> CheckExpiredTicketByUserIdAsync();
        Task CreateTicketAsync(Tickets ticket);
        Task UpdateTicketAsync(string id, Tickets ticket);
        Task DeleteTicketAsync(string id);
        Task<List<EventPopularityDto>> GetEventPopularityAsync();
        Task<List<RevenueByTicketTypeDto>> GetRevenueByTicketTypeAsync(string eventId);

        Task<TicketNumberTracker> GetTicketRefNumberAsync(string catagoryName, int year);
        Task CreateTicketRefNumberAsync(TicketNumberTracker ticketNum);
        Task UpdateTicketRefNumberAsync(string id, int lastNumber);
        Task DeleteTicketRefNumberAsync(string id);
        
        Task<List<Tickets>> GetReservedTicketsByUserAsync(string userId);
        Task<List<Tickets>> GetConfirmedTickets(string userId);

    }
}
