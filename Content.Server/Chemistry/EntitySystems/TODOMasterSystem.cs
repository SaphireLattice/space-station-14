using Content.Server.Chemistry.Components;
using Content.Server.Labels;
using Content.Server.Popups;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Administration.Logs;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Storage;
using JetBrains.Annotations;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.Chemistry.EntitySystems
{

    /// <summary>
    /// Contains all the server-side logic for jug-based ChemMasters.
    /// <seealso cref="TODOMasterComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed class TODOMasterSystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly StorageSystem _storageSystem = default!;
        [Dependency] private readonly LabelSystem _labelSystem = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        [Dependency] private readonly SolutionTransferMachineSystem _transferMachineSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<TODOMasterComponent, ComponentStartup>(SubscribeUpdateUiState);
            SubscribeLocalEvent<TODOMasterComponent, SolutionContainerChangedEvent>(SubscribeUpdateUiState);
            SubscribeLocalEvent<TODOMasterComponent, EntInsertedIntoContainerMessage>(SubscribeUpdateUiState);
            SubscribeLocalEvent<TODOMasterComponent, EntRemovedFromContainerMessage>(SubscribeUpdateUiState);
            SubscribeLocalEvent<TODOMasterComponent, BoundUIOpenedEvent>(SubscribeUpdateUiState);

            //SubscribeLocalEvent<ChemMasterComponent, ChemMasterReagentAmountButtonMessage>(OnReagentButtonMessage);

            //SubscribeLocalEvent<TODOMasterComponent, ChemMasterSetModeMessage>(OnSetModeMessage);
            SubscribeLocalEvent<TODOMasterComponent, TODOMasterReagentAmountButtonMessage>(OnReagentButtonMessage);
            SubscribeLocalEvent<TODOMasterComponent, TODOMasterStorageSelectedMessage>(OnStorageSelected);
            SubscribeLocalEvent<TODOMasterComponent, TODOMasterTargetSwitchedMessage>(OnTargetSwitched);
        }

        private void SubscribeUpdateUiState<T>(Entity<TODOMasterComponent> ent, ref T ev)
        {
            UpdateUiState(ent);
        }

        private void ClickSound(Entity<TODOMasterComponent> entity)
        {
            _audioSystem.PlayPvs(entity.Comp.ClickSound, entity, AudioParams.Default.WithVolume(-2f));
        }

        private void OnReagentButtonMessage(Entity<TODOMasterComponent> entity, ref TODOMasterReagentAmountButtonMessage message)
        {
            Console.WriteLine($"{message.Source} {message.Amount} ({message.ReagentId}) - {entity.Comp.SelectedStorageSlot}, {entity.Comp.StorageTransferTarget}");
            var dispenserInventory = _transferMachineSystem.GetInventory(entity.Owner, true, withContents: true, fullName: true, insertNulls: true);
            if (dispenserInventory is null || dispenserInventory.Count != 2)
                return;

            var sourceSlot = message.Source switch
            {
                TODOMasterSlot.Primary => dispenserInventory[0]?.StorageSlotId,
                TODOMasterSlot.Secondary => dispenserInventory[1]?.StorageSlotId,
                TODOMasterSlot.Buffer => entity.Comp.SelectedStorageSlot,
                _ => null
            };

            var targetSlot = message.ReagentId is null ?
                // No specific reagent being transferred. Storage goes into selected beaker, beakers go selected storage
                message.Source switch
                {
                    TODOMasterSlot.Primary => entity.Comp.SelectedStorageSlot,
                    TODOMasterSlot.Secondary => entity.Comp.SelectedStorageSlot,
                    TODOMasterSlot.Buffer => entity.Comp.StorageTransferTarget == TODOMasterSlot.Primary
                        ? dispenserInventory[0]?.StorageSlotId
                        : dispenserInventory[1]?.StorageSlotId,
                    _ => null
                }
                // Got a filter, beakers into one another, buffer invalid as a source
                : message.Source switch
                {
                    TODOMasterSlot.Primary => dispenserInventory[1]?.StorageSlotId,
                    TODOMasterSlot.Secondary => dispenserInventory[0]?.StorageSlotId,
                    TODOMasterSlot.Buffer => null,
                    _ => null
                };

            if (sourceSlot is null || targetSlot is null)
                return;

            _transferMachineSystem.SolutionTransfer(entity.Owner, sourceSlot, targetSlot, (int)message.Amount, message.ReagentId);

            UpdateUiState(entity);
            ClickSound(entity);
        }

        private void OnStorageSelected(Entity<TODOMasterComponent> entity, ref TODOMasterStorageSelectedMessage message)
        {
            // TODO: Ensure the slot exists - Won't really cause anything to happen but could cause odd UI state. Put that into the STM system?

            entity.Comp.SelectedStorageSlot = message.SlotId;

            UpdateUiState(entity, true);
            ClickSound(entity);
        }

        private void OnTargetSwitched(Entity<TODOMasterComponent> entity, ref TODOMasterTargetSwitchedMessage message)
        {
            if (!Enum.IsDefined(typeof(TODOMasterSlot), message.Target) || message.Target == TODOMasterSlot.Buffer)
                return;

            entity.Comp.StorageTransferTarget = message.Target;

            UpdateUiState(entity, true);
            ClickSound(entity);
        }

        private void UpdateUiState(Entity<TODOMasterComponent> ent, bool partialUpdate = false)
        {
            var dispenserInventory = _transferMachineSystem.GetInventory(ent.Owner, true, withContents: true, fullName: true, insertNulls: true);

            if (dispenserInventory is null || dispenserInventory.Count != 2)
                return;

            var storageInventory = partialUpdate ? null : _transferMachineSystem.GetInventory(ent.Owner, false, withContents: true, fullName: false, insertNulls: false);
            if (!partialUpdate && storageInventory is null)
                return;

            var state = new TODOMasterBoundUserInterfaceState(
                ent.Comp.StorageTransferTarget,
                ent.Comp.SelectedStorageSlot,
                dispenserInventory[0],
                dispenserInventory[1],
                storageInventory
            );

            _userInterfaceSystem.SetUiState(ent.Owner, TODOMasterUiKey.Key, state);
        }
    }
}