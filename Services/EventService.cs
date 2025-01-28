using Microsoft.Extensions.Logging;
using NegusEventsApi.DTO;
using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.Ticket;
using NegusEventsApi.Models.User;

namespace NegusEventsApi.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;


        public EventService(IEventRepository eventRepository, IUserRepository userRepository)
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<Events>> GetAllEventsAsync()
        {
            return await _eventRepository.GetAllEventsAsync();
        }
        public async Task<IEnumerable<Events>> GetActiveEventsAsync()
        {
            var result = await _eventRepository.GetActiveEventsAsync();
            return result;
        }

        public async Task<Events> GetEventByIdandUserIdAsync(string id, string userId)
        {

            return await _eventRepository.GetEventByIdandUserIdAsync(id, userId);
        }
        public async Task<Events> GetEventByIdAsync(string id)
        {

            return await _eventRepository.GetEventByIdAsync(id);
        }

        public async Task CreateEventAsync(Events eventItem)
        {
            eventItem.Status = EventStatus.Pending.ToString();
            var newEvent = await _eventRepository.CreateEventAsync(eventItem);
            var organizerData = await _userRepository.GetOrganizerByIdAsync(eventItem.Organizer.OrganizerId);
            var eventDetail = new EventDetail { EventId = newEvent.Id, Name = newEvent.Name, StartDate = newEvent.StartDate };
            if(organizerData != null)
                organizerData.Events.Add(eventDetail);
            
            eventItem.QuantityAvailable = eventItem.MaximumAttendees;
            eventItem.QuantitySold = 0;
            await _userRepository.UpdateUserAsync(organizerData.Id, organizerData);
        }

        public async Task UpdateEventAsync(string id, Events eventItem)
        {
            await _eventRepository.UpdateEventAsync(id, eventItem);
        }

        public async Task DeleteEventAsync(string id)
        {
            await _eventRepository.DeleteEventAsync(id);
        }
        
        public async Task<List<Events>> SearchEventsAsync(SearchEventsDTO searchCriteria)
        {
            return await _eventRepository.SearchEventsAsync(searchCriteria);
        }
        public async Task StartEventAsync(string id, string userId)
        {
            var existingEvent = await GetEventByIdandUserIdAsync(id, userId);
            if (existingEvent == null || existingEvent.Organizer.OrganizerId != userId)
            {
                throw new Exception("Event is not available");
            }
            existingEvent.Status = EventStatus.Started.ToString();

            existingEvent.UpdatedAt = DateTime.Now.ToString();
            await _eventRepository.UpdateEventAsync(id, existingEvent);
        }
        public async Task PublishEventAsync(string id, string userId)
        {
            var existingEvent = await GetEventByIdandUserIdAsync(id, userId);
            if (existingEvent == null || existingEvent.Organizer.OrganizerId != userId)
            {
                throw new Exception("Event is not available");
            }
            existingEvent.Status = EventStatus.Published.ToString();

            existingEvent.UpdatedAt = DateTime.Now.ToString();
            await _eventRepository.UpdateEventAsync(id, existingEvent);
        }
        public async Task ExtendEventAsync(string id, string userId, string startDate, string endDate)
        {
            var existingEvent = await GetEventByIdandUserIdAsync(id, userId);
            if (existingEvent == null || existingEvent.Organizer.OrganizerId != userId)
            {
                throw new Exception("Event is not available");
            }
            existingEvent.StartDate = startDate;
            existingEvent.EndDate = endDate;
            existingEvent.Status = EventStatus.Extended.ToString();

            existingEvent.UpdatedAt = DateTime.Now.ToString();
            await _eventRepository.UpdateEventAsync(id, existingEvent);
        }
        public async Task CancelEventAsync(string id, string userId)
        {
            var existingEvent = await GetEventByIdandUserIdAsync(id, userId);
            if (existingEvent == null || existingEvent.Organizer.OrganizerId != userId)
            {
                throw new Exception("Event is not available");
            }
            existingEvent.Status = EventStatus.Cancelled.ToString();

            existingEvent.UpdatedAt = DateTime.Now.ToString();
            await _eventRepository.UpdateEventAsync(id, existingEvent);
        }

        public Task<List<Events>> GetEventByOrganizerIdAsync(string organizerId)
        {
            var eventsInOrganizer = _eventRepository.GetEventByOrganizerIdAsync(organizerId);
            return eventsInOrganizer;
        }

        public Task<List<Events>> GetEventByCityandCountryAsync(string city, string country)
        {
            var eventsInCountry = _eventRepository.GetEventByCityandCountryAsync(city, country);
            return eventsInCountry;
        }

        public Task<List<Events>> GetEventByCountryAsync(string country)
        {
            var eventsInCountry = _eventRepository.GetEventByCountryAsync(country);
            return eventsInCountry;
        }

        public Task<List<Events>> GetEventByCityAsync(string city)
        {
            var eventsInCity = _eventRepository.GetEventByCityAsync(city);
            return eventsInCity;
        }
    }
}
