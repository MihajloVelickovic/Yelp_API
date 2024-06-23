namespace Yelp_API;

public static class Program{
    public static void Main(string[] args) {
        
        SetEnvironmetVariables();
        
        var yelpService = new YelpService();

        yelpService.StartListen();

        Console.ReadKey();

        Console.WriteLine("Server was shutdown by user");
    }
    public static void SetEnvironmetVariables(){
        var baseDir = DirExtension.ProjectBase();
        if (baseDir != null){
            var path = Path.Combine(baseDir, ".env");
            DotEnv.Inject(path);
        }
    }
}