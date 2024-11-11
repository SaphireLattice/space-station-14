using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Dispenser;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Chemistry.Components
{
    /// <summary>
    /// An industrial grade chemical manipulator
    /// <seealso cref="TODOMasterSystem"/>
    /// </summary>
    [RegisterComponent]
    [Access(typeof(TODOMasterSystem))]
    public sealed partial class TODOMasterComponent : Component
    {

        [DataField]
        public string? SelectedStorageSlot;

        [DataField]
        public TODOMasterSlot StorageTransferTarget = TODOMasterSlot.Primary;

        [DataField]
        public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");
    }
}
