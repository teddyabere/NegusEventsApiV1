using NegusEventsApi.Models.Feedback;

namespace NegusEventsApi.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<IEnumerable<Feedbacks>> GetAllFeedbackAsync()
        {
            return await _feedbackRepository.GetAllFeedbackAsync();
        }

        public async Task<Feedbacks> GetFeedbackByIdAsync(string id)
        {
            return await _feedbackRepository.GetFeedbackByIdAsync(id);
        }

        public async Task AddFeedbackAsync(Feedbacks feedback)
        {
            feedback.CreatedAt = DateTime.Now.ToString();
            feedback.UpdatedAt = DateTime.Now.ToString();
            await _feedbackRepository.AddFeedbackAsync(feedback);
        }

        public async Task UpdateFeedbackAsync(string id, Feedbacks feedback)
        {
            feedback.UpdatedAt = DateTime.Now.ToString();
            await _feedbackRepository.UpdateFeedbackAsync(id, feedback);
        }

        public async Task DeleteFeedbackAsync(string id)
        {
            await _feedbackRepository.DeleteFeedbackAsync(id);
        }
        public async Task<double> GetAverageRatingAsync(string eventId)
        {
            return await _feedbackRepository.GetAverageRatingAsync(eventId);
        }

    }
}
