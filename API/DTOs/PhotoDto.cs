namespace API.DTOs;

public class PhotoDto
{
    public int Id { get; set; }
    public string? Url { get; set; }
    public bool IsMain { get; set; }
    // PHOTO MANAGEMENT TASK
    public bool isApproved { get; set; }
}