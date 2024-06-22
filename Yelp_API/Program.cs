namespace Yelp_API;

public static class Program{
    public static async Task Main(string[] args) {
        
        SetEnvironmetVariables();

        try {
            var searchResult = await YelpService.GetRestaurantIds("London", ["indian"]);

            foreach (var r in searchResult.Ids!){
                Console.WriteLine($"Business: {r.Id}");
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