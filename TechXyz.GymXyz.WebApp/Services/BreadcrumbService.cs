using System;
using System.Collections.Generic;

namespace TechXyz.GymXyz.WebApp.Services;

public sealed class BreadcrumbService
{
    private readonly List<BreadcrumbItem> items = new();

    public IReadOnlyList<BreadcrumbItem> Items => items;

    public event Action? Changed;

    public void SetItems(IEnumerable<BreadcrumbItem> newItems)
    {
        items.Clear();
        items.AddRange(newItems);
        Changed?.Invoke();
    }
}

public sealed record BreadcrumbItem
{
    public string? Text { get; init; }
    public string? Href { get; init; }
    public bool IsHome { get; init; }
}
