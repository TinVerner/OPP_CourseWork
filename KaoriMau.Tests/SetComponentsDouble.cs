using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ООП_Курсовой.Tables;

namespace KaoriMau.Tests;

// Заглушка возвращает подготовленные данные или ошибку.
// Также записывает обращения для проверки в тестах.
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