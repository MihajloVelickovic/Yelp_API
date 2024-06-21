namespace Yelp_API;

public static class YelpService{
    private static readonly HttpClient _httpClient = new();

    static YelpService(){

        var apiKey = Environment.GetEnvironmentVariable("API_KEY");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        //_httpClient.BaseAddress = new Uri("https://api.yelp.com/v3/");
    }

    public static IObservable<YelpReviews>  GetRestaurantReviews(string restaurantId){
        var apiUrl = $"https://api.yelp.com/v3/businesses/{restaurantId}/reviews";

        return Observable.FromAsync(async () => {
            var response = await _httpClient!.GetAsync(apiUrl);
            response!.EnsureSuccessStatusCode();
            string json = await response!.Content!.ReadAsStringAsync();

            
            var yelpReviews = JsonConvert.DeserializeObject<YelpReviews>(json);
            if (yelpReviews == null)
                throw new Exception("Failed to deserialize YelpReviews from JSON.");

            return yelpReviews;
        });
    }

    public async static Task<YelpReviews> GetReviews(string? id){
        var apiUrl = $"https://api.yelp.com/v3/businesses/{id}/reviews";

        var response = await _httpClient!.GetAsync(apiUrl);
        response!.EnsureSuccessStatusCode();
        string json = await response!.Content!.ReadAsStringAsync();


        var yelpReviews = JsonConvert.DeserializeObject<YelpReviews>(json);
        if (yelpReviews == null)
            throw new Exception("Failed to deserialize YelpReviews from JSON.");

        return yelpReviews;
    }

    public static async Task<YelpSearchResult> FindRestaurantId(string name, string location){
        var apiUrl = $"https://api.yelp.com/v3/businesses/search?term=" +
            $"{Uri.EscapeDataString(name)}&location={Uri.EscapeDataString(location)}";

        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        var searchResult = JsonConvert.DeserializeObject<YelpSearchResult>(json);
        if (searchResult != null && searchResult?.Businesses?.Count > 0)
            return searchResult;
        else
            throw new Exception("Restaurant not found or no results found.");

    }
}
