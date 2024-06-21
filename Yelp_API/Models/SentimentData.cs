namespace Yelp_API.Models;

public class SentimentData{
    [LoadColumn(0)]
    public string? SentimentText { get; set; }
}
