namespace RoverSearch.Models;

public class Result
{
    public string Filename { get; set; } = string.Empty;
    public string Title { get; set; }
    public string Season { get; set; }
    public string Episode { get; set; }
    public string Description { get; set; }
    public int Score { get; set; }
}
