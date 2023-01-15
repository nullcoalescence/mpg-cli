using SpotifyAPI.Web;

namespace mpg_cli.Services
{
    internal class PlaylistService
    {
        private SpotifyClient spotify;

        public PlaylistService(SpotifyClient spotify)
        {
            this.spotify = spotify;
        }

        // Create playlist
        public async Task CreatePlaylistOfLikedSongsInRange(string playlistName, int month, int year)
        {
            var songsLikedInRange = await GetSongsInMonth(month, year);
            var uris = songsLikedInRange.Select(s => s.Track.Uri).ToList();

            var playlist = await CreatePlaylist(playlistName);

            await this.spotify.Playlists.AddItems(playlist.Id,
                new PlaylistAddItemsRequest(uris));

            Console.WriteLine("Created playlist!");
        }

        // Create playlist
        private async Task<FullPlaylist> CreatePlaylist(string name)
        {
            var user = await this.spotify.UserProfile.Current();
            var id = user.Id;

            var playlist = await this.spotify.Playlists.Create(
                id.ToString(),
                new PlaylistCreateRequest(name));

            return playlist;
        }

        // Pull songs from date range
        private async Task<List<SavedTrack>> GetSongsInMonth(int month, int year)
        {
            Console.WriteLine($"Pulling all songs liked in {month}/{year}. This may take awhile...");

            var tracks = await this.spotify.Library.GetTracks();
            List<SavedTrack> savedTracks = new List<SavedTrack>();

            await foreach (var item in spotify.Paginate(tracks))
            {
                savedTracks.Add(item);
            }

            List<SavedTrack> tracksInRange = savedTracks
                .Where(s => s.AddedAt.Month == month && s.AddedAt.Year == year)
                .OrderBy(s => s.AddedAt)
                .ToList();

            Console.WriteLine($"Done, found {tracksInRange.Count} liked in {month}/{year}.");

            return tracksInRange;
        }


    }
}
