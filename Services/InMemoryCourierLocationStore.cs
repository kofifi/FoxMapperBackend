using System.Collections.Concurrent;
using FoxMapperBackend.Models.DTOs;

namespace FoxMapperBackend.Services;

public class InMemoryCourierLocationStore : ICourierLocationStore
{
    private readonly ConcurrentDictionary<int, CourierLocationDto> _locations = new();

    public CourierLocationDto? Get(int courierId)
        => _locations.TryGetValue(courierId, out var location) ? location : null;

    public void Set(int courierId, CourierLocationDto location)
        => _locations[courierId] = location;
}

