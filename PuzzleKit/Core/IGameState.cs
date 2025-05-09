namespace PuzzleKit.Core;

public interface IGameState
{
    void Enter();
    void Update(GameTime gameTime);
    void Draw(Graphics.ICanvas canvas);
    void Exit();
}