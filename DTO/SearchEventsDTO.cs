namespace NegusEventsApi.DTO
{
    public class SearchEventsDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinAttendees { get; set; }
        public int? MaxAttendees { get; set; }
        public string? Status { get; set; }
        public string? Category { get; set; }
    }

}
