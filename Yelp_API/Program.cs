namespace Yelp_API;

public static class Program{
    public async static Task Main(string[] args) {
        string? clientId, apikey;

        SetEnvironmetVariables(out clientId, out apikey);

        try {
            var resorani = await YelpService.FindRestaurantId("Donnies Donuts", " Daytona Blvd Daytona Beach");

            if (resorani == null)
                return;


            //Console.WriteLine(resorani.Businesses[0].Id);
            var reviews = await YelpService.GetReviews(resorani.Businesses[0].Id);
                

        }
        catch(Exception ex){
            Console.WriteLine(ex.Message);
        }
    }
    public static void SetEnvironmetVariables(out string? clientId, out string? apiKey){
        var baseDir = DirExtension.ProjectBase();
        if (baseDir != null)
        {
            var path = Path.Combine(baseDir, ".env");
            DotEnv.Inject(path);
        }
        clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
        apiKey = Environment.GetEnvironmentVariable("API_KEY");
    }
}
