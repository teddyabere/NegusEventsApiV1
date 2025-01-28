using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NegusEventsApi.DTO;
using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.Ticket;
using NegusEventsApi.Models.User;
using NegusEventsApi.Redis;
using System.Net.Sockets;

namespace NegusEventsApi.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;

        public TicketService(ITicketRepository ticketRepository, IUserRepository userRepository, IEventRepository eventRepository, IRedisCacheService redisCacheService)
        {
            _ticketRepository = ticketRepository;
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _redisCacheService = redisCacheService;
        }

        public async Task<IEnumerable<Tickets>> GetAllTicketsAsync()
        {
            return await _ticketRepository.GetAllTicketsAsync();
        }
        public async Task<IEnumerable<Tickets>> GetAllTicketsByEventIdAsync(string eventId)
        {
            return await _ticketRepository.GetAllTicketsByEventIdAsync(eventId);
        }
        public async Task<IEnumerable<Tickets>> GetReservedTicketsByEventIdAsync(string eventId)
        {
            return await _ticketRepository.GetReservedTicketsByEventIdAsync(eventId);
        }
        public async Task<IEnumerable<Tickets>> GetConfirmedTicketsByEventIdAsync(string eventId)
        {
            return await _ticketRepository.GetConfirmedTicketsByEventIdAsync(eventId);
        }
        public async Task<IEnumerable<Tickets>> GetCancelledTicketsByEventIdAsync(string eventId)
        {
            return await _ticketRepository.GetCancelledTicketsByEventIdAsync(eventId);
        }
        public async Task<Tickets> GetTicketByIdAsync(string id)
        {
            return await _ticketRepository.GetTicketByIdAsync(id);
        }


        public async Task<bool> CreateTicketReservationAsync(Tickets ticket, string attendeeId, string attendeeName, string attendeeEmail)
        {
            var eventId = ticket.Event.EventId;
            //var userId = attendeeId;
            var existingReservation = await _ticketRepository.CheckReservedTicketByUserIdAsync(attendeeId, eventId);
            if (existingReservation != null)
                throw new Exception("User already has a pending reservation for this event.");


            var checkEvent = await _eventRepository.GetEventByIdAsync(eventId);
            if (checkEvent == null) {
                throw new Exception("Event is not available");
            }

          
            var remainingTickets = await _redisCacheService.DecrementAvailableTicketsAsync(eventId, ticket.TicketQuantity);

            if (remainingTickets < 0)
            {
                await _redisCacheService.IncrementAvailableTicketsAsync(eventId);
                throw new Exception("Tickets are sold out.");
            }

            ticket.Event = new Event
            {
                EventId = checkEvent.Id,
                Name = checkEvent.Name,
                StartDate = checkEvent.StartDate,
                OrganizerId = checkEvent.Organizer.OrganizerId,
                Country = checkEvent.Location.Country
            };

            ticket.Attendee = new Attendee
            {
                AttendeeId = attendeeId,
                Name = attendeeName
            };
            ticket.Status = TicketStatus.Reserved;
            ticket.IsPaid = false;
            ticket.CreatedAt = DateTime.Now.ToString();
            ticket.UpdatedAt = DateTime.Now.ToString();
            ticket.Payments = new Payments();

            await _ticketRepository.CreateTicketAsync(ticket);
            await _redisCacheService.ReserveTicketAsync(attendeeId, eventId);

            return true;
        }

        public async Task<bool> ConfirmReservationAsync(string userId, string ticketId, Int32 amountPaid)
        {
            var existingTicket = await GetTicketByIdAsync(ticketId);
            if (existingTicket == null || existingTicket.Attendee.AttendeeId != userId)
            {
                throw new Exception("Ticket is not available");
            }
            var existingEvent = await _eventRepository.GetEventByIdAsync(existingTicket.Event.EventId);
            if (existingEvent == null) {
                throw new Exception("Event is not available");
            }
            if (await _redisCacheService.HasUserPurchasedTicketAsync(userId, existingEvent.Id))
                throw new Exception("User has already purchased a ticket for this event.");
            
            var checkReservationKey = await _redisCacheService.CheckReservationAsync(userId, existingTicket.Event.EventId);
            if (!checkReservationKey)
                throw new Exception("Reservation expired or does not exist.");

            var existingReservation = await _ticketRepository.CheckReservedTicketByUserIdAsync(userId, existingEvent.Id);
            if (existingReservation == null)
                throw new Exception("User already has no pending reservation for this event.");

            var ticketsType = existingEvent.TicketTypes;
            if (ticketsType == null)
                throw new Exception("Ticket Type not Found.");

            if ( amountPaid != existingTicket.TicketQuantity * ticketsType.Price)
            {
                throw new Exception("Payment amount is not Correct.");
            }

            var ticketAvailable = existingEvent.QuantityAvailable - existingTicket.TicketQuantity;
            var ticketSold = existingEvent.QuantitySold + existingTicket.TicketQuantity;

            if (ticketAvailable < 0 || ticketSold > existingEvent.MaximumAttendees)
                throw new Exception("Tickets are sold out.");

            existingEvent.QuantitySold += existingTicket.TicketQuantity;
            existingEvent.QuantityAvailable -= existingTicket.TicketQuantity;

            existingTicket.Status = TicketStatus.Confirmed;
            existingTicket.IsPaid = true;
            existingTicket.UpdatedAt = DateTime.Now.ToString();
            if (existingTicket.Payments == null)
            {
                existingTicket.Payments = new Payments();
            }

            existingTicket.Payments.AmountPaid = amountPaid;
            existingTicket.Payments.PaymentMethod = "Card";
            existingTicket.Payments.PaymentStatus = PaymentStatus.Paid.ToString();
            existingTicket.Payments.PaymentDate = DateTime.Now.ToString();


            await _ticketRepository.UpdateTicketAsync(ticketId, existingTicket);

            var attendeeData = await _userRepository.GetAttendeeByIdAsync(existingTicket.Attendee.AttendeeId);
            if (attendeeData == null)
                throw new Exception($"Attendee with ID {existingTicket.Attendee.AttendeeId} not found.");

            if (attendeeData.Tickets == null)
                attendeeData.Tickets = new List<TicketDetail>();

            var ticketDetail = new TicketDetail
            {
                TicketId = existingTicket.Id,
                TicketType = existingTicket.TicketType,
                AmountPaid = existingTicket.Payments.AmountPaid,
                EventName = existingTicket.Event.Name
            };
            attendeeData.Tickets.Add(ticketDetail);

            await _userRepository.UpdateUserAsync(attendeeData.Id, attendeeData);

            await _eventRepository.UpdateEventAsync(existingEvent.Id, existingEvent);

           
            await _redisCacheService.MarkUserTicketPurchasedAsync(userId, existingEvent.Id);

            return true;
        }
        public async Task CleanupExpiredReservationsAsync()
        {
            var tobeExpiredReservation = await _ticketRepository.CheckExpiredTicketByUserIdAsync();
            if (tobeExpiredReservation == null)
                throw new Exception("User already has no pending reservation for this event.");

            foreach (var ticket in tobeExpiredReservation)
            {
                
                ticket.Status = TicketStatus.Cancelled;
                ticket.UpdatedAt = DateTime.Now.ToString();

                await _redisCacheService.IncrementAvailableTicketsAsync(ticket.Event.EventId);
                await _ticketRepository.UpdateTicketAsync(ticket.Id, ticket);
            }
        }

        public async Task<bool> CleanupExpiredReservationsRedisAsync(string attendeeId)
        {
            var abc = await _redisCacheService.CleanupExpiredReservationsAsync(attendeeId);
            if (abc)
                return true;
            return false;
        }


        public async Task UpdateTicketAsync(string id, string userId, Tickets ticket)
        {
            var existingTicket = await GetTicketByIdAsync(id);
            if (existingTicket == null || existingTicket.Attendee.AttendeeId != userId)
            {
                throw new Exception("Ticket is not available");
            }

            ticket.UpdatedAt = DateTime.Now.ToString();
            await _ticketRepository.UpdateTicketAsync(id, ticket);
        }

        public async Task CancelTicketAsync(string id, string userId)
        {
            var existingTicket = await GetTicketByIdAsync(id);
            if (existingTicket == null || existingTicket.Attendee.AttendeeId != userId)
            {
                throw new Exception("Ticket is not available");
            }
            if (existingTicket.Status == TicketStatus.Confirmed)
            {
                throw new Exception("Ticket can not be cancelled");
            }
            existingTicket.Status = TicketStatus.Cancelled;

            existingTicket.UpdatedAt = DateTime.Now.ToString();
            await _ticketRepository.UpdateTicketAsync(id, existingTicket);
        }

        public async Task DeleteTicketAsync(string id)
        {
            await _ticketRepository.DeleteTicketAsync(id);
        }
        public async Task<bool> CancelReservationAsync(string userId, string eventId)
        {
            if (!await _redisCacheService.HasUserPurchasedTicketAsync(userId, eventId))
                throw new Exception("No ticket found for the user.");

            await _redisCacheService.IncrementAvailableTicketsAsync(eventId);
            return true;
        }
        public async Task<List<Tickets>> GetReservedTicketsAsync(string userId)
        {
            await CleanupExpiredReservationsAsync();
            await CleanupExpiredReservationsRedisAsync(userId);

            var tickets = await _ticketRepository.GetReservedTicketsByUserAsync(userId);

            if (tickets == null || !tickets.Any())
                throw new Exception("No reserved tickets found for this user.");

            return tickets;
        }
        public async Task<List<Tickets>> GetConfirmedTickets(string userId)
        {
            var tickets = await _ticketRepository.GetConfirmedTickets(userId);

            if (tickets == null || !tickets.Any())
                throw new Exception("No Confirmed tickets found for this user.");

            return tickets;
        }


        public async Task<List<Tickets>> GetValidReservedTicketsAsync(string attendeeId)
        {
            var reservedTickets = await _redisCacheService.GetValidReservedTicketsAsync(attendeeId);

            return reservedTickets;
        }
        //public async Task<List<Tickets>> GetConfirmedFromRedisTickets(string attendeeId)
        //{
        //    var reservedTickets = await _ticketRepository.GetConfirmedTicketsRedisAsync(attendeeId);

        //    return reservedTickets;
        //}

        private static string GenerateTicketNumber(string eventCategory, int eventStartYear, string ticketRefereceNumber)
        {
            var negusName = "NEG";
            return $"{negusName}-{eventCategory}-{eventStartYear}- {ticketRefereceNumber}";
        }

    }
}
