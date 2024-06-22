namespace Yelp_API.Configuration;

public class DotEnv{
    public static void Inject(string path){

        if (!File.Exists(path))
            return;

        foreach (var line in File.ReadAllLines(path)){
            var split = line.Split("=", StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
                throw new Exception("Invalid format of env variable");

            var variables = split[1].Split(",");
            for(var i=0; i<variables.Length; ++i){
                var begin = variables[i].IndexOf("${", StringComparison.Ordinal);
                var end = variables[i].IndexOf('}');
            
                if((begin == -1 && end != -1) || (begin != -1 && end == -1) || (begin == -1 && end == -1))
                    continue;
            
                var substr = variables[i].Substring(begin + 2, end - begin - 2);
                var envVar = Environment.GetEnvironmentVariable(substr);

                if (envVar == null)
                    throw new Exception($"Environment variable {substr} not found");

                variables[i] = variables[i].Replace(split[1].Substring(begin, end-begin + 1), envVar);
            }
            split[1] = string.Join(",", variables);
            Environment.SetEnvironmentVariable(split[0], split[1]);

        }
    }
}
