namespace Moda.Web.BlazorClient.Models.Common;

public class SelectViewModel<TKey>
{
    public SelectViewModel() { }

    public SelectViewModel(IEnumerable<TKey> selectedValues, List<SelectOptionsViewModel<TKey>> options)
    {
        if (selectedValues.Any())
            SelectedValues = selectedValues;
        Options = options.OrderBy(o => o.Name).ToList();
    }

    public IEnumerable<TKey> SelectedValues { get; set; } = new HashSet<TKey>();
    public List<SelectOptionsViewModel<TKey>> Options { get; set; } = new();
}
