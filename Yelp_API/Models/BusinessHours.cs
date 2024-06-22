namespace Yelp_API.Models;

public class BusinessHours{
    [JsonProperty("is_open_now")]
    public bool? IsOpenNow{ get; set; }
    
}