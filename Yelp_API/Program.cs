﻿namespace Yelp_API;

public static class Program{
    public async static Task Main(string[] args) {
        string? clientId, apikey;

        SetEnvironmetVariables(out clientId, out apikey);

        try {
            var searchResult = await YelpService.FindRestaurantId("Donnies Donuts", "Daytona Blvd Daytona Beach");

            if (searchResult == null)
                return;

            foreach (var r in searchResult.Businesses){
                Console.WriteLine(r.Id);

                var result = await YelpService.GetRestaurantById(r.Id);

                Console.WriteLine($"Business: {result.Name} {result.Id}");

            }
            string a = "asdasd";
            

        }
        catch(Exception ex){
            Console.WriteLine(ex.Message);
        }
    }
    public static void SetEnvironmetVariables(out string? clientId, out string? apiKey){
        var baseDir = DirExtension.ProjectBase();
        if (baseDir != null){
            var path = Path.Combine(baseDir, ".env");
            DotEnv.Inject(path);
        }
        clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
        apiKey = Environment.GetEnvironmentVariable("API_KEY");
    }
}