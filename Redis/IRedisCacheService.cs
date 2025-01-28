using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.Ticket;

namespace NegusEventsApi.Redis
{
    public interface IRedisCacheService
    {
        Task SetCacheAsync<T>(string key, T value, TimeSpan expiration);
        Task<T?> GetCacheAsync<T>(string key);
        Task RemoveCacheAsync(string key);

        Task InitializeAvailableTicketsAsync(string eventId, int maxAttendees);
        Task<int> GetAvailableTicketsAsync(string eventId);
        Task<bool> ValidateAvailableTicketsAsync(string eventId);
        Task<bool> HasUserPurchasedTicketAsync(string userId, string eventId);
        Task MarkUserTicketPurchasedAsync(string userId, string eventId);
        Task<int> DecrementAvailableTicketsAsync(string eventId, int numberOfTickets);
        Task IncrementAvailableTicketsAsync(string eventId);
        Task ReserveTicketAsync(string userId, string eventId);
        Task<bool> CheckReservationAsync(string userId, string eventId);
        Task<List<Tickets>> GetValidReservedTicketsAsync(string attendeeId);
        // Task<List<Ticket>> GetConfirmedTicketsRedisAsync(string userId);
        Task<bool> CleanupExpiredReservationsAsync(string attendeeId);
        Task<List<string>> GetConfirmedTickets1Async(string userId);
        Task CacheAttendeeEventsAsync(string attendeeId, string eventId);
        Task<List<string>> GetAttendeeEventsAsync(string attendeeId);
        Task<List<string>> GetReservationsByEventIdAsync(string eventId);
    }
}
