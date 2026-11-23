using Escoba.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Editor.Build
{
    /// <summary>Utilidades para construir jerarquías uGUI desde código de editor.</summary>
    internal static class UiFactory
    {
        public static readonly Color TextLight = new Color(0.98f, 0.96f, 0.9f, 1f);
        public static readonly Color TextDark = new Color(0.12f, 0.1f, 0.08f, 1f);
        public static readonly Color ButtonColor = new Color(0.96f, 0.93f, 0.84f, 1f);
        public static readonly Color PanelColor = new Color(0.04f, 0.12f, 0.08f, 0.92f);
        public static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.55f);
        public static readonly Color Gold = new Color(0.95f, 0.78f, 0.25f, 1f);

        private static int UiLayer => LayerMask.NameToLayer("UI");

        public static Sprite RoundedSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = UiLayer;
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Ancla un rect en un punto (0-1) del padre con posición y tamaño.</summary>
        public static RectTransform Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        /// <summary>Estira un rect para ocupar todo el padre con un margen.</summary>
        public static RectTransform Stretch(RectTransform rect, float margin = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(margin, margin);
            rect.offsetMax = new Vector2(-margin, -margin);
            return rect;
        }

        public static Image CreateImage(Transform parent, string name, Color color, Sprite sprite = null, bool raycast = false)
        {
            RectTransform rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.type = sprite != null && sprite.border != Vector4.zero ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            image.raycastTarget = raycast;
            return image;
        }

        /// <summary>Texto TMP. Si <paramref name="key"/> no es null, añade <see cref="LocalizedText"/>.</summary>
        public static TextMeshProUGUI CreateText(Transform parent, string name, string key, float size, TextAlignmentOptions alignment, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.text = key != null ? $"[{key}]" : string.Empty;
            if (key != null)
            {
                var localized = rect.gameObject.AddComponent<LocalizedText>();
                SetField(localized, "_key", key);
            }

            return text;
        }

        /// <summary>Botón con fondo redondeado y texto localizado.</summary>
        public static Button CreateButton(Transform parent, string name, string key, Vector2 size, float fontSize = 40f)
        {
            Image background = CreateImage(parent, name, ButtonColor, RoundedSprite, true);
            background.rectTransform.sizeDelta = size;
            var button = background.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.78f, 0.7f, 1f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
            button.colors = colors;
            TextMeshProUGUI label = CreateText(background.transform, "Label", key, fontSize, TextAlignmentOptions.Center, TextDark);
            Stretch(label.rectTransform, 8f);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMin = 18f;
            label.fontSizeMax = fontSize;
            return button;
        }

        /// <summary>
        /// Campo de texto TMP de una línea. El texto de ayuda (placeholder) lleva
        /// <see cref="LocalizedText"/> con <paramref name="placeholderKey"/>.
        /// </summary>
        public static TMP_InputField CreateInputField(Transform parent, string name, string placeholderKey, Vector2 size, float fontSize)
        {
            var resources = new TMP_DefaultControls.Resources { inputField = RoundedSprite };
            GameObject go = TMP_DefaultControls.CreateInputField(resources);
            go.name = name;
            foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = UiLayer;
            }

            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = size;

            var input = go.GetComponent<TMP_InputField>();
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.image.color = ButtonColor;

            var text = (TextMeshProUGUI)input.textComponent;
            text.fontSize = fontSize;
            text.color = TextDark;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.richText = false;

            var placeholder = (TextMeshProUGUI)input.placeholder;
            placeholder.fontSize = fontSize;
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.color = new Color(0.35f, 0.33f, 0.3f, 0.8f);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.text = $"[{placeholderKey}]";
            var localized = placeholder.gameObject.AddComponent<LocalizedText>();
            SetField(localized, "_key", placeholderKey);
            return input;
        }

        /// <summary>Asigna un campo serializado (referencia, cadena o array de referencias).</summary>
        public static void SetField(Object target, string field, object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new System.ArgumentException($"{target.GetType().Name} no tiene el campo '{field}'.");
            }

            switch (value)
            {
                case string s:
                    property.stringValue = s;
                    break;
                case bool b:
                    property.boolValue = b;
                    break;
                case Object[] array:
                    property.arraySize = array.Length;
                    for (int i = 0; i < array.Length; i++)
                    {
                        property.GetArrayElementAtIndex(i).objectReferenceValue = array[i];
                    }

                    break;
                case Object reference:
                    property.objectReferenceValue = reference;
                    break;
                default:
                    property.objectReferenceValue = null;
                    break;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
