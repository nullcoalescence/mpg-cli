using SpotifyAPI.Web;

namespace mpg_cli
{
    public class Constants
    {
        // App info
        public static readonly string AppNameASCII = @"                               _ _ 
 _ __ ___  _ __   __ _     ___| (_)
| '_ ` _ \| '_ \ / _` |   / __| | |
| | | | | | |_) | (_| |  | (__| | |
|_| |_| |_| .__/ \__, |___\___|_|_|
          |_|    |___/_____|       ";
        
        public static readonly string AppName = "mpg_cli";
        public static readonly string AppVersion = "v1.0";

        // 'spotify_config.json' - can hardcode it here, or pass it in as an arg
        public static readonly string SpotifyConfigPath = @"d:/keystore/mpg-cli/spotify_config.json";

        // Scopes
        // When updating these, need to call SpotifyPkceAuthService.Authorization() with
        // param forceReAuth: true
        public static List<string> SpotifyScopes = new List<string> { Scopes.UserReadEmail };
    }
}
