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

    public static IObservable<SearchId> GetRestaurantIds(string location, string[] categories){
        var apiUrl = $"search?location={location}&categories={string.Join(",", categories)}";

        return Observable.FromAsync(async () => {
            var response = await _httpClient!.GetAsync(apiUrl);
            response!.EnsureSuccessStatusCode();
            string json = await response!.Content!.ReadAsStringAsync();
            
            var yelpReviews = JsonConvert.DeserializeObject<SearchId>(json);
            if (yelpReviews == null)
                throw new Exception("Failed to deserialize YelpReviews from JSON.");
            
            return yelpReviews;
        });
    }

    public static IObservable<SearchResult> GetRestaurants(){

        return Observable.FromAsync(async () => {
            return new SearchResult();
        });

    }
    
}
