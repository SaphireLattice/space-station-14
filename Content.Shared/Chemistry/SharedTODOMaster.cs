using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry
{
    /// <summary>
    /// This class holds constants that are shared between client and server.
    /// </summary>
    public sealed class SharedTODOMaster
    {
    }

    [Serializable, NetSerializable]
    public sealed class TODOMasterReagentAmountButtonMessage(ReagentId? reagentId, TODOMasterReagentAmount amount, TODOMasterSlot source) : BoundUserInterfaceMessage
    {
        public readonly ReagentId? ReagentId = reagentId;
        public readonly TODOMasterReagentAmount Amount = amount;
        public readonly TODOMasterSlot Source = source;
    }

    [Serializable, NetSerializable]
    public sealed class TODOMasterTargetSwitchedMessage(TODOMasterSlot target) : BoundUserInterfaceMessage
    {
        public readonly TODOMasterSlot Target = target;
    }

    [Serializable, NetSerializable]
    public sealed class TODOMasterStorageSelectedMessage(string slotId) : BoundUserInterfaceMessage
    {
        public readonly string SlotId = slotId;
    }

    [Serializable, NetSerializable]
    public sealed class TODOMasterBoundUserInterfaceState(
        TODOMasterSlot target,
        string? selectedSlotId,
        ReagentInventoryItem? primary,
        ReagentInventoryItem? secondary,
        List<ReagentInventoryItem?>? bufferContainers
    ) : BoundUserInterfaceState
    {
        public readonly TODOMasterSlot TargetSlot = target;
        public readonly string? SelectedSlotId = selectedSlotId;
        public readonly ReagentInventoryItem? PrimaryContainerInfo = primary;
        public readonly ReagentInventoryItem? SecondaryContainerInfo = secondary;
        public readonly List<ReagentInventoryItem?>? BufferContainersInfo = bufferContainers;
    }


    public enum TODOMasterSlot
    {
        Primary,
        Secondary,
        Buffer
    }

    public enum TODOMasterReagentAmount
    {
        U1 = 1,
        U5 = 5,
        U10 = 10,
        U25 = 25,
        U50 = 50,
        U100 = 100,
        All,
    }

    public static class TODOMasterReagentAmountToFixedPoint
    {
        public static FixedPoint2 GetFixedPoint(this TODOMasterReagentAmount amount)
        {
            if (amount == TODOMasterReagentAmount.All)
                return FixedPoint2.MaxValue;
            else
                return FixedPoint2.New((int)amount);
        }
    }

    [Serializable, NetSerializable]
    public enum TODOMasterUiKey
    {
        Key
    }
}
