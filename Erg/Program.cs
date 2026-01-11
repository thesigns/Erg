using Erg.Platforms.Raylib;
using Erg.Core.Game;

var output = new RaylibOutput(80, 25, "Erg", "Assets/CascadiaCode-Light.ttf", 42);
var input = new RaylibInput();
var game = new Game(output, input);

while (!output.ShouldClose)
{
    game.Update();
    game.Render();
}

output.Dispose();