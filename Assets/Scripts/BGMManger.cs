using UnityEngine;

// 单例模式：保证全局只有一个BGM管理器，场景切换不销毁
public class BGMManger : MonoBehaviour
{
    public static BGMManger instance; // 单例实例
    public AudioSource audioSource;   // 音频播放组件
    public AudioClip bgmClip;         // BGM音频文件

    void Awake()
    {
        // 单例逻辑：确保只有一个实例，切换场景不销毁
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换时保留该物体
        }
        else
        {
            Destroy(gameObject); // 重复创建则销毁
            return;
        }

        // 初始化音频组件
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        audioSource.clip = bgmClip;
        audioSource.loop = true; // 循环播放
    }

    void Start()
    {
        PlayBGM(); // 游戏启动自动播放
    }

    // 播放BGM
    public void PlayBGM()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log("BGM开始播放");
        }
    }

    // 暂停BGM
    public void PauseBGM()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("BGM暂停");
        }
    }

    // 继续播放BGM
    public void ResumeBGM()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.UnPause();
            Debug.Log("BGM继续播放");
        }
    }

    // 停止BGM（完全停止，再次播放会从头开始）
    public void StopBGM()
    {
        audioSource.Stop();
        Debug.Log("BGM停止");
    }
}