using Erg.Core.Abstractions;
using Erg.Core.Game.Views;

namespace Erg.Core.Game;

public class Game
{
    private IOutput _output;
    private IInput _input;
    private IGameView _currentView = null!;
    private readonly CheatKeys _cheatKeys = new();

    private Session _currentSession = null!;
    public Session CurrentSession => _currentSession;
    
    public Game(IOutput output, IInput input)
    {
        _output = output;
        _input = input;
        
        // Zainicjalizuj pierwszy view
        SwitchView(new IntroView(this));
    }
    
    public void SwitchView(IGameView newView)
    {
        _currentView?.OnExit();
        _currentView = newView;
        _currentView.OnEnter();
    }
    
    public void Update()
    {
        _input.Update();
        _cheatKeys.Update(_input, _currentSession);
        _currentView.Update(_input);
    }
    
    public void Render()
    {
        _currentView.Render(_output);
    }

    public void NewSession()
    {
        _currentSession = new Session(SessionConfig.QuickStart());
        SwitchView(new PlayView(this));
    }
}