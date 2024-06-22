namespace Yelp_API.Models;

public class YelpBusiness{
    [JsonProperty("review_count")]
    public int ReviewCount { get; set; }
    [JsonProperty("id")]
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Alias { get; set; }
}
