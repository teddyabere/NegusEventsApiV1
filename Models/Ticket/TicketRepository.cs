using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using NegusEventsApi.DTO;
using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.User;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NegusEventsApi.Models.Ticket
{
    public class TicketRepository : ITicketRepository
    {
        private readonly IMongoCollection<Tickets> _tickets;
        private readonly IMongoCollection<TicketNumberTracker> _ticketTracker;
        private readonly IDatabase _redisDatabase;
        private readonly IMongoCollection<Events> _eventCollection;
        private readonly IConnectionMultiplexer _redisConnection;

        public TicketRepository(IOptions<NegusEventsDbSettings> negusDbSettings, IDatabase redisDatabase, IConnectionMultiplexer redisConnection)
        {
            var client = new MongoClient(negusDbSettings.Value.ConnectionString);
            var database = client.GetDatabase(negusDbSettings.Value.DatabaseName);
            _tickets = database.GetCollection<Tickets>("Ticket");
            _ticketTracker = database.GetCollection<TicketNumberTracker>("TicketTracker");
            _eventCollection = database.GetCollection<Events>("Event");
            _redisDatabase = redisDatabase;
            _redisConnection = redisConnection;
        }

        public async Task<IEnumerable<Tickets>> GetAllTicketsAsync()
        {
            return await _tickets.Find(_ => true).ToListAsync();
        }
        public async Task<IEnumerable<Tickets>> GetAllTicketsByEventIdAsync(string eventId)
        {
            return await _tickets.Find(x => x.Event.EventId == eventId).ToListAsync();
        }
        public async Task<IEnumerable<Tickets>> GetReservedTicketsByEventIdAsync(string eventId)
        {
            return await _tickets.Find(x => x.Event.EventId == eventId && x.Status == TicketStatus.Reserved).ToListAsync();
        }
        public async Task<IEnumerable<Tickets>> GetConfirmedTicketsByEventIdAsync(string eventId)
        {
            return await _tickets.Find(x => x.Event.EventId == eventId && x.Status == TicketStatus.Confirmed).ToListAsync();

        }
        public async Task<IEnumerable<Tickets>> GetCancelledTicketsByEventIdAsync(string eventId)
        {
            return await _tickets.Find(x => x.Event.EventId == eventId && x.Status == TicketStatus.Cancelled).ToListAsync();

        }

        public async Task<Tickets> GetTicketByIdAsync(string id)
        {
            return await _tickets.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Tickets> CheckReservedTicketByUserIdAsync(string userId, string eventId)
        {
            var existingReservation = await _tickets
                .Find(t => t.Attendee.AttendeeId == userId && 
                t.Event.EventId == eventId && t.Status == TicketStatus.Reserved)
        .FirstOrDefaultAsync();
            return existingReservation;
        }

        public async Task<List<Tickets>> GetReservedTicketsByUserAsync(string userId)
        {
            return await _tickets
                .Find(t => t.Attendee.AttendeeId == userId && t.Status == TicketStatus.Reserved)
                .ToListAsync();
        }
        public async Task<List<Tickets>> GetConfirmedTickets(string userId)
        {
            return await _tickets
                .Find(t => t.Attendee.AttendeeId == userId && t.Status == TicketStatus.Confirmed)
                .ToListAsync();
        }

        public async Task<List<Tickets>> CheckExpiredTicketByUserIdAsync()
        {
            var allReservedTickets = await _tickets
                .Find(t => t.Status == TicketStatus.Reserved)
                .ToListAsync();

            var expiredTickets = allReservedTickets
                .Where(t =>
                {
                    if (DateTime.TryParse(t.CreatedAt, out DateTime createdAt))
                    {
                        return createdAt < DateTime.UtcNow.AddMinutes(-15);
                    }
                    return false;
                })
                .ToList();

            return expiredTickets;
        }



        public async Task CreateTicketAsync(Tickets ticket)
        {
            await _tickets.InsertOneAsync(ticket);
        }

        public async Task UpdateTicketAsync(string id, Tickets ticket)
        {
            await _tickets.ReplaceOneAsync(t => t.Id == id, ticket);
        }

        public async Task DeleteTicketAsync(string id)
        {
            await _tickets.DeleteOneAsync(t => t.Id == id);
        }

        public async Task<int> GetTotalAttendeesAsync(string eventId)
        {
            var pipeline = new[]
            {
            new BsonDocument("$match", new BsonDocument("eventId", eventId)),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$eventId" },
                { "totalAttendees", new BsonDocument("$sum", 1) }
            })
        };

            var result = await _tickets.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            return result != null ? result["totalAttendees"].AsInt32 : 0;
        }
        public async Task<List<EventPopularityDto>> GetEventPopularityAsync()
        {
            var pipeline = new[]
            {
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$eventId" },
            { "ticketsSold", new BsonDocument("$sum", 1) }
        }),
        new BsonDocument("$sort", new BsonDocument("ticketsSold", -1))
    };

            var result = await _tickets.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.Select(r => new EventPopularityDto
            {
                EventId = r["_id"].AsString,
                TicketsSold = r["ticketsSold"].AsInt32
            }).ToList();
        }

        public async Task<List<RevenueByTicketTypeDto>> GetRevenueByTicketTypeAsync(string eventId)
        {
            var pipeline = new[]
            {
        new BsonDocument("$match", new BsonDocument("eventId", eventId)),
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "payments" },
            { "localField", "_id" },
            { "foreignField", "ticketId" },
            { "as", "paymentDetails" }
        }),
        new BsonDocument("$unwind", "$paymentDetails"),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$ticketType" },
            { "totalRevenue", new BsonDocument("$sum", "$paymentDetails.amountPaid") }
        })
    };

            var result = await _tickets.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.Select(r => new RevenueByTicketTypeDto
            {
                TicketType = r["_id"].AsString,
                TotalRevenue = r["totalRevenue"].AsDecimal
            }).ToList();
        }

        // Ticket Numeber
        public async Task<TicketNumberTracker> GetTicketRefNumberAsync(string catagoryName, int year)
        {
            return await _ticketTracker.Find(t => t.EventCategory == catagoryName && t.Year == year).FirstOrDefaultAsync();
        }

        public async Task CreateTicketRefNumberAsync(TicketNumberTracker ticketNum)
        {
            await _ticketTracker.InsertOneAsync(ticketNum);
        }

        public async Task UpdateTicketRefNumberAsync(string id, int lastNumber)
        {
            var ticket =  _ticketTracker.Find(x => x.Id == id).FirstOrDefault();
            ticket.LastNumber = lastNumber;
            await _ticketTracker.ReplaceOneAsync(t => t.Id == id, ticket);
        }

        public async Task DeleteTicketRefNumberAsync(string id)
        {
            await _ticketTracker.DeleteOneAsync(t => t.Id == id);
        }

        


    }
    public class Ticket
    {
        public string UserId { get; set; }
        public string EventId { get; set; }
        public bool IsPurchased { get; set; }
    }

}
