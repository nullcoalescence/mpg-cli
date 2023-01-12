using mpg_cli.Models;
using mpg_cli.Services;
using Newtonsoft.Json;

namespace mpg_cli
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Intro
            Console.WriteLine(Constants.AppNameASCII);
            Console.WriteLine(Constants.AppVersion);

            // Authorization
            Console.WriteLine("Authoriation your account with Spotify.");
            // @todo: allow this to be passed in as command line arg
            Console.WriteLine($"Using SpotifyConfig: '{Constants.SpotifyConfigPath}'.");

            if (!File.Exists(Constants.SpotifyConfigPath))
            {
                throw new FileNotFoundException(Constants.SpotifyConfigPath);
            }

            var spotifyConfigContents = File.ReadAllText(Constants.SpotifyConfigPath);
            var spotifyConfig = JsonConvert.DeserializeObject<SpotifyConfig>(spotifyConfigContents);

            var authService = new SpotifyPkceAuthService(spotifyConfig.CredentialsPath,
                spotifyConfig.ClientId,
                spotifyConfig.CallbackUrl,
                spotifyConfig.Port,
                Constants.SpotifyScopes);

            var spotifyClient = await authService.Authorize();
            var profile = await spotifyClient.UserProfile.Current();

            // Greet user
            Console.WriteLine($"Hello, {profile.DisplayName}!");
        }
    }
}