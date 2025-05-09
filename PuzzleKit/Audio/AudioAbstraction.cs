namespace PuzzleKit.Audio;

public interface IAudioService
{
    void Initialize();
    void PlaySound(string soundId);
    void StopSound(string soundId);
}