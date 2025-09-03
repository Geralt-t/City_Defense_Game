using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnergyInGame : MonoBehaviour
{
    public static EnergyInGame Instance;
    public Slider energySlider;   // Thay vì Image
    public float maxEnergy = 300f;
    public float currentEnergy = 300f;
    public float rechargeRate = 1f;
    public TextMeshProUGUI energyText;
    Vector2 originalPos;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        originalPos = energySlider.GetComponent<RectTransform>().anchoredPosition;
        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.value = currentEnergy;
            energyText.text = $"{Mathf.FloorToInt(currentEnergy)} / {maxEnergy}";
        }
    }

    private void Update()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += rechargeRate * Time.deltaTime;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
            UpdateEnergyBar();
        }
    }
    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        UpdateEnergyBar();
    }
    public void SpendEnergy(float amount)
    {
        currentEnergy -= amount;
        currentEnergy = Mathf.Max(currentEnergy, 0);
        UpdateEnergyBar();
    }

    public bool CanAfford(float amount)
    {
        return currentEnergy >= amount;
    }
    public void GetEnergy(float amount)
    {
        currentEnergy += amount;

        currentEnergy = Mathf.Min(currentEnergy, 300);
        UpdateEnergyBar();
    }

    private void UpdateEnergyBar()
    {
        if (energySlider != null)
        {
            energySlider.value = currentEnergy;
        }
        if (energyText != null)
        {
            energyText.text = $"{Mathf.FloorToInt(currentEnergy)} / {maxEnergy}";  // Hi?n th? s? nguyên
        }
    }
    private Coroutine shakeCoroutine = null;

    public void CantAffordAnim()
    {
       
        {
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[EnergyInGame] Không th? ch?y CantAffordAnim vì GameObject không ho?t ??ng.");
                return; 
            }
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            if (energySlider.GetComponent<RectTransform>() != null)
            {
                shakeCoroutine = StartCoroutine(ShakeTransform(energySlider.GetComponent<RectTransform>(), 0.5f, 10f));
            }
            else
            {
            }
        }
    }

    private IEnumerator ShakeTransform(RectTransform target, float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.5f, 0.5f) * magnitude;
            float y = Random.Range(-0.5f, 0.5f) * magnitude;

            target.anchoredPosition = originalPos + new Vector2(x, y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.anchoredPosition = originalPos;
        shakeCoroutine = null;
    }
}
