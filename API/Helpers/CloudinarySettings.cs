namespace API.Helpers;

public class CloudinarySettings
{
    public required string CloudName { get; set; }
    public required string ApiKey { get; set; }
    public required string ApiSecret { get; set; }
    public required string Folder { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public required  string Crop { get; set; }
    public required  string Gravity { get; set; }
}