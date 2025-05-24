namespace API.Entities
{
    public class PhotoTag
    {
        public int PhotoId { get; set; }
        public required Photo Photo { get; set; }
        public int TagId { get; set; }
        public required Tag Tag { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}