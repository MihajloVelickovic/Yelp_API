namespace Yelp_API.Models;

public class Sentiment{
    public double Score { get; }
    public string? Label { get; }

    public Sentiment(double score, string label)
    {
        Score = score;
        Label = label;
    }
}
