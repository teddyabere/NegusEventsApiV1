using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NegusEventsApi.Models.Feedback;
using NegusEventsApi.Services;

namespace NegusEventsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Creates new feedback.
        /// </summary>
        /// <param name="feedback">The feedback object to create.</param>
        /// <returns>A response indicating the result of the creation.</returns>
        /// <response code="401">Unauthorized if attendee ID is not provided.</response>
        /// <response code="500">Internal server error if something goes wrong.</response>
        [HttpPost("create-feedback")]
        public async Task<IActionResult> CreateFeedback([FromBody] Feedbacks feedback)
        {
            try
            {
                //var attendeeId = User.FindFirst("UserId")?.Value;
                //if (attendeeId == null)
                //{
                //    return Unauthorized("Attendee ID is not authorised.");
                //}

                feedback.Attendee.AttendeeId = feedback.Attendee.AttendeeId;

                await _feedbackService.AddFeedbackAsync(feedback);
                return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.Id }, feedback);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates feedback by ID.
        /// </summary>
        /// <param name="id">The ID of the feedback to update.</param>
        /// <param name="feedback">The updated feedback object.</param>
        /// <returns>A response indicating the result of the update.</returns>
        /// <response code="404">Feedback not found.</response>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateFeedback(string id, [FromBody] Feedbacks feedback)
        {
            var existingFeedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (existingFeedback == null)
            {
                return NotFound();
            }
            await _feedbackService.UpdateFeedbackAsync(id, feedback);
            return NoContent();
        }

        /// <summary>
        /// Deletes feedback by ID.
        /// </summary>
        /// <param name="id">The ID of the feedback to delete.</param>
        /// <returns>A response indicating the result of the deletion.</returns>
        /// <response code="404">Feedback not found.</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFeedback(string id)
        {
            var existingFeedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (existingFeedback == null)
            {
                return NotFound();
            }
            await _feedbackService.DeleteFeedbackAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Retrieves all feedback.
        /// </summary>
        /// <returns>A list of all feedback.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Feedbacks>>> GetAllFeedback()
        {
            var feedbacks = await _feedbackService.GetAllFeedbackAsync();
            return Ok(feedbacks);
        }

        /// <summary>
        /// Retrieves feedback by ID.
        /// </summary>
        /// <param name="id">The ID of the feedback to retrieve.</param>
        /// <returns>The feedback with the specified ID.</returns>
        /// <response code="404">Feedback not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Feedbacks>> GetFeedbackById(string id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return Ok(feedback);
        }


        /// <summary>
        /// Retrieves the average rating for a specific event based on feedback.
        /// </summary>
        /// <param name="eventId">The ID of the event to get the average rating for.</param>
        /// <returns>The average rating for the specified event.</returns>
        [HttpGet("{eventId}/average-rating")]
        public async Task<IActionResult> GetAverageRating(string eventId)
        {
            var result = await _feedbackService.GetAverageRatingAsync(eventId);
            return Ok(new { EventId = eventId, 
                AverageRating = result });
        }
    }
}

