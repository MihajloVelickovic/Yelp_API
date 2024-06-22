namespace Yelp_API;

public static class YelpService{
    private static readonly HttpClient _httpClient = new();
    static YelpService(){
        var apiKey = Environment.GetEnvironmentVariable("API_KEY");
        var baseAddr = Environment.GetEnvironmentVariable("BASE_ADDR");
        _httpClient = new();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _httpClient.BaseAddress = new Uri(baseAddr!);
    }

    public static IObservable<SearchResult>  GetRestaurants(string location, string[] categories){
        var apiUrl = $"search?location={location}&categories={string.Join(",", categories)}";

        return Observable.FromAsync(async () => {
            var response = await _httpClient!.GetAsync(apiUrl);
            response!.EnsureSuccessStatusCode();
            string json = await response!.Content!.ReadAsStringAsync();
            
            var yelpReviews = JsonConvert.DeserializeObject<SearchResult>(json);
            if (yelpReviews == null)
                throw new Exception("Failed to deserialize YelpReviews from JSON.");

            return yelpReviews;
        });
    }
    public static async Task<SearchResult> FindRestaurantId(string name, string location){
        var apiUrl = $"search?term=" +
            $"{Uri.EscapeDataString(name)}&location={Uri.EscapeDataString(location)}";

        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        var searchResult = JsonConvert.DeserializeObject<SearchResult>(json);
        if (searchResult != null && searchResult?.Businesses?.Count > 0)
            return searchResult;
        else
            throw new Exception("Restaurant not found or no results found.");

    }

    public static async Task<Business> GetRestaurantById(string businessId){
        try{
            var apiUrl = $"{businessId}";
            var response = await _httpClient.GetAsync(apiUrl);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var business = JsonConvert.DeserializeObject<Business>(json);
            return business!;
        }
        catch (Exception ex){
            Console.WriteLine($"Error retrieving restaurant details: {ex.Message}");
            throw;
        }
    }
}
