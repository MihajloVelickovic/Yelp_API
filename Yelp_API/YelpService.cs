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
            onNext: context =>
            {
                if (context != null)
                     ServeReq(context);
                else
                    Console.WriteLine("The context was null");
            },
            onError: err => Console.WriteLine(err.Message) 
        );

    }

    public void ServeReq(HttpListenerContext? context){
        if (context == null)
            return;


        var response = context.Request!.Url!.Query.Remove(0, 1).Split("&");

        if (response.Length != 2)
            throw new Exception("Need 2 query parameters");


        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var location = response[0].Split("=")[1];
        var categories = response[1].Split("=")[1].Split(",");

        SearchResult result = new();
        result.Businesses = new();

        GetRestaurants(location, categories).Subscribe(
            next => {               
                result.Businesses!.Add(next);
            },
            error => {
                Console.WriteLine(error.Message);
            },
            () => {
                result.Businesses.Sort();
                stopwatch.Stop();
                Console.WriteLine($"Found: {result.Businesses.Count} businesses");
                Console.WriteLine($"Location: {location}");
                Console.WriteLine($"Categories: {string.Join(",", categories)}");
                Console.WriteLine($"Time taked: {stopwatch.Elapsed.TotalMilliseconds} ms");
                //foreach (var b in result!.Businesses!)
                //    Console.WriteLine(b);
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
    public IObservable<Business> GetRestaurants(string location, string[] categories){

        return Observable.Create<Business>(async (observer, cencelationToken) => {           
            var ids = await GetRestaurantIds(location, categories);
          
            foreach (var id in ids.Ids!){
                var business = await  GetRestaurantById(id.Id!);               
                if (CheckBusinessCond(business))
                    observer.OnNext(business);
                
            }
            observer.OnCompleted();
        });
    }
    
    private bool CheckBusinessCond(Business business){
        return business.Price != null &&
               business.BusinessHours != null &&
               business.BusinessHours[0].IsOpenNow == false &&
               (business.Price.Length == 2 || business.Price.Length == 3) &&
               business.ReviewCount > 100;
    }

    public async Task<Business> GetRestaurantById(string businessId){
        try{
            var apiUrl = $"{businessId}";
            var response = await _httpClient!.GetAsync(apiUrl);
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
            Console.WriteLine($"Listening on port: {Environment.GetEnvironmentVariable("PORT")}....");
            while (true){
                try {
                    var context = await _httpListener!.GetContextAsync();
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
