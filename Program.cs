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

            // Generate playlist for user

            // First, ask what month/year they wish to generate from
            Console.WriteLine("Let's generate a playlist based on songs you've liked from a specified month and year.");

            bool inputGood = false;
            int year = 0, month = 0;

            while (!inputGood)
            {
                Console.Write("Enter a year: ");
                string yearInput = Console.ReadLine();

                Console.Write("Enter a month: ");
                string monthInput = Console.ReadLine();

                // Validate
                if ((int.TryParse(yearInput, out var y))
                    && (int.TryParse(monthInput, out var m)))
                {
                    if ((y >= 2008 && y <= DateTime.Now.Year)
                        && (m >= 1 && m <= 12))
                    {
                        year = y;
                        month = m;

                        inputGood = true;
                    }
                }

                if (!inputGood)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error: please enter a valid year and month.");
                    Console.WriteLine("For example, '2022', and '5' for January 2022.");
                    Console.WriteLine();
                }
            }

            Console.WriteLine($"You've selected {month}/{year}!");


            // Generate the playlist
            // @todo make a playlist service or something
        }
    }
}