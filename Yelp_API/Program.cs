namespace Yelp_API;

public static class Program{
    public static async Task Main(string[] args) {
        
        SetEnvironmetVariables();
        var yelpService = new YelpService();
        //var re = yelpService.GetRestaurants("London", ["indian"]);

        //await re.ForEachAsync(res => {
        //    foreach (var a in res.Businesses!){
        //        Console.WriteLine(a.Name);
        //    }
        //});
        
        Console.ReadKey();

    }
    public static void SetEnvironmetVariables(){
        var baseDir = DirExtension.ProjectBase();
        if (baseDir != null){
            var path = Path.Combine(baseDir, ".env");
            DotEnv.Inject(path);
        }
    }
}