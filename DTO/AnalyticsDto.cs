using MongoDB.Bson;

namespace NegusEventsApi.DTO
{
    public class AnalyticsDto
    {
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int EventCount { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<string, int> EventsByCategory { get; set; } = new();
        public List<BsonDocument> EventsByLocation { get; set; }
        //public Dictionary<string, decimal> RevenueByOrganizer { get; set; } = new();
    }

}
