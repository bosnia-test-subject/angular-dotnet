namespace API.DTOs;

public class PhotoDto
{
    public int Id { get; set; }
    public string? Url { get; set; }
    public bool IsMain { get; set; }
    public bool isApproved { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
}