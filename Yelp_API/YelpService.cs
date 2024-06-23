using System.Text;

namespace Yelp_API;

public class YelpService{
    private static HttpClient? _httpClient;
    private static HttpListener? _httpListener;
    private static IDisposable? _sub;
    private static readonly Lazy<YelpService> instance = new Lazy<YelpService>(() => new YelpService());
    public static YelpService Instance => instance.Value;


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
    }

    public void StartListen(){
        try
        {
            _httpListener!.Start();
        }
        catch (Exception exc)
        {
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

        HttpListenerResponse? response = null;

        try
        {
            var request = context.Request!.Url!.Query.Remove(0, 1).Split("&");
            response = context.Response;
            if (request.Length != 2)
                throw new Exception("Need 2 query parameters");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var locationParams = request[0].Split("=");

            var categorieParams = request[1].Split("=");

            if (locationParams[0] != "location")
                throw new Exception("The first paramether must me location!");
            if (categorieParams[0] != "categories")
                throw new Exception("The second paramether must be categories!");

            

            var location = locationParams[1];
            var categories = categorieParams[1].Split(',');

            Console.WriteLine($"Processing request for: {location}, {categorieParams[1]}");

            if (categories.Length < 1)
                throw new Exception("There must be at least one filter category");

            SearchResult result = new();
            result.Businesses = new();

            _ = GetRestaurants(location, categories).Subscribe(
                next =>{
                    result.Businesses!.Add(next);
                },
                error =>{
                    Console.WriteLine(error.Message);
                },
                () =>{
                    result.Businesses.Sort();
                    stopwatch.Stop();
                    Console.WriteLine($"Found: {result.Businesses.Count} businesses");
                    Console.WriteLine($"Location: {location}");
                    Console.WriteLine($"Categories: {string.Join(",", categories)}");
                    Console.WriteLine($"Time taken: {stopwatch.Elapsed.TotalMilliseconds} ms");
                    SendResponse(response, result, stopwatch.Elapsed.TotalMilliseconds);
                }
            );
        }
        catch(Exception ex){
            Console.WriteLine($"There was an error serving request: {ex.Message}");
            SendErrorResponse(response, ex.Message, HttpStatusCode.BadRequest);
        }
    }

    private void SendErrorResponse(HttpListenerResponse? response, string message, HttpStatusCode statusCode){
        try{
            if (response == null)
                throw new Exception("The response was null");

            if (message == null)
                throw new ArgumentNullException("The message was null");

            var errorResponse = new{
                StatusCode = (int)statusCode,
                Message = message
            };
            var responseJson = JsonConvert.SerializeObject(errorResponse);
            var responseByteArray = Encoding.UTF8.GetBytes(responseJson);
            response.ContentLength64 = responseByteArray.Length;
            response.ContentType = "application/json";
            response.StatusCode = (int)statusCode;
            response.OutputStream.Write(responseByteArray);
        }
        catch (Exception e){
            Console.WriteLine($"Error sending error response: {e.Message}");
        }
    }

    public void SendResponse(HttpListenerResponse response, SearchResult result, double time){
        var responseObject = new{
            businessCount = result.Businesses!.Count,
            businesses = result.Businesses,
            timeTaken = time,
        };
        var responseJson = JsonConvert.SerializeObject(responseObject);
        var responseByteArray = Encoding.UTF8.GetBytes(responseJson);
        response.ContentLength64 = responseByteArray.Length;
        response.ContentType = "application/json";
        response.OutputStream.Write(responseByteArray);
    }
    
    public async Task<SearchResult> GetRestaurantIds(string location, string[] categories){
        var apiUrl = $"search?location={location}&categories={string.Join(",", categories)}";

        var response = await _httpClient!.GetAsync(apiUrl);
        response!.EnsureSuccessStatusCode();
        string json = await response!.Content!.ReadAsStringAsync();

        var yelpReviews = JsonConvert.DeserializeObject<SearchResult>(json);
        if (yelpReviews == null)
            throw new Exception("Failed to deserialize YelpReviews from JSON.");
        return yelpReviews;
    }
    public IObservable<Business> GetRestaurants(string location, string[] categories){

        return Observable.Create<Business>(async (observer, cancellationToken) => {           
            var businesses = await GetRestaurantIds(location, categories);
          
            foreach (var b in businesses.Businesses!){
                if (FirstCheck(b)){
                    var business = await GetRestaurantById(b.Id!);
                    if (SecondCheck(business))
                        observer.OnNext(business);
                }
            }
            observer.OnCompleted();
        }).SubscribeOn(TaskPoolScheduler.Default);
    }

    private bool FirstCheck(Business business){
        return business.Price != null &&
               business.Rating != null &&
               (business.Price.Length == 2 || business.Price.Length == 3) &&
               business.ReviewCount > 100 &&
               Double.Parse(business.Rating) >= 4.5;
    }
    
    private bool SecondCheck(Business business){
        return business.BusinessHours != null &&
               business.BusinessHours[0].IsOpenNow == true;
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
