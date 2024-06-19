namespace Yelp_API;

public static class YelpService{
    private static readonly HttpClient httpClient = new();

    static YelpService(){

        var apiKey = Environment.GetEnvironmentVariable("API_KEY");

        httpClient.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
    }

    public static IObservable<YelpReviews>  GetRestaurantReviews(string restaurantId){
        var apiUrl = $"https://api.yelp.com/v3/businesses/{restaurantId}/reviews";

        return Observable.FromAsync(async () => {
            var response = await httpClient!.GetAsync(apiUrl);
            response!.EnsureSuccessStatusCode();
            string json = await response!.Content!.ReadAsStringAsync();

            
            var yelpReviews = JsonConvert.DeserializeObject<YelpReviews>(json);
            if (yelpReviews == null)
                throw new Exception("Failed to deserialize YelpReviews from JSON.");

            return yelpReviews;
        });
    }
}
