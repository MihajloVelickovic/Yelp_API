namespace Yelp_API;

public static class YelpService{
    private static readonly HttpClient _httpClient;
    private static readonly HttpListener _httpListener;
    private static IDisposable _sub;
    
    static YelpService(){
        var apiKey = Environment.GetEnvironmentVariable("API_KEY");
        var baseAddr = Environment.GetEnvironmentVariable("BASE_ADDR");
        var domains = Environment.GetEnvironmentVariable("PREFIXES")!.Split(",");
        _httpListener = new();
        foreach (var domain in domains){
            Console.WriteLine(domain);
            _httpListener.Prefixes.Add(domain);
        }

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
    
    private static IObservable<HttpListenerContext?> GetReqStream(){
        return Observable.Create<HttpListenerContext?>(async (o) => {
            while (true){
                try {
                    var context = await _httpListener.GetContextAsync();
                    Console.WriteLine($"Request accepted in thread ${Thread.CurrentThread.ManagedThreadId}");
                    o.OnNext(context);
                }
                catch (HttpListenerException ec) {
                    o.OnError(ec);
                    return;
                }
                catch (Exception) {
                    o.OnNext(null);
                }
            }
        });
    }

    
}
