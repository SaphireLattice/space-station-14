using Content.Shared.Chemistry;
using Content.Shared.Containers.ItemSlots;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.Chemistry.UI
{
    /// <summary>
    /// Initializes a <see cref="TODOMasterWindow"/> and updates it when new server messages are received.
    /// </summary>
    [UsedImplicitly]
    public sealed class TODOMasterBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private TODOMasterWindow? _window;

        public TODOMasterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        /// <summary>
        /// Called each time a chem master UI instance is opened. Generates the window and fills it with
        /// relevant info. Sets the actions for static buttons.
        /// </summary>
        protected override void Open()
        {
            base.Open();

            // Setup window layout/elements
            _window = this.CreateWindow<TODOMasterWindow>();
            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;

            _window.OnReagentButtonPressed += (button) => SendMessage(new TODOMasterReagentAmountButtonMessage(button.Id, button.Amount, button.SourceSlot));
            _window.OnStorageTransferPressed += (slotId, amount) =>
            {
                SendMessage(new TODOMasterStorageSelectedMessage(slotId));
                SendMessage(new TODOMasterReagentAmountButtonMessage(null, amount, TODOMasterSlot.Buffer));
            };
            _window.OnStorageSelected += (slotId) => SendMessage(new TODOMasterStorageSelectedMessage(slotId));
            _window.OnTargetSwitched += (slot) => SendMessage(new TODOMasterTargetSwitchedMessage(slot));

            _window.OnSlotEject += (slotId) => SendMessage(new ItemSlotButtonPressedEvent(slotId));
        }

        /// <summary>
        /// Update the ui each time new state data is sent from the server.
        /// </summary>
        /// <param name="state">
        /// Data of the <see cref="SharedReagentDispenserComponent"/> that this ui represents.
        /// Sent from the server.
        /// </param>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            var castState = (TODOMasterBoundUserInterfaceState) state;

            _window?.UpdateState(castState); // Update window state
        }
    }
}