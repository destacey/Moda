using Microsoft.AspNetCore.Components;
using Moda.Web.BlazorClient.Infrastructure.Preferences;
using MudBlazor;

namespace Moda.Web.BlazorClient.Components.Common;

public class ModaDataGrid<T> : MudDataGrid<T>
{
    [Inject]
    private IClientPreferenceManager ClientPreferences { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (await ClientPreferences.GetPreference() is ClientPreference clientPreference)
        {
            SetTablePreference(clientPreference.TablePreference);
        }
        
        await base.OnInitializedAsync();
    }

    private void SetTablePreference(ModaTablePreference tablePreference)
    {
        FixedHeader = true;

        Dense = tablePreference.IsDense;
        Striped = tablePreference.IsStriped;
        Bordered = tablePreference.HasBorder;
        Hover = tablePreference.IsHoverable;
    }
}