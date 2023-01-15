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

            // Spotify API allows for 100 songs to be added at once
            var paginatedUris = PaginateList(uris);

            var playlist = await CreatePlaylist(playlistName);

            foreach (var list in paginatedUris)
            {
                await this.spotify.Playlists.AddItems(playlist.Id,
                    new PlaylistAddItemsRequest(list));
            }


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

        // Chunk up List<SavedTracks> so they can be added to a playlist
        // Spotify's playlist endpoint only allows 100 songs added at once
        private List<List<string>> PaginateList(List<string> uriList)
        {
            var pageSize = 100;
            var outerList = new List<List<string>>();

            if (uriList.Count < pageSize)
            {
                outerList.Add(uriList);
                return outerList;
            }

            var div = Math.DivRem(uriList.Count, 100);

            // Paginate by 100
            for (int i = 0; i <= div.Quotient; i++)
            {
                outerList.Add(uriList
                    .Skip(i * pageSize)
                    .Take(pageSize)
                    .ToList());
            }

            // Any remaining after the last 100?
            outerList.Add(uriList.Skip(uriList.Count - div.Remainder).ToList());

            return outerList;
        }


    }
}
