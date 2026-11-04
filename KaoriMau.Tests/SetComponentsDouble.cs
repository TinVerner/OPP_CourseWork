using ООП_Курсовой.Tables;

namespace KaoriMau.Tests;

internal class SetComponentsDouble
{
    public List<MenuSetComponent> Components { get; set; } = new();

    public Exception? Error { get; set; }

    public List<long> RequestedIds { get; } = new();

    public Task<List<MenuSetComponent>> LoadAsync(long setId)
    {
        RequestedIds.Add(setId);

        if (Error != null)
        {
            return Task.FromException<List<MenuSetComponent>>(Error);
        }

        return Task.FromResult(Components);
    }
}