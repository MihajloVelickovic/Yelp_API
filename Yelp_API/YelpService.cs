using System.Diagnostics;
using System.Reactive.Concurrency;

namespace Yelp_API;

public class YelpService{
    private static HttpClient? _httpClient;
    private static HttpListener? _httpListener;
    private static IDisposable? _sub;
    
    public YelpService(){
        var apiKey = Environment.GetEnvironmentVariable("API_KEY");
        var baseAddr = Environment.GetEnvironmentVariable("BASE_ADDR");
        var domains = Environment.GetEnvironmentVariable("PREFIXES")!.Split(",");
        _httpListener = new();
        foreach (var domain in domains)
            _httpListener.Prefixes.Add(domain);
        
        _httpClient = new();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _httpClient.BaseAddress = new Uri(baseAddr!);
        try{
            _httpListener.Start();
        }
        catch (Exception exc){
            Console.WriteLine(exc.Message);
            Environment.Exit(-1);
        }

        _sub = GetReqStream().Distinct().Subscribe(
            onNext: ServeReq,
            onError: err => Console.WriteLine(err.Message) 
        );

    }

    public void ServeReq(HttpListenerContext? context){
        if (context == null)
            return;

        var response = context.Request!.Url!.Query.Remove(0, 1).Split("&");

        if (response.Length != 2)
            throw new Exception("Need 2 query parameters");
        
        var location = response[0].Split("=")[1];
        var categories = response[1].Split("=")[1].Split(",");

        List<SearchResult> lista = new();
        
        GetRestaurants(location, categories).Subscribe(
            next => {
                Console.WriteLine($"{location}, {categories}");
                lista.Add(next);
            },
            error => {
                Console.WriteLine(error.Message);
            },
            () => {
                foreach (var l in lista!)
                    foreach (var b in l.Businesses!)
                        Console.WriteLine(b.Name);
            }
        );
    }

    public async Task<SearchId> GetRestaurantIds(string location, string[] categories){
        var apiUrl = $"search?location={location}&categories={string.Join(",", categories)}";

        var response = await _httpClient!.GetAsync(apiUrl);
        response!.EnsureSuccessStatusCode();
        string json = await response!.Content!.ReadAsStringAsync();

        var yelpReviews = JsonConvert.DeserializeObject<SearchId>(json);
        if (yelpReviews == null)
            throw new Exception("Failed to deserialize YelpReviews from JSON.");

        return yelpReviews;
    }
    public IObservable<SearchResult> GetRestaurants(string location, string[] categories){

        return Observable.FromAsync(async () => {
            var ids = await GetRestaurantIds(location, categories);

            SearchResult list = new();
            list.Businesses = new();
            foreach (var id in ids.Ids!){
                var bId = await GetRestaurantById(id.Id!);
                list.Businesses!.Add(bId);
            }
            
            list.Businesses = list.Businesses!.Where(business => business.Price != null &&
                                                                 business.BusinessHours != null &&
                                                                 business.BusinessHours.IsOpenNow == false &&
                                                                 business.Price.Length is 2 or 3 &&
                                                                 business.ReviewCount > 0).ToList();
            list.Businesses.Sort();

            return list;
        });
    }
    
    public async Task<Business> GetRestaurantById(string businessId){
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
    private IObservable<HttpListenerContext?> GetReqStream(){
        return Observable.Create<HttpListenerContext?>(async (o) => {
            while (true){
                try {
                    var context = await _httpListener.GetContextAsync();
                    Console.WriteLine($"Request accepted in thread ${Thread.CurrentThread.ManagedThreadId}");
                    o.OnNext(context);
                }
                catch (HttpListenerException exc) {
                    Console.WriteLine(exc.Message);
                    o.OnError(exc);
                    return;
                }
                catch (Exception exc) {
                    Console.WriteLine(exc.Message);
                    o.OnNext(null);
                }
            }
        }).ObserveOn(TaskPoolScheduler.Default);
    }

    
}
