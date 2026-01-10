using Erg.Core.Abstractions;

namespace Erg.Core.Game.Views;

public interface IGameView
{
    void Update(IInput input);
    void Render(IOutput output);
    
    void OnEnter() { }
    void OnExit() { }
}