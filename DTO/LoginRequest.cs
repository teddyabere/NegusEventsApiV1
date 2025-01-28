using System.ComponentModel.DataAnnotations;

namespace NegusEventsApi.DTO
{
    public class NLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

}
