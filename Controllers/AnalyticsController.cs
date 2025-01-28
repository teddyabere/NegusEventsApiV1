using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NegusEventsApi.Redis;
using NegusEventsApi.Services;

namespace NegusEventsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsServices _analyticsService;

        public AnalyticsController(IAnalyticsServices analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Gets overall analytics for the admin.
        /// </summary>
        /// <returns>The overall analytics data.</returns>

        //[Authorize(Roles = "Admin")]
        [HttpGet("aggregation/admin/get-overall-aggregation")]
        public async Task<IActionResult> GetAdminAnalytics()
        {
            var analytics = await _analyticsService.GetAdminAnalyticsAsync();
            return Ok(analytics);
        }

        /// <summary>
        /// Gets the total number of tickets sold.
        /// </summary>
        /// <returns>The total number of tickets sold.</returns>

        //[Authorize(Roles = "Admin")]
        [HttpGet("aggregation/admin/get-total-tickets-sold")]
        public async Task<IActionResult> GetTotalTicketsSold()
        {
            var totalTickets = await _analyticsService.GetTotalTicketsSoldAsync();
            return Ok(totalTickets);
        }

        /// <summary>
        /// Gets the total revenue from all events.
        /// </summary>
        /// <returns>The total revenue from all events.</returns>

        //[Authorize(Roles = "Admin")]
        [HttpGet("aggregation/admin/total-revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var totalRevenue = await _analyticsService.GetTotalRevenueAsync();
            return Ok(totalRevenue);
        }

        /// <summary>
        /// Gets the events categorized by type.
        /// </summary>
        /// <returns>The events grouped by category.</returns>

        //[Authorize(Roles = "Admin")]
        [HttpGet("aggregation/admin/events-by-category")]
        public async Task<IActionResult> GetEventsByCategory()
        {
            var eventsByCategory = await _analyticsService.GetEventsByCategoryAsync();
            return Ok(eventsByCategory);
        }

        /// <summary>
        /// Gets the events categorized by location.
        /// </summary>
        /// <returns>The events grouped by location.</returns>

        // [Authorize(Roles = "Admin")]
        [HttpGet("aggregation/admin/events-by-location")]
        public async Task<IActionResult> GetEventsByLocation()
        {
            var eventsByLocation = await _analyticsService.GetEventsByLocationAsync();
            return Ok(eventsByLocation);
        }

        /// <summary>
        /// Gets the average rating for each event.
        /// </summary>
        /// <returns>The average rating for each event.</returns>

        //[Authorize(Roles = "Admin")]
        [HttpGet("aggregation/admin/average-rating-by-event")]
        public async Task<IActionResult> GetAverageRatingByEvent()
        {
            var averageRatingByEvent = await _analyticsService.GetAverageRatingByEventAsync();
            return Ok(averageRatingByEvent);
        }

        /// <summary>
        /// Gets the total tickets sold by a specific organizer.
        /// </summary>
        /// <param name="organizerId">The ID of the organizer.</param>
        /// <returns>The total number of tickets sold by the organizer.</returns>

        //[Authorize(Roles = "Organizer")]
        [HttpGet("aggregation/organizer/get-tickets-sold-by-organizer-id")]
        public async Task<IActionResult> GetTicketsSold([FromQuery] string organizerId)
        {
            var ticketsSold = await _analyticsService.GetTicketsSoldForOrganizerAsync(organizerId);
            return Ok(ticketsSold);
        }

        /// <summary>
        /// Gets the total revenue by a specific organizer.
        /// </summary>
        /// <param name="organizerId">The ID of the organizer.</param>
        /// <returns>The total revenue for the organizer.</returns>

        //[Authorize(Roles = "Organizer")]
        [HttpGet("aggregation/organizer/get-total-revenue-by-organizer-id")]
        public async Task<IActionResult> GetRevenue([FromQuery] string organizerId)
        {
            var revenue = await _analyticsService.GetRevenueForOrganizerAsync(organizerId);
            return Ok(revenue);
        }

        /// <summary>
        /// Gets attendee statistics for a specific organizer.
        /// </summary>
        /// <param name="organizerId">The ID of the organizer.</param>
        /// <returns>The attendee statistics for the organizer.</returns>

        //[Authorize(Roles = "Organizer")]
        [HttpGet("aggregation/organizer/get-attendee-statistics-by-organizer-id")]
        public async Task<IActionResult> GetAttendeeStatistics([FromQuery] string organizerId)
        {
            var attendeeStatistics = await _analyticsService.GetAttendeeStatisticsForOrganizerAsync(organizerId);
            return Ok(attendeeStatistics);
        }
       
        /// <summary>
        /// Gets events by city.
        /// </summary>
        /// <returns>The events that are hosted in various cities.</returns>

        [HttpGet("aggregation/get-events-by-city")]
        public async Task<IActionResult> GetEventsByCityAsync()
        {
            var attendeeStatistics = await _analyticsService.GetEventsByCityAsync();
            return Ok(attendeeStatistics);
        }

        /// <summary>
        /// Gets events by country.
        /// </summary>
        /// <returns>The events that are hosted in various countries.</returns>

        [HttpGet("aggregation/get-events-by-country")]
        public async Task<IActionResult> GetEventsByCountryAsync()
        {
            var attendeeStatistics = await _analyticsService.GetEventsByCountryAsync();
            return Ok(attendeeStatistics);
        }

        /// <summary>
        /// Gets the organizers by country.
        /// </summary>
        /// <returns>The organizers that are based in various countries.</returns>

        [HttpGet("aggregation/get-organizers-by-country")]
        public async Task<IActionResult> GetOrganizersByCountryAsync()
        {
            var attendeeStatistics = await _analyticsService.GetOrganizersByCountryAsync();
            return Ok(attendeeStatistics);
        }

        /// <summary>
        /// Gets the number of attendees by country.
        /// </summary>
        /// <returns>The number of attendees by country.</returns>

        [HttpGet("aggregation/get-number-of-attendees-by-country")]
        public async Task<IActionResult> GetNumberOfAttendeesByCountryAsync()
        {
            var attendeeStatistics = await _analyticsService.GetNumberOfAttendeesByCountryAsync();
            return Ok(attendeeStatistics);
        }

        /// <summary>
        /// Gets the top 5 cities with the most events.
        /// </summary>
        /// <returns>The top 5 cities with the most events.</returns>

        [HttpGet("aggregation/get-top5-cities-with-most-events")]
        public async Task<IActionResult> GetTop5CitiesWithMostEventsAsync()
        {
            var attendeeStatistics = await _analyticsService.GetTop5CitiesWithMostEventsAsync();
            return Ok(attendeeStatistics);
        }

        /// <summary>
        /// Gets the top 10 events each year by rating.
        /// </summary>
        /// <returns>The top 10 events each year based on ratings.</returns>

        [HttpGet("aggregation/get-top10-events-by-rating")]
        public async Task<IActionResult> GetTop10EventsByRatingAsync()
        {
            var attendeeStatistics = await _analyticsService.GetTop10EventsByRatingAsync();
            return Ok(attendeeStatistics);
        }

        /// <summary>
        /// Gets the top 5 events based on ratings.
        /// </summary>
        /// <returns>The top 5 events based on ratings.</returns>

        [HttpGet("aggregation/get-top5-events-by-rating")]
        public async Task<IActionResult> GetTop5EventsByRatingAsync()
        {
            var attendeeStatistics = await _analyticsService.GetTop5EventsByRatingAsync();
            return Ok(attendeeStatistics);
        }

    }
}
