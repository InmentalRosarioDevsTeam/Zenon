using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
	//-----------------------------------------------------------------------------------
	
	//----------------------------internal
	
	public enum PlayMode
	{
        PlayOver,
        WaitLast
	};

	//-----------------------------------------------------------------------------------
	
	//----------------------------public_vars
	
    public AudioClip[] sounds;
	
    public static AudioManager instance
	{
		get
		{
			if ( _instance == null )
			{
				GameObject go = GameObject.Find("AudioManager");
				if ( go == null )
				{
					go = new GameObject();
					go.name = "AudioManager";
				}
				
				_instance = go.GetComponent<AudioManager>() ?? go.AddComponent<AudioManager>() ;

                _instance.Awake();
            }
			
			return _instance;
		}
	}
	
	//----------------------------private_vars
	
	private static Dictionary<string, AudioSource> _audios;
	private static int _sLen;
	
	private static AudioManager _instance;
    bool _awaked;

	//----------------------------------------------------------------------------
	
	private void Awake()
	{
        if (_awaked)
            return;

        _awaked = true;
        gameObject.name = "AudioManager";
        sounds = Resources.LoadAll<AudioClip>("Audio");

        _audios = new Dictionary<string, AudioSource>();
        if (sounds != null)
        {
            _sLen = sounds.Length;
            for (int i = 0; i < _sLen; i++)
                _audios.Add(sounds[i].name, (AudioSource)gameObject.AddComponent<AudioSource>());
        }
    }
	
	//----------------------------------------------------------------------------
	
    public void PlaySound(string soundKey)
    {
        PlaySound(soundKey, 1, false,PlayMode.PlayOver);
    }

    public void PlaySound( string soundKey , float volume , bool loop )
    {
        AudioSource cAsource;
       
	    if (_audios.TryGetValue(soundKey, out cAsource))
         {
           cAsource.clip =  GetSoundByName(soundKey) ;
           cAsource.volume = volume;
           if (loop) cAsource.loop = true;
           /*if ( !cAsource.isPlaying ) */cAsource.Play();
       }
	   else
		   Debug.LogWarning("[CUIDADO] en " + this.name + " no se encontro " + soundKey + " en la lista de sonidos.");
    } 
	
	
	public void PlaySound( string soundKey , float volume , bool loop , PlayMode pm )
    {
        AudioSource cAsource;
       
	    if (_audios.TryGetValue(soundKey, out cAsource))
         {
           cAsource.clip =  GetSoundByName(soundKey) ;
           cAsource.volume = volume;
           if (loop) cAsource.loop = true;
           if ( !cAsource.isPlaying || pm == PlayMode.PlayOver ) cAsource.Play();
       }
	   else
		   Debug.LogWarning("[CUIDADO] en " + this.name + " no se encontro " + soundKey + " en la lista de sonidos.");
    } 
	
	//----------------------------------------------------------------------------
	
    public void StopSound(string soundKey)
    {
        AudioSource cAsource;

        if (_audios.TryGetValue(soundKey, out cAsource))
        {
            cAsource.clip = GetSoundByName(soundKey);
            cAsource.Stop();
        }
    }
	
    public void StopAllSounds()
    {
        foreach (KeyValuePair<string, AudioSource> item in _audios)
        {
            item.Value.Stop();
        }
    }
	//----------------------------------------------------------------------------
	
    private AudioClip GetSoundByName( string soundName )
    {
        for (int i = 0; i < _sLen ; i++)
        {
            if (sounds[i].name == soundName)
                return sounds[i];
        }
        return null;
    }
	
	//----------------------------------------------------------------------------
}
