using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonAction
{
    PlaceSpecialUnit,
    CallNuke
}

public class RealeaseAndHoldUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;

    public PlayerController pc;
    public ButtonAction buttonAction;

    public Image buttonImage;
    public Image buttonImage2;
    public float pressedScale = 0.9f;
    public float scaleDuration = 0.1f;

    public Color normalColor = new Color(1f, 1f, 0f);       
    public Color disabledColor = new Color(0.74f, 0.74f, 0); 

    private bool hasClicked = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        hasClicked = false;
        buttonImage.color = normalColor;
        buttonImage2.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (hasClicked) return;

        rectTransform.DOScale(pressedScale, scaleDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (hasClicked) return;

        rectTransform.DOScale(1f, scaleDuration).SetEase(Ease.OutBack);

        switch (buttonAction)
        {
            case ButtonAction.PlaceSpecialUnit:
                pc.ButtonClickedPlaceSpecialUnit();
                break;
            case ButtonAction.CallNuke:
                pc.ButtonClickedSpawnNuke();
                break;
        }

        hasClicked = true;
        buttonImage.color = disabledColor;
        buttonImage2.color = disabledColor;
    }
}
