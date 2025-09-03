using UnityEngine;

public enum SoundType { Music, SFX }

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public SoundType type = SoundType.SFX;
    public bool loop = false;
    [Range(0f, 1f)] public float volume = 1f;
}
