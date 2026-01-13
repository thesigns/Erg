using Erg.Core.Abstractions;
using Erg.Core.Ui;
using Erg.Core.World;
using Erg.Core.World.Generators;

namespace Erg.Core.Game.Views;

public class DebugGenerationView : IGameView
{
    private readonly Game _game;
    private readonly DungeonGenerator3 _generator;
    private readonly IEnumerator<GenerationStep> _enumerator;
    private readonly List<string> _log = new();
    private readonly int _seed;
    private Area? _currentArea;
    private bool _finished;

    private const int LogLines = 5;
    private const int MapHeight = 20;

    public DebugGenerationView(Game game) : this(game, Environment.TickCount) { }

    public DebugGenerationView(Game game, int seed)
    {
        _game = game;
        _seed = seed;
        _generator = new DungeonGenerator3(80, MapHeight, _seed, level: 1);
        _enumerator = _generator.GenerateStepByStep().GetEnumerator();
        _log.Add("Press SPACE to execute next step");
        _log.Add("Press ENTER to finish generation");
        _log.Add("Press ESC to return to menu");
    }

    public void Update(IInput input)
    {
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Escape))
        {
            _game.SwitchView(new IntroView(_game));
            return;
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Space) && !_finished)
        {
            if (_enumerator.MoveNext())
            {
                var step = _enumerator.Current;
                _currentArea = step.Area;
                _log.Add(step.Message);

                // Keep only last N log entries
                while (_log.Count > 20)
                    _log.RemoveAt(0);
            }
            else
            {
                _finished = true;
                _log.Add("--- GENERATION COMPLETE (R: restart, N: new seed) ---");
            }
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Enter) && !_finished)
        {
            while (_enumerator.MoveNext())
            {
                var step = _enumerator.Current;
                _currentArea = step.Area;
            }
            _finished = true;
            _log.Add("--- GENERATION COMPLETE (R: restart, N: new seed) ---");
        }

        // Restart handlers when finished
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.R) && _finished)
        {
            _game.SwitchView(new DebugGenerationView(_game, _seed));
            return;
        }

        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.N) && _finished)
        {
            _game.SwitchView(new DebugGenerationView(_game));
            return;
        }
    }

    public void Render(IOutput output)
    {
        var writer = new Writer(output);
        writer.Clear();

        // Render map (top part)
        if (_currentArea != null)
        {
            RenderArea(output, _currentArea);
        }

        // Render separator line
        writer.Locate(0, MapHeight);
        writer.SetForegroundColor(100, 100, 100);
        writer.Write(new string('-', 80));

        // Render log (bottom part)
        RenderLog(output, writer);

        output.SetCursor(0, 0, false);
        output.Render();
    }

    private void RenderArea(IOutput output, Area area)
    {
        for (int y = 0; y < area.Height && y < MapHeight; y++)
        {
            for (int x = 0; x < area.Width; x++)
            {
                var tile = area.GetTile(x, y);
                if (tile != null)
                {
                    output.PutGlyph(x, y, tile.GetDisplayGlyph());
                }
            }
        }
    }

    private void RenderLog(IOutput output, Writer writer)
    {
        int startLine = MapHeight + 1;
        int linesToShow = Math.Min(_log.Count, LogLines);
        int startIndex = Math.Max(0, _log.Count - linesToShow);

        for (int i = 0; i < linesToShow; i++)
        {
            writer.Locate(0, startLine + i);
            writer.SetForegroundColor(200, 200, 200);

            string line = _log[startIndex + i];
            if (line.Length > 80)
                line = line.Substring(0, 77) + "...";

            writer.Write(line);
        }

        // Show seed on the left
        writer.Locate(3, MapHeight);
        writer.SetForegroundColor(255, 255, 0);
        writer.Write($"[Seed: {_seed}]");

        // Show step counter on the right
        writer.Locate(68, MapHeight);
        writer.Write($"[Step {_log.Count - 2}]");
    }
}
