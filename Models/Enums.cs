namespace FoxMapperBackend.Models;

public enum DeliveryRunStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum PackageStatus
{
    Assigned = 0,    // przypisana do przejazdu, jeszcze nie odebrana
    PickedUp = 1,    // kurier odebrał z magazynu
    InTransit = 2,   // "w trasie"
    Delivered = 3,   // doręczona
    Failed = 4       // nieudana próba (np. klient nieobecny)
}

public enum RouteStopType
{
    Depot = 0,       // magazyn / punkt startowy
    Package = 1,     // przystanek odpowiadający paczce
    Custom = 2       // np. dodatkowy punkt techniczny
}