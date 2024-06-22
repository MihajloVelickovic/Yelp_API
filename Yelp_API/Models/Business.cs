namespace Yelp_API.Models;

public class Business: IComparable<Business>{
    [JsonProperty("review_count")]
    public int ReviewCount { get; set; }
    [JsonProperty("id")]
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Rating{ get; set; }
    [JsonProperty("business_hours")]
    public BusinessHours? BusinessHours{ get; set; }
    public string? Price{ get; set; }
    public int CompareTo(Business? other){
        if (Price!.Length == other!.Price!.Length)
            return 0;
        else if (Price.Length > other.Price.Length)
            return 1;
        return -1;
    }
    public override string ToString()
    {
        return "{\n" + $"  Restaurant: {Name ?? "Unknown"}\n" +
               $"  ID: {Id ?? "Unknown"}\n" +
               $"  Rating: {Rating ?? "Unknown"}\n" +
               $"  Review Count: {ReviewCount}\n" +
               $"  Price: {Price ?? "Unknown"}\n" +
               $"  Business Hours: {BusinessHours?.ToString() ?? "Unknown"}" +
               "\n}";
    }
}
