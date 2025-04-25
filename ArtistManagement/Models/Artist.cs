using SQLite;

namespace ArtistManagement.Models;

public class Artist
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Genre { get; set; }
    public int AlbumsSold { get; set; }
    public DateTime ContractEndDate { get; set; }
}