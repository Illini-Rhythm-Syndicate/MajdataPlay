using MajdataPlay.Databases;
using MajdataPlay.i18n;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable
namespace MajdataPlay
{
    internal sealed class CreditDisplayer : MonoBehaviour
    {
        const string CREDIT_LABEL_I18N_KEY = "MAJTEXT_CREDITS";
        const string TRACK_LABEL_I18N_KEY = "MAJTEXT_TRACK";
        const string FREE_PLAY_I18N_KEY = "MAJTEXT_FREE_PLAY";
        const string SUB_DISPLAY_NAME = "Sub_Display";
        const float FONT_HEIGHT_RATIO = 0.055f;
        const float TOP_MARGIN = 8f;
        const float RECT_WIDTH_RATIO = 0.9f;
        const float OUTLINE_WIDTH = 0.25f;
        static readonly int OUTLINE_COLOR_ID = Shader.PropertyToID("_OutlineColor");

        TextMeshProUGUI _creditText = null!;
        RectTransform? _attachedTo;

        internal static CreditDisplayer Create()
        {
            var root = new GameObject(nameof(CreditDisplayer), typeof(RectTransform));
            var displayer = root.AddComponent<CreditDisplayer>();
            DontDestroyOnLoad(root);
            return displayer;
        }

        void Awake()
        {
            CreditManager.StateChanged += OnStateChanged;
            Localization.OnLanguageChanged += OnLanguageChanged;
        }

        void OnStateChanged(object? sender, EventArgs e) => Refresh();

        void OnLanguageChanged(object? sender, Language language) => Refresh();

        void OnDestroy()
        {
            CreditManager.StateChanged -= OnStateChanged;
            Localization.OnLanguageChanged -= OnLanguageChanged;
            DestroyText();
        }

        void Start() => AttachIfNeeded();

        void LateUpdate() => AttachIfNeeded();

        void AttachIfNeeded()
        {
            var subDisplay = FindSubDisplay();
            if (subDisplay == null)
            {
                return;
            }
            var isAttached = _creditText != null
                          && _attachedTo == subDisplay
                          && _creditText.transform.parent == subDisplay;
            if (isAttached)
            {
                return;
            }
            DestroyText();
            _creditText = CreateText(subDisplay);
            _attachedTo = subDisplay;
            Refresh();
        }

        void DestroyText()
        {
            if (_creditText != null)
            {
                Destroy(_creditText.gameObject);
                _creditText = null!;
            }
            _attachedTo = null;
        }

        void Refresh()
        {
            if (_creditText == null)
            {
                return;
            }
            if (CreditManager.IsFreePlay)
            {
                _creditText.text = FREE_PLAY_I18N_KEY.i18n();
                return;
            }
            var creditLabel = CREDIT_LABEL_I18N_KEY.i18n();
            if (!CreditManager.IsSessionActive)
            {
                _creditText.text = $"{creditLabel}: {CreditManager.Credits}";
                return;
            }
            var trackLabel = TRACK_LABEL_I18N_KEY.i18n();
            _creditText.text = $"{creditLabel}: {CreditManager.Credits}   {trackLabel} {CreditManager.CurrentTrack}/{CreditManager.PlaysPerCredit}";
        }

        static RectTransform? FindSubDisplay()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                if (!root.activeInHierarchy)
                {
                    continue;
                }
                var found = FindDescendant(root.transform);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        static RectTransform? FindDescendant(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }
            if (parent.name == SUB_DISPLAY_NAME && parent is RectTransform rect && parent.gameObject.activeInHierarchy)
            {
                return rect;
            }
            var childCount = parent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var found = FindDescendant(parent.GetChild(i));
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        TextMeshProUGUI CreateText(RectTransform parent)
        {
            var height = parent.rect.height;
            var fontSize = height * FONT_HEIGHT_RATIO;

            var textObject = new GameObject("CreditText", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            ApplyTopmostSortKey(textObject.AddComponent<Canvas>());

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = GameRuntime.Instance.LocalizedFonts.Default;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.outlineWidth = OUTLINE_WIDTH;
            text.fontMaterial.SetColor(OUTLINE_COLOR_ID, Color.black);

            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -TOP_MARGIN);
            rect.sizeDelta = new Vector2(parent.rect.width * RECT_WIDTH_RATIO, fontSize);
            rect.localScale = Vector3.one;
            return text;
        }

        static void ApplyTopmostSortKey(Canvas canvas)
        {
            var layers = SortingLayer.layers;
            var topmostIndex = 0;
            var topmostOrder = int.MinValue;
            foreach (var candidate in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (candidate == null)
                {
                    continue;
                }
                var index = LayerIndexOf(candidate.sortingLayerID, layers);
                if (index < topmostIndex || index == topmostIndex && candidate.sortingOrder <= topmostOrder)
                {
                    continue;
                }
                topmostIndex = index;
                topmostOrder = candidate.sortingOrder;
            }
            canvas.overrideSorting = true;
            canvas.sortingLayerID = layers[topmostIndex].id;
            canvas.sortingOrder = topmostOrder == int.MinValue ? 0 : topmostOrder + 1;
        }

        static int LayerIndexOf(int sortingLayerID, SortingLayer[] layers)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].id == sortingLayerID)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
