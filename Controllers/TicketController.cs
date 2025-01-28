using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.Ticket;
using NegusEventsApi.Redis;
using NegusEventsApi.Services;
using System.Security.Claims;

namespace NegusEventsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IEventService _eventService;
        private readonly IRedisCacheService _redisCacheService;

        public TicketController(ITicketService ticketService, IEventService eventService, IRedisCacheService redisCacheService)
        {
            _ticketService = ticketService;
            _eventService = eventService;
            _redisCacheService = redisCacheService;
        }

        /// <summary>
        /// Creates a ticket reservation for the specified event.
        /// </summary>
        /// <param name="ticket">The ticket details, including the associated event.</param>
        /// <returns>
        /// A status indicating the result of the reservation:
        /// - <see cref="OkObjectResult"/>: If the reservation is successful.
        /// - <see cref="BadRequestObjectResult"/>: If tickets are sold out or the event is invalid.
        /// - <see cref="UnauthorizedObjectResult"/>: If attendee information is not found in the token.
        /// </returns>

        [HttpPost("create-ticket-reservation")]
        [Authorize(Roles = "Attendee")]
        public async Task<IActionResult> CreateTicketReservation([FromBody] Tickets ticket)
        {
            var eventCheck = await _eventService.GetEventByIdAsync(ticket.Event.EventId);
            if (eventCheck == null)
            {
                return BadRequest("Selected Event is not correct.");
            }

            var attendeeId = User.FindFirst("UserId")?.Value;
            var attendeeName = User.FindFirst("Name")?.Value;
            var attendeeEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(attendeeId) || string.IsNullOrEmpty(attendeeEmail))
            {
                return Unauthorized("Attendee information not found in the token.");
            }

            await _ticketService.CreateTicketReservationAsync(ticket, attendeeId, attendeeName, attendeeEmail);

            return Ok("Ticket reserved successfully!");
        }

        /// <summary>
        /// Confirms a ticket reservation and processes the payment.
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket reservation to confirm.</param>
        /// <param name="amountPaid">The payment amount for the reservation.</param>
        /// <returns>
        /// A status indicating the result of the confirmation:
        /// - <see cref="OkObjectResult"/>: If the ticket purchase is successful.
        /// - <see cref="BadRequestObjectResult"/>: If the confirmation fails or an error occurs.
        /// </returns>
        [HttpPost("confirm-ticket-reservation")]
        [Authorize(Roles = "Attendee")]
        public async Task<IActionResult> ConfirmTicketReservation(string ticketId, Int32 amountPaid)
        {
            try
            {
                var attendeeId = User.FindFirst("UserId")?.Value;
                var result = await _ticketService.ConfirmReservationAsync(attendeeId, ticketId, amountPaid);

                if (result)
                {
                    return Ok("Ticket purchased successfully!");
                }

                return BadRequest("Reservation confirmation failed.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        /// <summary>
        /// Cancels a ticket reservation for the specified ticket and event.
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket to cancel.</param>
        /// <param name="eventId">The unique identifier of the event associated with the ticket.</param>
        /// <returns>
        /// A status indicating the result of the cancellation:
        /// - <see cref="OkObjectResult"/>: If the cancellation is successful.
        /// - <see cref="BadRequestObjectResult"/>: If the cancellation fails.
        /// </returns>
        [HttpPost("cancel-ticket-reservation")]
        [Authorize(Roles = "Attendee")]
        public async Task<IActionResult> CancelTicketReservation(string ticketId, string eventId)
        {
            try
            {
                var attendeeId = User.FindFirst("UserId")?.Value;
                var result = await _ticketService.CancelReservationAsync(attendeeId, eventId);

                if (result)
                {
                    return Ok("Ticket cancelled successfully!");
                }

                return BadRequest("Failed to cancel the reservation.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Updates the details of a ticket for the specified ticket ID.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket to update.</param>
        /// <param name="ticket">The updated ticket details.</param>
        /// <returns>
        /// A status indicating the result of the update:
        /// - <see cref="NoContentResult"/>: If the update is successful.
        /// - <see cref="NotFoundObjectResult"/>: If the ticket is not found.
        /// </returns>
        [HttpPut("update-ticket/{id}")]
        [Authorize(Roles = "Attendee")]
        public async Task<IActionResult> UpdateTicket(string id, Tickets ticket)
        {
            var attendeeId = User.FindFirst("UserId")?.Value;
            var existingTicket = await _ticketService.GetTicketByIdAsync(id);

            if (existingTicket == null)
            {
                return NotFound("Ticket not found.");
            }

            await _ticketService.UpdateTicketAsync(id, attendeeId, ticket);
            return NoContent();
        }

        /// <summary>
        /// Cancels a ticket booking for the specified ticket ID.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket booking to cancel.</param>
        /// <returns>
        /// A status indicating the result of the cancellation:
        /// - <see cref="NoContentResult"/>: If the cancellation is successful.
        /// </returns>
        [HttpPut("cancel-booking/{id}")]
        [Authorize(Roles = "Attendee")]
        public async Task<IActionResult> CancelBooking(string id)
        {
            var attendeeId = User.FindFirst("UserId")?.Value;
            await _ticketService.CancelTicketAsync(id, attendeeId);
            return NoContent();
        }
        /// <summary>
        /// Deletes a ticket by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket to delete.</param>
        /// <returns>
        /// A status indicating the result of the deletion:
        /// - <see cref="NoContentResult"/>: If the deletion is successful.
        /// - <see cref="NotFoundObjectResult"/>: If the ticket is not found.
        /// </returns>
        [HttpDelete("delete-ticket/{id}")]
        public async Task<IActionResult> DeleteTicket(string id)
        {
            var existingTicket = await _ticketService.GetTicketByIdAsync(id);

            if (existingTicket == null)
            {
                return NotFound("Ticket not found.");
            }

            await _ticketService.DeleteTicketAsync(id);
            await _redisCacheService.RemoveCacheAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Retrieves all tickets associated with a specific event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>
        /// A list of tickets for the specified event, along with the total count:
        /// - <see cref="OkObjectResult"/>: Contains the total count and ticket details.
        /// </returns>
        [HttpGet("get-tickets-by-event-id/{eventId}")]
        public async Task<IActionResult> GetTicketsByEventId(string eventId)
        {
            var tickets = await _ticketService.GetAllTicketsByEventIdAsync(eventId);
            return Ok(new { TotalCount = tickets.Count(), Tickets = tickets });
        }

        /// <summary>
        /// Retrieves all confirmed tickets associated with a specific event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>
        /// A list of tickets for the specified event, along with the total count:
        /// - <see cref="OkObjectResult"/>: Contains the total count and ticket details.
        /// </returns>
        [HttpGet("get-confirmed-tickets-by-event-id/{eventId}")]
        public async Task<IActionResult> GetConfirmedTicketsByEventId(string eventId)
        {
            var tickets = await _ticketService.GetConfirmedTicketsByEventIdAsync(eventId);
            return Ok(new { TotalCount = tickets.Count(), Tickets = tickets });
        }

        /// <summary>
        /// Retrieves all reserved tickets associated with a specific event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>
        /// A list of tickets for the specified event, along with the total count:
        /// - <see cref="OkObjectResult"/>: Contains the total count and ticket details.
        /// </returns>
        [HttpGet("get-reserved-tickets-by-event-id/{eventId}")]
        public async Task<IActionResult> GetReservedTicketsByEventId(string eventId)
        {
            var tickets = await _ticketService.GetReservedTicketsByEventIdAsync(eventId);
            return Ok(new { TotalCount = tickets.Count(), Tickets = tickets });
        }

        /// <summary>
        /// Retrieves all cancelled tickets associated with a specific event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>
        /// A list of tickets for the specified event, along with the total count:
        /// - <see cref="OkObjectResult"/>: Contains the total count and ticket details.
        /// </returns>
        [HttpGet("get-cancelled-tickets-by-event-id/{eventId}")]
        public async Task<IActionResult> GetCancelledTicketsByEventId(string eventId)
        {
            var tickets = await _ticketService.GetCancelledTicketsByEventIdAsync(eventId);
            return Ok(new { TotalCount = tickets.Count(), Tickets = tickets });
        }

        /// <summary>
        /// Retrieves specific ticket associated with ticket id.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket.</param>
        /// <returns>
        /// Tickets for the specified ticket, along with the total count:
        /// - <see cref="OkObjectResult"/>: Contains the ticket detail.
        /// </returns>
        [HttpGet("get-ticket-by-id/{id}")]
        public async Task<IActionResult> GetTicketById(string id)
        {
            var cacheKey = $"ticket:{id}";
            var cachedTicket = await _redisCacheService.GetCacheAsync<Tickets>(cacheKey);

            if (cachedTicket != null)
            {
                return Ok(cachedTicket);
            }

            var ticket = await _ticketService.GetTicketByIdAsync(id);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            await _redisCacheService.SetCacheAsync(cacheKey, ticket, TimeSpan.FromMinutes(10));
            return Ok(ticket);
        }

        /// <summary>
        /// Retrieves all reserved tickets associated with a specific user from redis.
        /// </summary>
        /// <param name="attendeeId">The unique identifier of the user.</param>
        /// <returns>
        /// A list of tickets for the specified event, along with the total count:
        /// - <see cref="OkObjectResult"/>: Contains the total count and ticket details.
        /// </returns>
        [HttpGet("get-reserved-tickets-by-user-id")]
        [Authorize(Roles = "Attendee")]
        public async Task<IActionResult> GetReservedTicketsByUser()
        {
            try
            {
                var attendeeId = User.FindFirst("UserId")?.Value;
                var tickets = await _ticketService.GetReservedTicketsAsync(attendeeId);
                return Ok(new { TotalCount = tickets.Count(), Tickets = tickets });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all confirmed tickets associated with a specific user from redis.
        /// </summary>
        /// <param name="attendeeId">The unique identifier of the user.</param>
        /// <returns>
        /// A list of tickets for the specified event, along with the total count:
        /// - <see cref="OkObjectResult"/>: Contains the total count and ticket details.
        /// </returns>
        [HttpGet("get-confirmed-tickets-by-user-id")]
        [Authorize(Roles = "Attendee")]
        public async Task<IActionResult> GetConfirmedTicketsByUser()
        {
            try
            {
                var attendeeId = User.FindFirst("UserId")?.Value;
                var tickets = await _ticketService.GetConfirmedTickets(attendeeId);
                return Ok(new { TotalCount = tickets.Count(), Tickets = tickets });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all available tickets associated with a specific event from redis.
        /// </summary>
        /// <param name="eventId">The unique identifier of the user.</param>
        /// <returns>
        /// Available number of tickets for the specified event,:
        /// - <see cref="OkObjectResult"/>: Contains the event id and availble number of ticket.
        /// </returns>
        [HttpGet("get-available-tickets-by-event-id-redis/{eventId}")]
        public async Task<IActionResult> GetAvailableTickets(string eventId)
        {
            try
            {
                var availableTickets = await _redisCacheService.GetAvailableTicketsAsync(eventId);
                return Ok(new { EventId = eventId, AvailableTickets = availableTickets });
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}

