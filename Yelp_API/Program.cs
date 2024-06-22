namespace Yelp_API;

public static class Program{
    public static async Task Main(string[] args) {
        
        SetEnvironmetVariables();

        try {
            var searchResult = await YelpService.GetRestaurants("NYC", ["bars","french"]);

            foreach (var r in searchResult.Businesses!){
                Console.WriteLine(r.Id);
                var result = await YelpService.GetRestaurantById(r.Id!);
                Console.WriteLine($"Business: {result.Name} {result.Id}");
            }
            
        }
        catch(Exception ex){
            Console.WriteLine(ex.Message);
        }
    }
    public static void SetEnvironmetVariables(){
        var baseDir = DirExtension.ProjectBase();
        if (baseDir != null){
            var path = Path.Combine(baseDir, ".env");
            DotEnv.Inject(path);
        }
    }
}