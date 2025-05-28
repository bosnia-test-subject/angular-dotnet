namespace API.DTOs
{
    public class PhotoStatsDto
    {
        public required string Username { get; set; }
        public int ApprovedPhotos { get; set; }
        public int UnapprovedPhotos { get; set; }
    }
}