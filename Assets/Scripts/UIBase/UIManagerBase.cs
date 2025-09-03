using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
#if UNITY_EDITOR
namespace MyUI.EditorClass
{
    using UnityEditor;
    [CustomEditor(typeof(UIManagerBase))]
    public class UIManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Get References"))
            {
                UIManagerBase u = target as UIManagerBase;
                u.canvasScaler = u.GetComponentsInChildren<CanvasScaler>(true);
                // u.popups = FindObjectsOfType<PopupUI>(true);
                PopupUI[] popups = Resources.LoadAll<PopupUI>("UI/Popups/");
                for (int i = 0; i < popups.Length; i++)
                {
                    string nname = popups[i].GetType().Name;
                    Debug.Log(nname);
                    popups[i].gameObject.name = nname;
                    string pa = AssetDatabase.GetAssetPath(popups[i].gameObject);
                    AssetDatabase.RenameAsset(pa, nname);
                    AssetDatabase.SaveAssetIfDirty(popups[i].gameObject);
                }
                ScreenUI[] screens = Resources.LoadAll<ScreenUI>("UI/Screens/");
                for (int i = 0; i < screens.Length; i++)
                {
                    string nname = screens[i].GetType().Name;
                    Debug.Log(nname);
                    screens[i].gameObject.name = nname;
                    string pa = AssetDatabase.GetAssetPath(screens[i].gameObject);
                    AssetDatabase.RenameAsset(pa, nname);
                    AssetDatabase.SaveAssetIfDirty(screens[i].gameObject);
                }
                EditorUtility.SetDirty(u);
            }
        }
    }
}
#endif
public class UIManagerBase : MonoBehaviour
{
    public static UIManagerBase instance;
    public Camera UICamera;
    public Canvas canvas;
    public RectTransform screenHolder;
    public RectTransform popupHolder;
    public CanvasScaler[] canvasScaler;
    [SerializeField] List<PopupUI> listPopupCached;
    [SerializeField] List<PopupUI> listPopupExist;
    [SerializeField] List<ScreenUI> listScreenCached;
    [SerializeField] List<ScreenUI> listScreenExist;
    public ScreenUI CurrentScreen { get; private set; }
    public static event Action<Vector2> OnChangeScreen;
    public Vector2 currentScreenSize;
    public static bool IsFoldScreen { get; private set; }
    private bool ready;
    public Vector2 GetCanvasSize()
    {
        return canvas.GetComponent<RectTransform>().sizeDelta;
    }

    private void Awake()
    {
        instance = this;
    }

    public void Initialize()
    {
        listPopupCached = popupHolder.GetComponentsInChildren<PopupUI>(true).ToList();
        listScreenCached = screenHolder.GetComponentsInChildren<ScreenUI>(true).ToList();
        IsFoldScreen = false;
        for (int i = 0; i < canvasScaler.Length; i++)
        {
            canvasScaler[i].matchWidthOrHeight = 0;
            // if (mygame.sdk.SdkUtil.isiPad())
            // {
            //     canvasScaler[i].matchWidthOrHeight = 1f;
            // }
#if UNITY_EDITOR
            // else
            // {
            //     float w = Screen.width;
            //     float h = Screen.height;
            //     float aspect = w / h;
            //     if (aspect > 1.6f)
            //     {
            //         canvasScaler[i].matchWidthOrHeight = 0.8f;
            //     }
            // }
#endif

        }
        currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize.x / currentScreenSize.y < 1f && currentScreenSize.x / currentScreenSize.y > 0.8f)
        {
            IsFoldScreen = true;
        }
        for (int i = 0; i < listScreenCached.Count; i++)
        {
            listScreenCached[i].Initialize(this);
        }
        for (int i = 0; i < listPopupCached.Count; i++)
        {
            listPopupCached[i].Initialize(this);
        }
        listPopupExist = new List<PopupUI>(listPopupCached);
        listScreenExist = new List<ScreenUI>(listScreenCached);
        // blockerUI.SetActive(false);

        PopupUI.OnDestroyPopup += OnPopupDestroyed;
        ScreenUI.OnDestroyScreen += OnDestroyScreen;
        ready = true;
        // GameManager.OnPause += Pause;
        // GameManager.OnResume += Resume;
    }
    public void NotifyContent(string content)
    {
        // notifyPanel.ShowNotify(content);
    }
    private void Pause()
    {
        // blockerUI.SetActive(true);
    }
    private void Resume()
    {
        // blockerUI.SetActive(false);
    }
    private void Update()
    {
        if (!ready) return;
        if (Screen.width != currentScreenSize.x && Screen.height != currentScreenSize.y)
        {
            currentScreenSize.x = Screen.width;
            currentScreenSize.y = Screen.height;
            for (int i = 0; i < canvasScaler.Length; i++)
            {
                canvasScaler[i].matchWidthOrHeight = 0;
                // if (mygame.sdk.SdkUtil.isiPad())
                // {
                //     canvasScaler[i].matchWidthOrHeight = 1f;
                // }
            }
            if (currentScreenSize.x / currentScreenSize.y < 1f && currentScreenSize.x / currentScreenSize.y > 0.8f)
            {
                IsFoldScreen = true;
            }
            else
            {
                IsFoldScreen = false;
            }
            for (int i = 0; i < canvasScaler.Length; i++)
            {
                canvasScaler[i].matchWidthOrHeight = 0;
                // if (mygame.sdk.SdkUtil.isiPad())
                // {
                //     canvasScaler[i].matchWidthOrHeight = 1f;
                // }
#if UNITY_EDITOR
                // else
                // {
                //     float w = Screen.width;
                //     float h = Screen.height;
                //     float aspect = w / h;
                //     if (aspect > 1.6f)
                //     {
                //         canvasScaler[i].matchWidthOrHeight = 0.8f;
                //     }
                // }
#endif
            }
            OnChangeScreen?.Invoke(currentScreenSize);
        }
    }
    private void OnPopupDestroyed(PopupUI obj)
    {
        if (listPopupCached.Contains(obj))
        {
            listPopupCached.Remove(obj);
        }
        listPopupExist.Remove(obj);
        Destroy(obj.gameObject);
    }
    private void OnDestroyScreen(ScreenUI screen)
    {
        if (listScreenCached.Contains(screen))
        {
            listScreenCached.Remove(screen);
        }
        if (listScreenExist.Contains(screen))
        {
            listScreenExist.Remove(screen);
        }
        Destroy(screen.gameObject);
    }
    public T ShowScreen<T>() where T : ScreenUI
    {
        if (CurrentScreen)
        {
            CurrentScreen.Deactive();
        }
        for (int i = 0; i < listScreenCached.Count; i++)
        {
            if (listScreenCached[i] is T)
            {
                CurrentScreen = listScreenCached[i];
                listScreenCached[i].Active();
                listScreenCached[i].transform.SetAsLastSibling();
                return listScreenCached[i].GetComponent<T>();
            }
        }
        T screen = CreateScreen<T>();
        CurrentScreen = screen;
        screen.Active();
        screen.transform.SetAsLastSibling();
        return screen;
    }
    private T CreateScreen<T>() where T : ScreenUI
    {
        string screenName = typeof(T).Name;
        T screen = Instantiate(Resources.Load<T>("UI/Screens/" + screenName), screenHolder);
        listScreenExist.Add(screen);
        if (screen.isCache)
        {
            listScreenCached.Add(screen);
        }
        screen.Initialize(this);
        return screen;
    }
    public T GetScreen<T>() where T : ScreenUI
    {
        T screen;
        for (int i = 0; i < listScreenExist.Count; i++)
        {
            if (listScreenExist[i] is T)
            {
                screen = listScreenExist[i].GetComponent<T>();
                return screen;
            }
        }
        screen = CreateScreen<T>();
        return screen;
    }
    public T GetScreenActive<T>() where T : ScreenUI
    {
        T screen = default;
        for (int i = 0; i < listScreenExist.Count; i++)
        {
            if (listScreenExist[i] is T)
            {
                screen = listScreenExist[i].GetComponent<T>();
                return screen;
            }
        }
        return screen;
    }
    public T ShowPopup<T>(Action onClose) where T : PopupUI
    {
        for (int i = 0; i < listPopupCached.Count; i++)
        {
            if (listPopupCached[i] is T)
            {
                listPopupCached[i].Show(onClose);
                listPopupCached[i].transform.SetAsLastSibling();
                return listPopupCached[i].GetComponent<T>();
            }
        }
        T popup = CreatePopup<T>();
        popup.Show(onClose);
        popup.transform.SetAsLastSibling();
        return popup;
    }
    public void CloseAllPopup()
    {
        for (int i = 0; i < listPopupCached.Count; i++)
        {
            listPopupCached[i].Hide();
        }
        for (int i = 0; i < listPopupExist.Count; i++)
        {
            listPopupExist[i].Hide();
        }
    }
    private T CreatePopup<T>() where T : PopupUI
    {
        string popupName = typeof(T).Name;
        T popup = Instantiate(Resources.Load<T>("UI/Popups/" + popupName), popupHolder);
        listPopupExist.Add(popup);
        if (popup.isCache)
        {
            listPopupCached.Add(popup);
        }
        popup.Initialize(this);
        return popup;
    }
    public T GetPopup<T>() where T : PopupUI
    {
        T popup = default;
        for (int i = 0; i < listPopupCached.Count; i++)
        {
            if (listPopupCached[i] is T)
            {
                popup = listPopupCached[i].GetComponent<T>();
                return popup;
            }
        }
        popup = CreatePopup<T>();
        return popup;
    }
    public T GetPopupActive<T>() where T : PopupUI
    {
        T popup = default;
        for (int i = 0; i < listPopupCached.Count; i++)
        {
            if (listPopupCached[i] is T)
            {
                popup = listPopupCached[i].GetComponent<T>();
                return popup;
            }
        }
        return popup;
    }
    public bool HasPopupShowing()
    {
        foreach (var item in listPopupExist)
        {
            if (item.isShowing) return true;
        }
        return false;
    }
}
public static class CanvasPositioningExtensions
{
    public static Vector3 WorldToCanvasPosition(this Canvas canvas, Vector3 worldPosition, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }
        var viewportPosition = camera.WorldToViewportPoint(worldPosition);
        return canvas.ViewportToCanvasPosition(viewportPosition);
    }

    public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition)
    {
        var viewportPosition = new Vector3(screenPosition.x / Screen.width, screenPosition.y / Screen.height, 0);
        return canvas.ViewportToCanvasPosition(viewportPosition);
    }

    public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition)
    {
        var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
        var canvasRect = canvas.GetComponent<RectTransform>();
        var scale = canvasRect.sizeDelta;
        return Vector3.Scale(centerBasedViewPortPosition, scale);
    }

}