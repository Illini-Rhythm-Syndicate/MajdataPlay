using MajdataPlay.Databases;
using MajdataPlay.i18n;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable
namespace MajdataPlay
{
    internal sealed class CreditDisplayer : MonoBehaviour
    {
        const string CREDIT_LABEL_I18N_KEY = "MAJTEXT_CREDITS";
        const float REFERENCE_HEIGHT = 1920f;
        const float FONT_SIZE = 56f;
        const float MARGIN = 48f;
        const float WIDTH_RATIO = 0.6f;
        const int SORTING_ORDER = short.MaxValue;

        RectTransform _canvasRect = null!;
        RectTransform _textRect = null!;
        TextMeshProUGUI _creditText = null!;

        internal static CreditDisplayer Create()
        {
            var root = new GameObject(nameof(CreditDisplayer), typeof(RectTransform));
            var displayer = root.AddComponent<CreditDisplayer>();
            DontDestroyOnLoad(root);
            return displayer;
        }

        void Awake()
        {
            _canvasRect = (RectTransform)transform;
            _canvasRect.anchorMin = Vector2.zero;
            _canvasRect.anchorMax = Vector2.one;
            _canvasRect.offsetMin = Vector2.zero;
            _canvasRect.offsetMax = Vector2.zero;

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = SORTING_ORDER;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            _creditText = CreateText(_canvasRect);
            _textRect = _creditText.rectTransform;

            CreditManager.StateChanged += Refresh;
            Localization.OnLanguageChanged += OnLanguageChanged;
        }

        void OnDestroy()
        {
            CreditManager.StateChanged -= Refresh;
            Localization.OnLanguageChanged -= OnLanguageChanged;
        }

        void Start() => Refresh(null, EventArgs.Empty);

        void LateUpdate() => Layout();

        void OnLanguageChanged(object? sender, Language language) => Refresh(sender, EventArgs.Empty);

        void Refresh(object? sender, EventArgs e)
        {
            if (_creditText is null)
            {
                return;
            }
            var label = CREDIT_LABEL_I18N_KEY.i18n();
            _creditText.text = label + ": " + CreditManager.Credits;
        }

        void Layout()
        {
            var height = _canvasRect.rect.height;
            if (height <= 0f)
            {
                return;
            }
            var scale = height / REFERENCE_HEIGHT;
            var margin = MARGIN * scale;
            _creditText.fontSize = FONT_SIZE * scale;
            _textRect.anchoredPosition = new Vector2(-margin, -margin);
            _textRect.sizeDelta = new Vector2(_canvasRect.rect.width * WIDTH_RATIO, FONT_SIZE * scale * 2f);
        }

        static TextMeshProUGUI CreateText(Transform parent)
        {
            var textObject = new GameObject("CreditText", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = GameRuntime.Instance.LocalizedFonts.Default;
            text.alignment = TextAlignmentOptions.TopRight;
            text.color = Color.white;
            text.raycastTarget = false;
            text.overflowMode = TextOverflowModes.Overflow;

            var rectTransform = text.rectTransform;
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            return text;
        }
    }
}
