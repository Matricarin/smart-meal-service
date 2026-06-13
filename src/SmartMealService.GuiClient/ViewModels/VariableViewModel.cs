using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartMealService.GuiClient.ViewModels;

public partial class VariableViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Key { get; set; }

    [ObservableProperty]
    public partial string Value { get; set; }

    [ObservableProperty]
    public partial string Comment { get; set; }
}