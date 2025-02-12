namespace inventory_server.Models.Response;

public class PagedResult<T>
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<T> Items { get; set; } = new List<T>();
}