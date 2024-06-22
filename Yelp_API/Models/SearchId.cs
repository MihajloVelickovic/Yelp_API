namespace Yelp_API.Models;

public class SearchId{
    [JsonProperty("businesses")]
    public List<BusinessId>? Ids { get; set; }
}