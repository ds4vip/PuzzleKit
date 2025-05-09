namespace PuzzleKit.Backends.Maui;

using PuzzleKit.Audio;

public class MauiAudioService : PuzzleKit.Audio.IAudioService
{
    public void Initialize()
    {
        // 初期化処理（実装予定）
        System.Diagnostics.Debug.WriteLine("Audio service initialized");
    }
    
    public void PlaySound(string soundId)
    {
        // 音声再生（今回は実装せず）
        System.Diagnostics.Debug.WriteLine($"Playing sound: {soundId}");
    }
    
    public void StopSound(string soundId)
    {
        // 音声停止
    }
}