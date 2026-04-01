namespace Expedition.Domain.Enums;

public enum ExpeditionStatus
{
    AwaitingCarrierPickup = 1,
    PickedUpByCarrier = 2,
    InTransit = 3,
    Delivered = 4,
    DeliveryFailed = 5
}
