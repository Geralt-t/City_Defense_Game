using UnityEngine;
using DG.Tweening;

public class EnergyPopupUI : MonoBehaviour
{
    public float floatDistance = 50f;
    public float duration = 0.5f;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f; 

        rect.DOAnchorPosY(rect.anchoredPosition.y + floatDistance, duration).SetEase(Ease.OutQuad);
        canvasGroup.DOFade(0.3f, duration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

}
