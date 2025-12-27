using FoxMapperBackend.Models.DTOs;

namespace FoxMapperBackend.Services;

public interface ICourierLocationStore
{
    CourierLocationDto? Get(int courierId);
    void Set(int courierId, CourierLocationDto location);
}

