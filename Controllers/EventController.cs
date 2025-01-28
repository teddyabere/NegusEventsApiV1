using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NegusEventsApi.DTO;
using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.Ticket;
using NegusEventsApi.Redis;
using NegusEventsApi.Services;

namespace NegusEventsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IRedisCacheService _redisCacheService;

        public EventController(IEventService eventService, IRedisCacheService cacheService)
        {
            _eventService = eventService;
            _redisCacheService = cacheService;
        }

        /// <summary>
        /// Creates a new event.
        /// </summary>
        /// <param name="eventItem">The event details.</param>
        /// <returns>A response with the created event.</returns>
        [HttpPost("create-event")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult> CreateEvent(Events eventItem)
        {
            var organizerId = User.FindFirst("UserId")?.Value;
            var organizerName = User.FindFirst("Name")?.Value;
            var orgStatus = User.FindFirst("Status")?.Value;
            if(orgStatus != "Approved")
            {
                return Unauthorized("Organizer is not Approved");
            }

            if (string.IsNullOrEmpty(organizerId))
            {
                return Unauthorized("Unauthorized user");
            }

            if (eventItem.Organizer == null)
            {
                eventItem.Organizer = new OrganizerInfo();
            }

            eventItem.Organizer.OrganizerId = organizerId;
            eventItem.Organizer.Name = organizerName;

            eventItem.CreatedAt = DateTime.Now;
            eventItem.UpdatedAt = DateTime.Now;

            await _eventService.CreateEventAsync(eventItem);

            return CreatedAtAction(nameof(GetEventById), new { id = eventItem.Id }, eventItem);
        }

        /// <summary>
        /// Retrieves all active events.
        /// </summary>
        /// <returns>A list of active events.</returns>
        [HttpGet("get-all-events")]
        public async Task<ActionResult<IEnumerable<Events>>> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            var resultFinal = new
            {
                TotalCount = events.Count(),
                Events = events
            };
            return Ok(resultFinal);
        }

        /// <summary>
        /// Retrieves all active events.
        /// </summary>
        /// <returns>A list of active events.</returns>
        [HttpGet("get-active-events")]
        public async Task<ActionResult<IEnumerable<Events>>> GetActiveEvents()
        {
            var events = await _eventService.GetActiveEventsAsync();
            var resultFinal = new
            {
                TotalCount = events.Count(),
                Events = events
            };
            return Ok(resultFinal);
        }

        /// <summary>
        /// Retrieves an event by its ID.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>The event details.</returns>
        [HttpGet("get-event-byid/{id}")]
        public async Task<ActionResult<Events>> GetEventById(string id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            return Ok(eventItem);
        }

        /// <summary>
        /// Retrieves an event by its ID from the cache, or fetches from the database if not cached.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>The event details from cache or database.</returns>
        [HttpGet("get-event-redis/{id}")]
        public async Task<ActionResult<Events>> GetEventByIdWithRedis(string id)
        {
            var cacheKey = $"event:{id}";
            var cachedEvent = await _redisCacheService.GetCacheAsync<Events>(cacheKey);

            if (cachedEvent != null)
            {
                return Ok(cachedEvent);
            }

            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            await _redisCacheService.SetCacheAsync(cacheKey, eventItem, TimeSpan.FromMinutes(30));
            return Ok(eventItem);
        }

        /// <summary>
        /// Starts an event.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>A message indicating the event has started.</returns>
        [HttpPut("start-event/{id}")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult> Startevent(string id)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                Unauthorized("Unauthorized user");
            }

            await _eventService.StartEventAsync(id, userId);
            return Ok("Event Started Successfully");
        }

        /// <summary>
        /// Extends the duration of an event.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <param name="startDate">The new start date for the event.</param>
        /// <param name="endDate">The new end date for the event.</param>
        /// <returns>A message indicating the event has been extended.</returns>
        
        [HttpPut("extend-event/{id}")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult> Extendevent(string id, DateTime startDate, DateTime endDate)
        {
            // TODO: Check if the attendee is updating its own ticket

            var userId = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                Unauthorized("Unauthorized user");
            }

            await _eventService.ExtendEventAsync(id, userId, startDate, endDate);
            return Ok("Event Extended Successfully");
        }

        /// <summary>
        /// Cancels an event.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>A message indicating the event has been cancelled.</returns>
        [HttpPut("cancel-event/{id}")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult> Cancelevent(string id)
        {
            // TODO: Check if the attendee is updating its own ticket
        
            var userId = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                Unauthorized("Unauthorized user");
            }

            await _eventService.CancelEventAsync(id, userId);
            return Ok("Event Cancelled Successfully");
        }

        /// <summary>
        /// Updates an existing event.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <param name="eventItem">The updated event details.</param>
        /// <returns>A response indicating the event has been updated.</returns>
        [HttpPut("update-event/{id}")]
        public async Task<ActionResult> UpdateEvent(string id, Events eventItem)
        {
            var organizerId = User.FindFirst("UserId")?.Value;
            var existingEvent = await _eventService.GetEventByIdandUserIdAsync(id, organizerId);
            if (existingEvent == null)
            {
                return NotFound();
            }
            await _eventService.UpdateEventAsync(id, eventItem);
            return NoContent();
        }

        /// <summary>
        /// Deletes an event by its ID.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>A response indicating the event has been deleted.</returns>
        [HttpDelete("delete-event/{id}")]
        public async Task<ActionResult> DeleteEvent(string id)
        {
            var organizerId = User.FindFirst("UserId")?.Value;
            var existingEvent = await _eventService.GetEventByIdandUserIdAsync(id, organizerId);
            if (existingEvent == null)
            {
                return NotFound();
            }
            await _eventService.DeleteEventAsync(id);
            return NoContent();
        }


        /// <summary>
        /// Searches for events based on the provided criteria.
        /// </summary>
        /// <param name="searchCriteria">The search criteria for events.</param>
        /// <returns>A list of events matching the search criteria.</returns>
        [HttpPost("search-event")]
        public async Task<ActionResult<List<Events>>> SearchEvents([FromBody] SearchEventsDTO searchCriteria)
        {
            var events = await _eventService.SearchEventsAsync(searchCriteria);
            if (events == null || !events.Any())
            {
                return NotFound("No events match the given criteria.");
            }
            var resultFinal = new
            {
                TotalCount = events.Count(),
                Events = events
            };
            return Ok(resultFinal);
        }

        /// <summary>
        /// Retrieves events by the organizer's ID.
        /// </summary>
        /// <param name="organizerId">The organizer's ID.</param>
        /// <returns>A list of events for the given organizer.</returns>
        [HttpGet("get-event-by-organizer-id")]
        public async Task<IActionResult> GetEventByOrganizerIdAsync([FromQuery] string organizerId)
        {
            var result = await _eventService.GetEventByOrganizerIdAsync(organizerId);

            var resultFinal = new
            {
                TotalCount = result.Count(),
                Events = result
            };
            return Ok(resultFinal);
        }

        /// <summary>
        /// Retrieves events by city and country.
        /// </summary>
        /// <param name="city">The city name.</param>
        /// <param name="country">The country name.</param>
        /// <returns>A list of events in the specified city and country.</returns>
        [HttpGet("get-event-by-city-and-country")]
        public async Task<IActionResult> GetEventByCityandCountryAsync([FromQuery] string city, string country)
        {
            var result = await _eventService.GetEventByCityandCountryAsync(city, country);
            var resultFinal = new
            {
                TotalCount = result.Count(),
                Events = result
            };
            return Ok(resultFinal);
        }

        /// <summary>
        /// Retrieves events by country.
        /// </summary>
        /// <param name="country">The country name.</param>
        /// <returns>A list of events in the specified country.</returns>
        [HttpGet("get-event-by-country")]
        public async Task<IActionResult> GetEventByCountryAsync([FromQuery] string country)
        {
            var result = await _eventService.GetEventByCountryAsync(country);
            var resultFinal = new
            {
                TotalCount = result.Count(),
                Events = result
            };
            return Ok(resultFinal);
        }

        /// <summary>
        /// Retrieves events by city.
        /// </summary>
        /// <param name="city">The city name.</param>
        /// <returns>A list of events in the specified city.</returns>
        [HttpGet("get-event-by-city")]
        public async Task<IActionResult> GetEventByCityAsync([FromQuery] string city)
        {
            var result = await _eventService.GetEventByCityAsync(city);
            var resultFinal = new
            {
                TotalCount = result.Count(),
                Events = result
            };
            return Ok(resultFinal);
        }
    }
}
