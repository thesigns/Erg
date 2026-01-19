using Erg.Core.Abstractions;
using Erg.Core.Systems;
using Erg.Core.Ui;

namespace Erg.Core.Game.Views;

public class SkillsView : IGameView
{
    private readonly Game _game;
    private readonly IGameView _previousView;

    public SkillsView(Game game, IGameView previousView)
    {
        _game = game;
        _previousView = previousView;
    }

    public void OnEnter() { }
    public void OnExit() { }

    public void Update(IInput input)
    {
        if (input.KeyPulse.GetValueOrDefault(KeyboardKey.Escape))
        {
            _game.SwitchView(_previousView);
        }
    }

    public void Render(IOutput output)
    {
        var writer = new Writer(output);
        var player = _game.CurrentSession.Player;
        var skills = player.Skills;

        writer.Clear();

        // Title
        writer.Locate(2, 2);
        writer.SetForegroundColor(255, 255, 0);
        writer.Write("=== SKILLS ===");

        // Column headers
        writer.SetForegroundColor(100, 200, 200);
        writer.Locate(2, 4);
        writer.Write("-- Combat Skills --");
        writer.Locate(42, 4);
        writer.Write("-- Other Skills --");

        // Combat Skills
        int row = 6;
        WriteSkillLine(writer, 2, row++, "Attack", skills.AttackSkill);
        WriteSkillLine(writer, 2, row++, "Defense", skills.DefenseSkill);
        WriteSkillLine(writer, 2, row++, "Unarmed", skills.UnarmedSkill);

        // Other Skills
        row = 6;
        WriteSkillLine(writer, 42, row++, "Literacy", skills.LiteracySkill);
        WriteSkillLine(writer, 42, row++, "Swimming", skills.SwimmingSkill);

        // Footer
        writer.Locate(2, output.Rows - 2);
        writer.SetForegroundColor(128, 128, 128);
        writer.Write("Press [ESC] to close");

        output.SetCursor(0, 0, false);
        output.Render();
    }

    private void WriteSkillLine(Writer writer, int col, int row, string name, Skill skill)
    {
        writer.Locate(col, row);
        writer.SetForegroundColor(200, 200, 200);
        writer.Write(name.PadRight(12));

        // Skill value (green)
        writer.SetForegroundColor(100, 255, 100);
        writer.Write($"{skill.DisplayValue}%");
    }
}
