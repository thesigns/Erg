using Erg.Core.Abstractions;
using Erg.Core.Game.Views.Inventory;
using Erg.Core.Ui;
using Erg.Core.World;
using Erg.Core.World.Actions;
using Erg.Core.World.Items;

namespace Erg.Core.Game.Views;

public enum InventoryViewMode
{
    Normal,
    QuantityInput
}

public class InventoryView : IGameView
{
    private readonly Game _game;
    private readonly IGameView _previousView;

    private List<InventoryListEntry> _entries = null!;
    private InventoryNavigator _navigator = null!;
    private InventoryViewMode _mode = InventoryViewMode.Normal;
    private string _quantityInput = "";
    private ItemActions _pendingAction;

    private readonly InventoryListBuilder _listBuilder = new();
    private readonly InventoryRenderer _renderer = new();

    public InventoryView(Game game, IGameView previousView)
    {
        _game = game;
        _previousView = previousView;
        RefreshList();
    }

    public void OnEnter()
    {
        RefreshList();
    }

    public void OnExit() { }

    private void RefreshList()
    {
        _entries = _listBuilder.Build(_game.CurrentSession.Player.Inventory);
        _navigator = new InventoryNavigator(_entries);
    }

    public void Update(IInput input)
    {
        if (_mode == InventoryViewMode.QuantityInput)
        {
            HandleQuantityInput(input);
            return;
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Escape) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.I))
        {
            _game.SwitchView(_previousView);
            return;
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Up) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp8))
            _navigator.MoveUp();
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Down) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp2))
            _navigator.MoveDown();
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.PageUp) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp9))
            _navigator.PageUp();
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.PageDown) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp3))
            _navigator.PageDown();
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Home) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp7))
            _navigator.Home();
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.End) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.Kp1))
            _navigator.End();

        if (_navigator.SelectedEntry is not ItemEntry itemEntry)
            return;

        var item = itemEntry.Item;
        var available = ItemActionResolver.GetAvailableActions(item);

        bool shiftHeld = input.KeyHeld.GetValueOrDefault(KeyboardKey.LeftShift) ||
                         input.KeyHeld.GetValueOrDefault(KeyboardKey.RightShift);

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.D) &&
            available.HasFlag(ItemActions.Drop))
        {
            if (shiftHeld && item.Count > 1)
                StartQuantityInput(ItemActions.Drop);
            else
                ExecuteAction(ItemActions.Drop, item);
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.R) &&
            available.HasFlag(ItemActions.Read))
        {
            if (shiftHeld && item.Count > 1)
                StartQuantityInput(ItemActions.Read);
            else
                ExecuteAction(ItemActions.Read, item);
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.W) &&
            available.HasFlag(ItemActions.Wield))
        {
            ExecuteAction(ItemActions.Wield, item);
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.C) &&
            available.HasFlag(ItemActions.Consume))
        {
            if (shiftHeld && item.Count > 1)
                StartQuantityInput(ItemActions.Consume);
            else
                ExecuteAction(ItemActions.Consume, item);
        }
    }

    private void StartQuantityInput(ItemActions action)
    {
        _mode = InventoryViewMode.QuantityInput;
        _quantityInput = "";
        _pendingAction = action;
    }

    private void HandleQuantityInput(IInput input)
    {
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Escape))
        {
            _mode = InventoryViewMode.Normal;
            return;
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Enter) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.KpEnter))
        {
            if (int.TryParse(_quantityInput, out int quantity) && quantity > 0)
            {
                if (_navigator.SelectedEntry is ItemEntry itemEntry)
                {
                    ExecuteAction(_pendingAction, itemEntry.Item, quantity);
                    return;
                }
            }
            _mode = InventoryViewMode.Normal;
            return;
        }

        for (int i = 0; i <= 9; i++)
        {
            var key = (KeyboardKey)(48 + i);
            var kpKey = (KeyboardKey)(320 + i);

            if (input.KeyPulse.GetValueOrDefault(key) ||
                input.KeyPulse.GetValueOrDefault(kpKey))
            {
                if (_quantityInput.Length < 6)
                    _quantityInput += i.ToString();
            }
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Backspace) && _quantityInput.Length > 0)
        {
            _quantityInput = _quantityInput[..^1];
        }
    }

    private void ExecuteAction(ItemActions action, Item item, int quantity = -1)
    {
        var session = _game.CurrentSession;

        CritterAction? critterAction = action switch
        {
            ItemActions.Drop => new DropAction(item, quantity),
            ItemActions.Read when item is Book book => CreateReadActions(book, quantity),
            ItemActions.Wield => new WieldAction(item),
            ItemActions.Consume => new ConsumeAction(item, quantity),
            _ => null
        };

        if (critterAction == null)
        {
            _mode = InventoryViewMode.Normal;
            return;
        }

        var result = critterAction.Execute(session.Player, session);

        if (result.Success)
        {
            session.ProcessCritterTurns(result.EnergyCost);

            if (!session.Player.IsAlive)
            {
                _game.SwitchView(_previousView);
                return;
            }
        }

        _game.SwitchView(_previousView);
    }

    private CritterAction CreateReadActions(Book book, int quantity)
    {
        return new ReadAction(book);
    }

    public void Render(IOutput output)
    {
        var writer = new Writer(output);

        ItemActions available = ItemActions.None;
        if (_navigator.SelectedEntry is ItemEntry itemEntry)
        {
            available = ItemActionResolver.GetAvailableActions(itemEntry.Item);
        }

        _renderer.Render(
            writer,
            _navigator,
            available,
            _mode == InventoryViewMode.QuantityInput,
            _quantityInput);

        output.SetCursor(0, 0, false);
        output.Render();
    }
}
