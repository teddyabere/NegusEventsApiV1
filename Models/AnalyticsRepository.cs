using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using NegusEventsApi.DTO;
using NegusEventsApi.Models.Event;
using NegusEventsApi.Models.Feedback;
using NegusEventsApi.Models.Ticket;
using NegusEventsApi.Models.User;
using System.Diagnostics.CodeAnalysis;

namespace NegusEventsApi.Models
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly IMongoCollection<Events> _events;
        private readonly IMongoCollection<Tickets> _tickets;
        private readonly IMongoCollection<Feedbacks> _feedback;
        private readonly IMongoCollection<Users> _users;
        //private readonly IMongoCollection<Payments> _payments;


        public AnalyticsRepository(IOptions<NegusEventsDbSettings> negusDbSettings)
        {
            var client = new MongoClient(negusDbSettings.Value.ConnectionString);
            var database = client.GetDatabase(negusDbSettings.Value.DatabaseName);
            _events = database.GetCollection<Events>("Event");
            _tickets = database.GetCollection<Tickets>("Ticket");
            _feedback = database.GetCollection<Feedbacks>("Feedback");
            _users = database.GetCollection<Users>("User");

        }

        public async Task<int> GetTotalTicketsSoldAsync()
        {
            return (int)await _tickets.CountDocumentsAsync(new BsonDocument());
        }
        public async Task<decimal> GetTotalRevenueAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                   { "_id", BsonNull.Value },
                    { "totalRevenue", new BsonDocument("$sum", "$payments.amountPaid") }
                })
            };
            var result = await _tickets.AggregateAsync<BsonDocument>(pipeline);
            var totalRevenue = result.FirstOrDefault()?["totalRevenue"].AsDecimal;

            return totalRevenue ?? 0;
        }

        public async Task<Dictionary<string, int>> GetEventsByCategoryAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray
                {
                    new BsonDocument("$type", "$category"), "string"
                }))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument("$ifNull", new BsonArray { "$category", "Unknown" }) },
                    { "eventCount", new BsonDocument("$sum", 1) }
        })
            };

            try
            {
                var result = await _events.Aggregate<BsonDocument>(pipeline).ToListAsync();

                return result.ToDictionary(
                r => r["_id"].IsBsonNull ? "Unknown" : r["_id"].AsString,
                r => r["eventCount"].AsInt32);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetEventsByLocationAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$location.city" },
                    { "eventCount", new BsonDocument("$sum", 1) }
                })
            };
            var result = await _events.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.ToDictionary(
                r => r["_id"].IsBsonNull ? "Unknown" : r["_id"].AsString,
                r => r["eventCount"].AsInt32);

        }
        public async Task<Dictionary<string, double>> GetAverageRatingByEventAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$event.eventId" },
                    { "averageRating", new BsonDocument("$avg", "$rating") }
                })
            };
            var result = await _feedback.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.ToDictionary(
                r => r["_id"].IsBsonNull ? "Unknown" : $"eventId: {r["_id"].AsString}",
                r =>Math.Round( r["averageRating"].AsDouble, 2));
            
        }

        public async Task<AnalyticsDto> GetAdminAnalyticsAsync()
        {
            
            var totalTickets = await _tickets.CountDocumentsAsync(new BsonDocument());

            var revenuePipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                   { "_id", BsonNull.Value },
                    { "totalRevenue", new BsonDocument("$sum", "$payments.amountPaid") }
                })
            };
            var revenueResult = await _tickets.Aggregate<BsonDocument>(revenuePipeline).FirstOrDefaultAsync();
            var totalRevenue = revenueResult != null && revenueResult.Contains("totalRevenue") && revenueResult["totalRevenue"].IsNumeric
    ? revenueResult["totalRevenue"].ToDecimal() : 0;

            // var totalRevenue = revenueResult?["totalRevenue"].AsDecimal ?? 0;

            var categoryPipeline = new[]
            {
                 new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray{
                    new BsonDocument("$type", "category"), "string"
                }))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$category" },
                    { "eventCount", new BsonDocument("$sum", 1) }
                })
            };
            var categories = await _events.Aggregate<BsonDocument>(categoryPipeline).ToListAsync();

            var locationPipeline = new[]
            {
                 new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray{
                    new BsonDocument("$type", "location.country"), "string"
                }))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$location.country" },
                    { "location", new BsonDocument("$sum", 1) }
                })
            };
            var locations = await _events.Aggregate<BsonDocument>(locationPipeline).ToListAsync();

            return new AnalyticsDto
            {
                TotalTicketsSold = (int)totalTickets,
                TotalRevenue = totalRevenue,
                EventsByCategory = categories.ToDictionary(
                    c => c["_id"] == BsonNull.Value ? "Unknown" : c["_id"].AsString,
                    c => c["eventCount"].AsInt32),
                EventsByCountry = locations.ToDictionary(
                    l => l["_id"] == BsonNull.Value ? "Unknown" : l["_id"].AsString,
                    l => l["location"].AsInt32)
            };
        }

        public async Task<int> GetTicketsSoldForOrganizerAsync(string organizerId)
        {
            var pipeline = new[]
            {   
                new BsonDocument("$match", new BsonDocument("event.organizerId", organizerId)),
                new BsonDocument("$count", "totalTicketsSold")
            };
            try
            {
               
                var result = await _tickets.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

                return result?["totalTicketsSold"].AsInt32 ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred during aggregation: {ex.Message}");
                throw;
            }

        }
        public async Task<int> GetRevenueForOrganizerAsync(string organizerId)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("event.organizerId", organizerId)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "totalRevenue", new BsonDocument("$sum", "$payments.amountPaid") }
                })
            };

            try
            {
                var result = await _tickets.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

                return result?["totalRevenue"]?.AsInt32 ?? 0;
            }
            catch (Exception ex)
            {
                throw; 
            }

        }

        public async Task<Dictionary<string, int>> GetAttendeeStatisticsForOrganizerAsync(string organizerId)
        {
            var pipeline = new[]
            {
              new BsonDocument("$match", new BsonDocument("event.organizerId", organizerId)),

              new BsonDocument("$group", new BsonDocument{
                  { "_id", "$event.eventId" },
                  { "attendeeCount", new BsonDocument("$sum", 1) },
                  { "eventName", new BsonDocument("$first", "$event.name") }
              })
            };

            try
            {
                var result = await _tickets.Aggregate<BsonDocument>(pipeline).ToListAsync();
                return result.ToDictionary(
                    r => r["eventName"]?.AsString ?? "Unknown Event",
                    r => r["attendeeCount"]?.AsInt32 ?? 0
                );
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task<Dictionary<string, double>> GetFeedbackAvgRatingForOrganizerAsync(string organizerId)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("event.organizerId", organizerId)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$eventId" },  
                    { "averageRating", new BsonDocument("$avg", "$rating") }
                })
            };

            try
            {
                var result = await _feedback.Aggregate<BsonDocument>(pipeline).ToListAsync();
                                
                return result.ToDictionary(
                    r => r["_id"].IsBsonNull ? "Unknown" : r["_id"].AsString,
                    r => r["averageRating"].AsDouble);

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        
        public async Task<Dictionary<string, int>> GetNumberOfAttendeesByCountryAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("status", "Confirmed")),
                new BsonDocument("$addFields", new BsonDocument("country", 
                new BsonDocument("$ifNull", new BsonArray { "$event.country", "Unknown" }))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$country" },
                    { "totalAttendees", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("totalAttendees", -1))
            };
            var result = await _tickets.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.ToDictionary(
                r => r["_id"].AsString,
                r => r["totalAttendees"].AsInt32);
        }

        public async Task<List<BsonDocument>> GetEventsByOrganizerAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$organizer.name" },
                    { "events", new BsonDocument("$push", "$name") }
                })
            };
            return await _events.Aggregate<BsonDocument>(pipeline).ToListAsync();        }

        public async Task<Dictionary<string, int>> GetTop5CitiesWithMostEventsAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$location.city" },
                    { "totalEvents", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("totalEvents", -1)),
                new BsonDocument("$limit", 5)
            };
            var result = await _events.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.ToDictionary(
                    r => r["_id"].IsBsonNull ? "Unknown" : r["_id"].AsString,
                    r => r["totalEvents"].AsInt32);

        }

        public async Task<Dictionary<string, double>> GetTop10EventsByRatingAsync()
        {
            var pipeline = new[]
            {
                
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$event.eventId" },
                    { "avgRating", new BsonDocument("$avg", "$rating") },
                    { "eventName", new BsonDocument("$first", "$event.eventName") } 
                }),
                
                new BsonDocument("$sort", new BsonDocument("avgRating", -1)),
                new BsonDocument("$limit", 10)
            };
            var result = await _feedback.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return result.ToDictionary(
                r => r["_id"].IsBsonNull ? "Unknown" : $"eventId: {r["_id"].AsString}",
                r => Math.Round(r["avgRating"].AsDouble, 2));

        }


        public async Task<Dictionary<string, double>> GetTop5EventsByRatingAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$event.eventId" },
                    { "avgRating", new BsonDocument("$avg", "$rating") }
                }),
                new BsonDocument("$sort", new BsonDocument("avgRating", -1)),
                new BsonDocument("$limit", 5)
            };
            var result = await _feedback.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return result.ToDictionary(
                r => r["_id"].IsBsonNull ? "Unknown" : $"eventId: {r["_id"].AsString}",
                r => Math.Round(r["avgRating"].AsDouble, 2));
        }

        public async Task<List<BsonDocument>> GetSoldTicketsByEventAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("status", new BsonDocument("$type", "string"))),
                new BsonDocument("$match", new BsonDocument("status", "Confirmed")),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$event.eventId" },
                    { "totalSoldTickets", new BsonDocument("$sum", 1) },
                    { "eventName", new BsonDocument("$first", "$event.eventName") },

                }),
                new BsonDocument("$sort", new BsonDocument("totalSoldTickets", -1))
            };
            var result = await _tickets.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return result;
        }

        public async Task<Dictionary<string, int>> GetEventsByCountryAsync()
        {
            var pipeline = new[]
            {     
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$location.country" },
                    { "totalEvents", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("totalEvents", -1))
            };

            var result = await _events.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return result.ToDictionary(
                r => r["_id"].IsBsonNull ? "Unknown" : r["_id"].AsString,
                r => r["totalEvents"].AsInt32);
        }

        public async Task<Dictionary<string, int>> GetEventsByCityAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray
                {
                    new BsonDocument("$type", "location.city"), "string"
                }))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$location.city" },
                    { "eventCount", new BsonDocument("$sum", 1) }
                })
            };

            var result = await _events.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.ToDictionary(
                r => r["_id"].IsBsonNull ? "Unknown" : r["_id"].AsString,
                r => r["eventCount"].AsInt32);
        }

        public async Task<Dictionary<string, int>> GetOrganizersByCountryAsync()
        {
            var pipeline = new[]
            {   
                new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray
                {
                    new BsonDocument("$type", "location.country"), "string"
                }))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$location.country" },
                    { "eventCount", new BsonDocument("$sum", 1) }
                })
            };

            var result = await _events.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return result.ToDictionary(
                r => r["_id"].IsBsonNull ? "Unknown" : r["_id"].AsString,
                r => r["eventCount"].AsInt32);
        }

        public async Task<List<BsonDocument>> GetSoldTicketsByOrganizerAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("status", "Confirmed")),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$event.organizerId" },
                    { "totalSoldTickets", new BsonDocument("$sum", 1) }
                }),    
                new BsonDocument("$sort", new BsonDocument("totalSoldTickets", -1))
            };

            return await _tickets.Aggregate<BsonDocument>(pipeline).ToListAsync();
        }

    }
}
