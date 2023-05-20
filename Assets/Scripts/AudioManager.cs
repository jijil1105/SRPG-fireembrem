using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    }

    [SerializeField]
    private SoundData[] soundDatas;

    private AudioSource[] audioSourceList = new AudioSource[20];

    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();

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


    private AudioSource GetUnusedAudioSource() =>
        audioSourceList.FirstOrDefault(audiosource => audiosource.isPlaying == false);

    public void Play(AudioClip clip)
    {
        var audioSouce = GetUnusedAudioSource();
        if (audioSouce == null) return;
        audioSouce.clip = clip;
        audioSouce.Play();
    }

    public void Play(string name)
    {
        if(soundDictionary.TryGetValue(name, out var soundData))
        {
            if(soundData.playedTime==0)
            {
                soundData.playedTime = Time.realtimeSinceStartup;
                Play(soundData.audioClip);
                return;
            }

            if (Time.realtimeSinceStartup - soundData.playedTime < soundData.playableDistance) return;

            soundData.playedTime = Time.realtimeSinceStartup;
            Play(soundData.audioClip);
        }
        else
        {
            Debug.LogWarning($"その別名は登録されていません:{name}");
        }
    }
}
