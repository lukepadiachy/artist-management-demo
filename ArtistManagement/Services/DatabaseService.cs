using SQLite;
using ArtistManagement.Models;

namespace ArtistManagement.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;
    private string _databasePath = Path.Combine(FileSystem.AppDataDirectory, "artists.db");

    public DatabaseService()
    {
        _database = new SQLiteAsyncConnection(_databasePath);
        _database.CreateTableAsync<Artist>().Wait();
    }

    public async Task<List<Artist>> GetArtistsAsync()
    {
        return await _database.Table<Artist>().ToListAsync();
    }

    public async Task<int> SaveArtistAsync(Artist artist)
    {
        if (artist.Id != 0)
            return await _database.UpdateAsync(artist);
        else
            return await _database.InsertAsync(artist);
    }

    public async Task<int> DeleteArtistAsync(Artist artist)
    {
        return await _database.DeleteAsync(artist);
    }
}