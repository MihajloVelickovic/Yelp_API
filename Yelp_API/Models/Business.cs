namespace Yelp_API.Models;

public class Business: IComparable<Business>{
    [JsonProperty("review_count")]
    public int ReviewCount { get; set; }
    [JsonProperty("id")]
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Rating{ get; set; }
    [JsonProperty("is_closed")]
    public bool? IsClosed{ get; set; }
    public string? Price{ get; set; }
    public int CompareTo(Business? other){
        if (Price!.Length == other!.Price!.Length)
            return 0;
        else if (Price.Length > other.Price.Length)
            return 1;
        return -1;
    }
}
