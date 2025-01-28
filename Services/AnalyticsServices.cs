using MongoDB.Bson;
using NegusEventsApi.DTO;
using NegusEventsApi.Models;

namespace NegusEventsApi.Services
{
    public class AnalyticsServices : IAnalyticsServices
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        public AnalyticsServices(IAnalyticsRepository analyticsRepository) { 
            _analyticsRepository = analyticsRepository;
        }
        public async Task<AnalyticsDto> GetAdminAnalyticsAsync()
        {
            return await _analyticsRepository.GetAdminAnalyticsAsync();
        }
        //public async Task<AnalyticsDto> GetOrganizerAnalyticsAsync(string organizerId)
        //{
        //    return await _analyticsRepository.GetOrganizerAnalyticsAsync(organizerId);
        //}
        public async Task<int> GetTotalTicketsSoldAsync()
        {
            return await _analyticsRepository.GetTotalTicketsSoldAsync();
        }
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _analyticsRepository.GetTotalRevenueAsync();
        }
        public async Task<Dictionary<string, int>> GetEventsByCategoryAsync()
        {
            return await _analyticsRepository.GetEventsByCategoryAsync();
        }
        public async Task<Dictionary<string, int>> GetEventsByLocationAsync()
        {
            return await _analyticsRepository.GetEventsByLocationAsync();
        }
        public async Task<Dictionary<string, double>> GetAverageRatingByEventAsync()
        {
            return await _analyticsRepository.GetAverageRatingByEventAsync();
        }
        //public async Task<Dictionary<string, decimal>> GetRevenueByOrganizerAsync()
        //{
        //    return await _analyticsRepository.GetRevenueByOrganizerAsync();
        //}
        public async Task<int> GetTicketsSoldForOrganizerAsync(string organizerId)
        {
            return await _analyticsRepository.GetTicketsSoldForOrganizerAsync(organizerId);
        }
        public async Task<int> GetRevenueForOrganizerAsync(string organizerId)
        {
            return await _analyticsRepository.GetRevenueForOrganizerAsync(organizerId);
        }
        public async Task<Dictionary<string, int>> GetAttendeeStatisticsForOrganizerAsync(string organizerId)
        {
            return await _analyticsRepository.GetAttendeeStatisticsForOrganizerAsync(organizerId);
        }

        public async Task<List<BsonDocument>> GetSoldTicketsByEventAsync()
        {
            return await _analyticsRepository.GetSoldTicketsByEventAsync();
        }

        public async Task<List<BsonDocument>> GetSoldTicketsByOrganizerAsync()
        {
            return await _analyticsRepository.GetSoldTicketsByOrganizerAsync();
        }

        public async Task<Dictionary<string, int>> GetEventsByCityAsync()
        {
            return await _analyticsRepository.GetEventsByCityAsync();
        }

        public async Task<Dictionary<string, int>> GetEventsByCountryAsync()
        {
            return await _analyticsRepository.GetEventsByCountryAsync();
        }

        public async Task<Dictionary<string, int>> GetOrganizersByCountryAsync()
        {
            return await _analyticsRepository.GetOrganizersByCountryAsync();
        }

        public async Task<Dictionary<string, int>> GetNumberOfAttendeesByCountryAsync()
        {
            return await _analyticsRepository.GetNumberOfAttendeesByCountryAsync();
        }

        public async Task<List<BsonDocument>> GetEventsByOrganizerAsync()
        {
            return await _analyticsRepository.GetEventsByOrganizerAsync();
        }

        public async Task<Dictionary<string, int>> GetTop5CitiesWithMostEventsAsync()
        {
            return await _analyticsRepository.GetTop5CitiesWithMostEventsAsync();
        }

        public async Task<List<BsonDocument>> GetTop10EventsByRatingAsync()
        {
            return await _analyticsRepository.GetTop10EventsByRatingAsync();
        }

        public async Task<Dictionary<string, double>> GetTop5EventsByRatingAsync()
        {
            return await _analyticsRepository.GetTop5EventsByRatingAsync();
        }
    }
}
