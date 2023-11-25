using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundData
    {
        public string name;
        public AudioClip audioClip;
        public float playedTime; //前回再生した時間
        public float playableDistance = 0.2f;//一度再生してから、次再生出来るまでの間隔(秒)
        public string type;
    }

    [SerializeField]
    private SoundData[] soundDatas;

    private AudioSource[] audioSourceList = new AudioSource[20];

    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();

    public AudioMixer audioMixer;

    public enum ParamType
    {
        Master,
        BGM,
        SE
    }

    [SerializeField]
    public float initVolume;

    //-------------------------------------------------------------------------

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }

        for (var i = 0; i < audioSourceList.Length; ++i)
        {
            audioSourceList[i] = gameObject.AddComponent<AudioSource>();
        }

        foreach(var soundData in soundDatas)
        {
            soundDictionary.Add(soundData.name, soundData);
        }
    }

    //-------------------------------------------------------------------------

    private void Start()
    {
        audioMixer.SetFloat("BGM", 0);
        audioMixer.SetFloat("SE", 0);
        audioMixer.SetFloat("Master", 0);
    }

    /// <summary>
    /// 使用されていないオーディオソースを取得する
    /// </summary>
    /// <returns></returns>
    private AudioSource GetUnusedAudioSource() =>
        audioSourceList.FirstOrDefault(audiosource => audiosource.isPlaying == false && audiosource != null);

    private AudioSource GetUsedAudioSource(string clip_name) =>
        audioSourceList.FirstOrDefault(audiosource => audiosource.isPlaying == true && audiosource != null && audiosource.clip.name == clip_name);

    /// <summary>
    /// 引数のクリップを再生
    /// </summary>
    /// <param name="clip">再生するオーディオクリップ</param>
    public void Play(AudioClip clip, string type)
    {
        var audioSouce = GetUnusedAudioSource();
        if (audioSouce == null) return;
        audioSouce.clip = clip;
        var mixer = audioMixer.FindMatchingGroups(type)[0];
        if (mixer != null)
            audioSouce.outputAudioMixerGroup = mixer;
        else
            audioSouce.outputAudioMixerGroup = null;

        audioSouce.Play();
    }

    /// <summary>
    /// 引数の名前から検索し再生
    /// </summary>
    /// <param name="name">再生する曲名</param>
    public void Play(string name)
    {
        if(soundDictionary.TryGetValue(name, out var soundData))
        {
            if(soundData.playedTime==0)
            {
                soundData.playedTime = Time.realtimeSinceStartup;
                Play(soundData.audioClip, soundData.type);
                return;
            }

            if (Time.realtimeSinceStartup - soundData.playedTime < soundData.playableDistance) return;

            soundData.playedTime = Time.realtimeSinceStartup;
            Play(soundData.audioClip, soundData.type);
        }
        else
        {
            Debug.LogWarning($"その別名は登録されていません:{name}");
        }
    }

    public void Stop(string clip_name)
    {
        if(soundDictionary.TryGetValue(clip_name, out var soundData))
        {
            if (soundData == null)
            {
                Debug.Log(clip_name + " Dont used to audiosource");
                return;
            }

            var audiosource = GetUsedAudioSource(soundData.audioClip.name);

            if (!audiosource)
            {
                Debug.Log(clip_name + " Dont used to audiosource");
                return;
            }

            if (audiosource)
            {
                audiosource.Stop();
                audiosource.clip = null;
                soundData.playedTime = 0;
                Debug.Log("STOP");
            }     
        }

        else
            Debug.LogWarning($"その別名は登録されていません:{name}");
    }
}
