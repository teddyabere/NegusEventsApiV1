using NegusEventsApi.DTO;

namespace NegusEventsApi.Models.Event
{
    public interface IEventRepository
    {
        Task<IEnumerable<Events>> GetAllEventsAsync();
        Task<IEnumerable<Events>> GetActiveEventsAsync();
        Task<Events> GetEventByIdAsync(string id);
        Task<Events> GetEventByIdandUserIdAsync(string id, string userId);
        Task<Events> CreateEventAsync(Events eventItem);
        Task UpdateEventAsync(string id, Events eventItem);
        Task DeleteEventAsync(string id);
        Task<List<Events>> SearchEventsAsync(SearchEventsDTO searchCriteria);
        Task<List<Events>> GetEventByOrganizerIdAsync(string organizerId);
        Task<List<Events>> GetEventByCityandCountryAsync(string city, string country);
        Task<List<Events>> GetEventByCountryAsync(string country);
        Task<List<Events>> GetEventByCityAsync(string city);


    }
}
