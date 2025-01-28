using NegusEventsApi.Models.Feedback;

namespace NegusEventsApi.Services
{
    public interface IFeedbackService
    {
        Task<IEnumerable<Feedbacks>> GetAllFeedbackAsync();
        Task<Feedbacks> GetFeedbackByIdAsync(string id);
        Task AddFeedbackAsync(Feedbacks feedback);
        Task UpdateFeedbackAsync(string id, Feedbacks feedback);
        Task DeleteFeedbackAsync(string id);
        Task<double> GetAverageRatingAsync(string eventId);
    }
}
