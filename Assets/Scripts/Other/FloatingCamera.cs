using UnityEngine;

public class FloatingCamera : MonoBehaviour
{
    [Header("Floating Movement")]
    public float floatAmplitude = 0.1f;
    public float floatFrequency = 0.1f;

    [Header("Noise Shake")]
    public float noiseAmplitude = 0.1f;
    public float noiseFrequency = 0.1f;

    [Header("Base Position Offset")]
    public Vector3 offset = new Vector3(0, 10, -10);

    private Vector3 basePosition;

    void Start()
    {
        basePosition = transform.position; // fallback n?u không set t? ngoài
    }

    void Update()
    {
        float floatY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        float noiseX = (Mathf.PerlinNoise(Time.time * noiseFrequency, 0f) - 0.5f) * 2f * noiseAmplitude;
        float noiseZ = (Mathf.PerlinNoise(0f, Time.time * noiseFrequency) - 0.5f) * 2f * noiseAmplitude;

        Vector3 floatOffset = new Vector3(noiseX, floatY, noiseZ);

        transform.position = basePosition + offset + floatOffset;
    }

    /// ? G?i t? LevelManager ?? c?p nh?t v? trí m?i
    public void SetBasePosition(Vector3 newBasePosition)
    {
        basePosition = newBasePosition;
    }
}
