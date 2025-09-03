using System;
using UnityEngine;
using DG.Tweening;
public enum AnimShowPopUp
{
    None,
    MoveMent,
    ScalePunch
}
public abstract class PopupUI : MonoBehaviour
{

    protected UIManagerBase uiManager;
    protected Action onClose;
    protected Action onHide;
    public static event Action<PopupUI> OnDestroyPopup;
    public static event Action<PopupUI> OnHide;
    public static event Action<PopupUI> OnShow;
    public bool isCache = false;
    public bool isAniClose = true;
    [SerializeField] private AnimShowPopUp animType;
    [SerializeField] protected RectTransform mainPopUp;

    public bool isShowing { get; protected set; }
    public virtual void Initialize(UIManagerBase manager)
    {
        this.uiManager = manager;
        gameObject.SetActive(false);
        isShowing = false;
    }
    public virtual void Show(Action onClose)
    {
        this.onClose = onClose;
        isShowing = true;
        if (mainPopUp)
        {
            switch (animType)
            {
                case AnimShowPopUp.MoveMent:
                    mainPopUp.anchoredPosition = new Vector2(-2000, mainPopUp.anchoredPosition.y);
                    mainPopUp.DOAnchorPos(new Vector2(0, mainPopUp.anchoredPosition.y), 0.3f).SetEase(Ease.OutQuad);
                    break;
                case AnimShowPopUp.ScalePunch:
                    mainPopUp.localScale = Vector3.zero;
                    mainPopUp.DOScale(1.1f, 0.3f).OnComplete(() =>
                    {
                        mainPopUp.DOScale(1, 0.1f);
                    });
                    break;
            }
        }

        gameObject.SetActive(true);

        OnShow?.Invoke(this);
    }
    public virtual void Hide()
    {
        if (!isShowing)
        {
            return;
        }
        //   AudioManager.Instance.PlayOneShot("SFX_ClosePopup", 1f);
        isShowing = false;
        float time = 0;
        if (mainPopUp && isAniClose)
        {
            switch (animType)
            {
                case AnimShowPopUp.MoveMent:
                    mainPopUp.DOAnchorPos(new Vector2(-2000, mainPopUp.anchoredPosition.y), 0.3f).SetEase(Ease.Linear).SetId(this);
                    time = .32f;
                    break;
                case AnimShowPopUp.ScalePunch:
                    mainPopUp.DOScale(1.1f, 0.1f).OnComplete(() =>
                    {
                        mainPopUp.DOScale(0, 0.3f).SetEase(Ease.OutQuart).SetId(this);
                    }).SetId(this);
                    time = .42f;
                    break;

            }

        }
        DOVirtual.DelayedCall(time, () =>
        {
            gameObject.SetActive(false);
            onClose?.Invoke();
            onHide?.Invoke();
            onClose = null;
            OnHide?.Invoke(this);
            if (!isCache)
            {
                OnDestroyPopup?.Invoke(this);
                OnPopupDestroyed();
            }
        }).SetId(this);

    }
    protected virtual void OnPopupDestroyed()
    {

    }
}