using Erg.Core.Game;
using Erg.Platforms.Sfml;

var output = new SfmlOutput(80, 25, "Erg", "Assets/CascadiaCode-Light.ttf", 32);
var input = new SfmlInput();
var game = new Game(output, input);

while (!output.ShouldClose && !game.ShouldQuit)
{
    game.Update();
    game.Render();
}

output.Dispose();