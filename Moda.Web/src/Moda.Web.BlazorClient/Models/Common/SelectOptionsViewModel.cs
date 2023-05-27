namespace Moda.Web.BlazorClient.Models.Common;

public class SelectOptionsViewModel<TKey>
{
    public SelectOptionsViewModel(TKey id, string name)
    {
        Id = id;
        Name = name;
    }

    public TKey Id { get; set; }
    public string Name { get; set; } = default!;
}
