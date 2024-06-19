namespace Yelp_API.Models;

public class YelpReviews
{
    [JsonProperty("reviews")]
    public List<Review>? Reviews { get; set; }
}
