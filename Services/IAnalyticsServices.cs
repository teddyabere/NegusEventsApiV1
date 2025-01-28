using MongoDB.Bson;
using NegusEventsApi.DTO;

namespace NegusEventsApi.Services
{
    public interface IAnalyticsServices
    {
        Task<AnalyticsDto> GetAdminAnalyticsAsync();
        Task<int> GetTotalTicketsSoldAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<Dictionary<string, int>> GetEventsByCategoryAsync();
        Task<Dictionary<string, int>> GetEventsByLocationAsync();
        Task<Dictionary<string, double>> GetAverageRatingByEventAsync();

        Task<int> GetTicketsSoldForOrganizerAsync(string organizerId);
        Task<int> GetRevenueForOrganizerAsync(string organizerId);
        Task<Dictionary<string, int>> GetAttendeeStatisticsForOrganizerAsync(string organizerId);
        Task<List<BsonDocument>> GetSoldTicketsByEventAsync();
        Task<List<BsonDocument>> GetSoldTicketsByOrganizerAsync();
        Task<Dictionary<string, int>> GetEventsByCityAsync();
        Task<Dictionary<string, int>> GetEventsByCountryAsync();

        Task<Dictionary<string, int>> GetOrganizersByCountryAsync();

        Task<Dictionary<string, int>> GetNumberOfAttendeesByCountryAsync();

        Task<List<BsonDocument>> GetEventsByOrganizerAsync();

        Task<Dictionary<string, int>> GetTop5CitiesWithMostEventsAsync();

        Task<List<BsonDocument>> GetTop10EventsByRatingAsync();

        Task<Dictionary<string, double>> GetTop5EventsByRatingAsync();
    }
}
