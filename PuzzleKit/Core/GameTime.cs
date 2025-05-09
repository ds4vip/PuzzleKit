namespace PuzzleKit.Core;

/// <summary>
/// ゲームループ用の状態（遅延やスキップフレーム情報も保持）
/// </summary>
public class GameTime
{
    /// <summary>起動からの累積経過時間（秒）</summary>
    public float TotalTime { get; set; }

    /// <summary>今回のUpdateで経過した仮想時間（常にTargetElapsedTime・単位秒）</summary>
    public float ElapsedTime { get; set; }

    /// <summary>実際にタイマーで経過した実時間（秒）</summary>
    public float RealElapsedTime { get; set; }

    /// <summary>スキップされた描画フレーム数</summary>
    public int SkippedRenderFrames { get; set; }

    /// <summary>Updateが1フレームで複数回呼ばれて遅延が発生している場合true</summary>
    public bool IsLagging { get; set; }
}