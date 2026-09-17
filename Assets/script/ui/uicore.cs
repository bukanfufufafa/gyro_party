using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class CoreMenuUI : MonoBehaviour
{
    [System.Serializable]
    public class MenuEntry
    {
        public string name;
        public RectTransform panel;
        public Vector3 defaultPosition;
        public Vector3 targetPosition;
        public Button triggerButton;
        public Image buttonBackground;
        public TMP_Text buttonText;

        [Header("Additional Panels (ikut bergerak)")]
        public List<RectTransform> additionalPanels;
        public Vector3 additionalDefaultPos;
        public Vector3 additionalTargetPos;

        [Header("Warna Aktif & Normal")]
        public Color activeButtonColor = new Color(1f, 0.9f, 0.5f);
        public Color activeTextColor = new Color(0.3f, 0f, 0f);
        public Color normalButtonColor = Color.white;
        public Color normalTextColor = Color.black;

        [Header("Opsi Visual Aktif")]
        public bool useActiveVisual = true;

        [Header("Default Aktif Saat Start")]
        public bool isDefaultActive = false;
    }

    [Header("Menu Entries")]
    public List<MenuEntry> menuEntries = new List<MenuEntry>();

    [Header("Transisi")]
    public float moveDuration = 0.5f;
    public LeanTweenType easing = LeanTweenType.easeInOutCubic;

    private MenuEntry currentActiveEntry = null;

    void Start()
    {
        _ = DoStart();
    }

    private async UniTask DoStart()
    {
        await UniTask.Delay(2750);

        foreach (var entry in menuEntries)
        {
            if (entry.panel != null)
                entry.panel.anchoredPosition = entry.defaultPosition;

            foreach (var add in entry.additionalPanels)
                if (add != null)
                    add.anchoredPosition = entry.additionalDefaultPos;

            if (entry.triggerButton != null)
            {
                string menuName = entry.name;
                entry.triggerButton.onClick.AddListener(() => OnMenuClick(menuName));
            }

            SetButtonVisual(entry, false);
        }

        // Aktifkan default panel jika ada
        MenuEntry defaultEntry = menuEntries.Find(e => e.isDefaultActive);
        if (defaultEntry != null)
        {
            MoveIn(defaultEntry);
            currentActiveEntry = defaultEntry;
        }
    }

    public void OnMenuClick(string menuName)
    {
        MenuEntry clicked = menuEntries.Find(e => e.name == menuName);
        if (clicked == null) return;

        if (currentActiveEntry != null)
            MoveOut(currentActiveEntry);

        MoveIn(clicked);
        currentActiveEntry = clicked;
    }

    void MoveIn(MenuEntry entry)
    {
        if (entry.panel != null)
            LeanTween.move(entry.panel, entry.targetPosition, moveDuration).setEase(easing);

        foreach (var add in entry.additionalPanels)
            if (add != null)
                LeanTween.move(add, entry.additionalTargetPos, moveDuration).setEase(easing);

        UpdateAllButtonVisuals(entry);
    }

    void MoveOut(MenuEntry entry)
    {
        if (entry.panel != null)
            LeanTween.move(entry.panel, entry.defaultPosition, moveDuration).setEase(easing);

        foreach (var add in entry.additionalPanels)
            if (add != null)
                LeanTween.move(add, entry.additionalDefaultPos, moveDuration).setEase(easing);
    }

    void UpdateAllButtonVisuals(MenuEntry newlyActiveEntry)
    {
        if (!newlyActiveEntry.useActiveVisual)
            return;

        foreach (var entry in menuEntries)
        {
            bool isActive = (entry == newlyActiveEntry);
            SetButtonVisual(entry, isActive);
        }
    }

    void SetButtonVisual(MenuEntry entry, bool isActive)
    {
        if (!entry.useActiveVisual && !isActive)
            return;

        if (entry.buttonBackground != null)
            entry.buttonBackground.color = isActive ? entry.activeButtonColor : entry.normalButtonColor;

        if (entry.buttonText != null)
            entry.buttonText.color = isActive ? entry.activeTextColor : entry.normalTextColor;
    }
}
