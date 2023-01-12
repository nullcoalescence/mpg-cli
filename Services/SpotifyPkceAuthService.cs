using Newtonsoft.Json;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace mpg_cli.Services
{
    internal class SpotifyPkceAuthService : IDisposable
    {
        private string credentialsPath;
        private readonly string? clientId;
        private readonly string callbackUrl;
        private readonly int port;
        private readonly List<string> scopes;

        private readonly EmbedIOAuthServer server;
        private SpotifyClient spotifyClient;

        public SpotifyPkceAuthService(string credentialsPath, string clientId, string callbackUrl, int port, List<string> scopes)
        {
            this.credentialsPath = credentialsPath;
            this.clientId = clientId;
            this.callbackUrl = callbackUrl;
            this.port = port;
            this.scopes = scopes;

            this.server = new EmbedIOAuthServer(new Uri(callbackUrl), port);
        }

        // forceReAuth needs to be set to true if you are changing scopes around
        public async Task<SpotifyClient> Authorize(bool forceReAuth = false)
        {
            if (!(await NeedsToReAuthorize(forceReAuth)))
            {
                await this.Start();
            }
            else
            {
                await this.StartAuthentication();
            }

            return this.spotifyClient;
        }

        private async Task Start()
        {
            var json = await File.ReadAllTextAsync(this.credentialsPath);
            var token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);

            var authenticator = new PKCEAuthenticator(this.clientId, token);
            authenticator.TokenRefreshed += (sender, token) => File.WriteAllText(this.credentialsPath, JsonConvert.SerializeObject(token));

            var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);

            this.spotifyClient = new SpotifyClient(config);
        }

        private async Task StartAuthentication()
        {
            var (verifier, challenge) = PKCEUtil.GenerateCodes();

            await this.server.Start();

            server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await this.server.Stop();

                PKCETokenResponse token = await new OAuthClient().RequestToken(
                    new PKCETokenRequest(this.clientId, response.Code, this.server.BaseUri, verifier)
                );

                await File.WriteAllTextAsync(this.credentialsPath, JsonConvert.SerializeObject(token));

                await Start();
            };

            var request = new LoginRequest(this.server.BaseUri, this.clientId, LoginRequest.ResponseType.Code)
            {
                CodeChallenge = challenge,
                CodeChallengeMethod = "S256",
                Scope = this.scopes
            };

            var uri = request.ToUri();

            try
            {
                Console.WriteLine("Opening your browser to authenticate with Spotify...");
                BrowserUtil.Open(uri);
            }
            catch (Exception)
            {
                Console.WriteLine($"Unable to open your browser. Please copy this URL into your browser and open:\n{uri}");
            }

            Console.WriteLine("****");
            Console.WriteLine("Press [ENTER] in this window after signing in to Spotify.");
            Console.WriteLine("****");
            Console.WriteLine();

            Console.ReadLine();
        }

        private async Task<bool> NeedsToReAuthorize(bool forceReAuth)
        {
            // Paramr passed to parent function is true - forcing re-auth
            if (forceReAuth) return true;

            // File doesn't exist - create it then return true
            if (!File.Exists(this.credentialsPath))
            {
                File.Create(this.credentialsPath);
                return true;
            }
            
            // Catches any weird formatting issues in the file leading to de-serialization issues
            // Most common case is maybe the file exists but has no contents.
            try
            {
                var json = await File.ReadAllTextAsync(this.credentialsPath);
                var token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);
            } 
            catch(Exception)
            {
                return true;
            }

            // If we made it all the way here, I guess we're good...
            return false;
        }

        public void Dispose()
        {
            this.server.Dispose();
        }
    }
}