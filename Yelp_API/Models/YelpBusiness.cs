namespace Yelp_API.Models;

public class YelpBusiness{
    [JsonProperty("review_count")]
    public int ReviewCount { get; set; }
    public string? Id { get; set; }
}
