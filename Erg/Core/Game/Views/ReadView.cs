using System;
using System.Collections.Generic;
using System.Linq;
using Erg.Core.Abstractions;
using Erg.Core.Ui;
using Erg.Core.World;
using Erg.Core.World.Actions;
using Erg.Core.World.Items;

namespace Erg.Core.Game.Views;

public class ReadView : IGameView
{
    private readonly Game _game;
    private readonly IGameView _previousView;
    private readonly List<Item> _readableItems = new();

    public ReadView(Game game, IGameView previousView)
    {
        _game = game;
        _previousView = previousView;
        RefreshReadableItems();
    }

    private void RefreshReadableItems()
    {
        _readableItems.Clear();
        var inventory = _game.CurrentSession.Player.Inventory;
        foreach (var item in inventory.Items)
        {
            if (item is Book)
            {
                _readableItems.Add(item);
            }
        }
    }

    public void OnEnter() { }
    public void OnExit() { }

    public void Update(IInput input)
    {
        // ESC returns to previous view
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Escape) ||
            input.KeyPulse.GetValueOrDefault(KeyboardKey.R))
        {
            _game.SwitchView(_previousView);
            return;
        }

        // Check for item selection (a-t for up to 20 items)
        for (int i = 0; i < Math.Min(_readableItems.Count, 20); i++)
        {
            var key = (KeyboardKey)((int)KeyboardKey.A + i);
            if (input.KeyPulse.GetValueOrDefault(key))
            {
                var item = _readableItems[i];
                if (item is Book book)
                {
                    var action = new ReadAction(book);
                    var result = action.Execute(_game.CurrentSession.Player, _game.CurrentSession);

                    if (result.Success)
                    {
                        _game.CurrentSession.IncrementTurn();
                        _game.CurrentSession.ProcessCritterTurns(result.EnergyCost);

                        // Check if player died during NPC turns
                        if (!_game.CurrentSession.Player.IsAlive)
                        {
                            _game.SwitchView(new GameSummaryView(_game, GameEndReason.Died));
                            return;
                        }
                    }
                }
                _game.SwitchView(_previousView);
                return;
            }
        }
    }

    public void Render(IOutput output)
    {
        var writer = new Writer(output);
        writer.Clear();

        writer.Locate(2, 2);
        writer.SetForegroundColor(255, 255, 0);
        writer.Write("=== READ ===");

        if (_readableItems.Count == 0)
        {
            writer.Locate(2, 4);
            writer.SetForegroundColor(128, 128, 128);
            writer.Write("You have nothing to read.");
        }
        else
        {
            int maxItems = Math.Min(_readableItems.Count, 20);
            for (int i = 0; i < maxItems; i++)
            {
                var item = _readableItems[i];
                int row = 4 + i;

                // Slot letter
                writer.Locate(2, row);
                writer.SetForegroundColor(200, 200, 200);
                writer.Write($"{(char)('a' + i)})");

                // Item glyph in its color
                writer.Locate(5, row);
                var glyph = item.Glyph;
                byte r = (byte)((glyph.ForegroundColor >> 24) & 0xFF);
                byte g = (byte)((glyph.ForegroundColor >> 16) & 0xFF);
                byte b = (byte)((glyph.ForegroundColor >> 8) & 0xFF);
                writer.SetForegroundColor(r, g, b);
                writer.Write(glyph.Character.ToString());

                // Name and count
                writer.Locate(7, row);
                writer.SetForegroundColor(200, 200, 200);
                if (item.Count > 1)
                    writer.Write($"{item.Name} (x{item.Count})");
                else
                    writer.Write(item.Name);
            }
        }

        writer.Locate(2, output.Rows - 2);
        writer.SetForegroundColor(128, 128, 128);
        writer.Write("Select item to read, or press [ESC] to cancel");

        output.SetCursor(0, 0, false);
        output.Render();
    }
}
