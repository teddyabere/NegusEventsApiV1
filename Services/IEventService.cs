using NegusEventsApi.DTO;
using NegusEventsApi.Models.Event;

namespace NegusEventsApi.Services
{
    public interface IEventService
    {
        Task<IEnumerable<Events>> GetAllEventsAsync();
        Task<IEnumerable<Events>> GetActiveEventsAsync();
        Task<Events> GetEventByIdAsync(string id);
        Task<Events> GetEventByIdandUserIdAsync(string id, string userId);
        Task CreateEventAsync(Events eventItem);
        Task UpdateEventAsync(string id, Events eventItem);
        Task DeleteEventAsync(string id);
        Task<List<Events>> SearchEventsAsync(SearchEventsDTO searchCriteria);
        Task StartEventAsync(string id, string userId);
        Task PublishEventAsync(string id, string userId);
        Task ExtendEventAsync(string id, string userId, DateTime startDate, DateTime endDate);
        Task CancelEventAsync(string id, string userId);
        Task<List<Events>> GetEventByOrganizerIdAsync(string organizerId);
        Task<List<Events>> GetEventByCityandCountryAsync(string city, string country);
        Task<List<Events>> GetEventByCountryAsync(string country);
        Task<List<Events>> GetEventByCityAsync(string city);
    }
}
