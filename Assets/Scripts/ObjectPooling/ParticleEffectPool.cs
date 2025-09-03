using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectPool : MonoBehaviour
{
    public static ParticleEffectPool Instance;

    [Header("Particle Prefab")]
    public ParticleSystem deathEffectPrefab;

    [Header("Pool Settings")]
    public int poolSize = 10;

    private Queue<ParticleSystem> pool = new Queue<ParticleSystem>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            ParticleSystem effect = Instantiate(deathEffectPrefab, transform);
            effect.gameObject.transform.SetParent(transform);
            effect.gameObject.SetActive(false);
            pool.Enqueue(effect);
        }
    }

    public ParticleSystem GetFromPool()
    {
        if (pool.Count > 0)
        {
            ParticleSystem effect = pool.Dequeue();
            effect.gameObject.SetActive(true);
            return effect;
        }

        ParticleSystem newEffect = Instantiate(deathEffectPrefab, transform);
        return newEffect;
    }

    public void ReturnToPool(ParticleSystem effect)
    {
        effect.Stop();
        effect.gameObject.SetActive(false);
        pool.Enqueue(effect);
    }

    // ?? G?i t? enemy khi ch?t
    public void SpawnDeathEffect(Vector3 position)
    {
        ParticleSystem effect = GetFromPool();
        effect.transform.position = position;
        effect.transform.rotation = Quaternion.identity;
        effect.Play();

        StartCoroutine(ReturnEffectToPool(effect, effect.main.duration));
    }

    public IEnumerator ReturnEffectToPool(ParticleSystem effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(effect);
    }
}
