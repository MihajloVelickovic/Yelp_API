﻿namespace Yelp_API;

public static class Program{
    public static void Main(string[] args){
        string? clientId, apikey;

        SetEnvironmetVariables(out clientId, out apikey);

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
