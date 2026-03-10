using System;
using UnityEngine;
using KSP;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    /// <summary>
    /// Simple button with optional icon and text, using the standard KSP button (no custom drawing).
    /// </summary>
    public class VesselBookmarkButton {

        public Texture2D Icon { get; private set; } = null;
        public Texture2D IconHover { get; private set; } = null;
        public Texture2D IconClicked { get; private set; } = null;
        public Texture2D IconDisabled { get; private set; } = null;
        public string Tooltip { get; private set; }
        public string Label { get; private set; }
        public int IconWidth { get; private set; }
        public int IconHeight { get; private set; }
        public float? ButtonWidth { get; private set; }

        private bool _isPressed;

        public static VesselBookmarkButtonBuilder Builder() {
            return new VesselBookmarkButtonBuilder();
        }

        public sealed class VesselBookmarkButtonBuilder {
            internal string _iconPath;
            internal string _tooltip = "";
            internal string _label = null;
            internal int _iconWidth = 20;
            internal int _iconHeight = 20;
            internal float? _buttonWidth = null;

            internal VesselBookmarkButtonBuilder() { }

            public VesselBookmarkButtonBuilder WithIconPath(string iconPath) {
                _iconPath = iconPath;
                return this;
            }

            public VesselBookmarkButtonBuilder WithTooltip(string tooltip) {
                _tooltip = tooltip ?? "";
                return this;
            }

            public VesselBookmarkButtonBuilder WithLabel(string label) {
                _label = label ?? "";
                return this;
            }

            public VesselBookmarkButtonBuilder WithIconSize(int width, int height) {
                _iconWidth = width;
                _iconHeight = height;
                return this;
            }

            public VesselBookmarkButtonBuilder WithButtonWidth(float width) {
                _buttonWidth = width;
                return this;
            }

            public VesselBookmarkButton Build() {
                return new VesselBookmarkButton(this);
            }
        }

        private VesselBookmarkButton(VesselBookmarkButtonBuilder builder) {
            if (!string.IsNullOrEmpty(builder._iconPath)) {
                Icon = GameDatabase.Instance.GetTexture(builder._iconPath, false);
                IconHover = GameDatabase.Instance.GetTexture(builder._iconPath + "_hover", false);
                IconClicked = GameDatabase.Instance.GetTexture(builder._iconPath + "_clicked", false);
                IconDisabled = GameDatabase.Instance.GetTexture(builder._iconPath + "_disabled", false);
            }
            if (IconHover == null) IconHover = Icon;
            if (IconClicked == null) IconClicked = Icon;
            if (IconDisabled == null) IconDisabled = Icon;

            Tooltip = string.IsNullOrEmpty(builder._tooltip) ? null : builder._tooltip;
            Label = string.IsNullOrEmpty(builder._label) ? null : builder._label;
            IconWidth = builder._iconWidth;
            IconHeight = builder._iconHeight;
            ButtonWidth = builder._buttonWidth;
        }

        private GUIStyle ButtonStyle => HighLogic.Skin?.button ?? GUI.skin.button;

        /// <summary>When no label, use zero padding so the icon fills the button.</summary>
        private GUIStyle GetStyle() {
            if (!string.IsNullOrEmpty(Label)) return ButtonStyle;
            GUIStyle noPadding = new GUIStyle(ButtonStyle);
            noPadding.padding = new RectOffset(0, 0, 0, 0);
            return noPadding;
        }

        private float GetTotalWidth() {
            if (ButtonWidth.HasValue) return ButtonWidth.Value;
            bool hasIcon = Icon != null;
            if (string.IsNullOrEmpty(Label)) return hasIcon ? IconWidth : 40f;
            float textW = ButtonStyle.CalcSize(new GUIContent(Label)).x;
            return (hasIcon ? IconWidth + 4 : 0) + 12 + textW;
        }

        private float GetTotalHeight() {
            return string.IsNullOrEmpty(Label) ? IconHeight : IconHeight + 8;
        }

        private GUIContent BuildContent(Texture2D icon) {
            string text = Label ?? "";
            if (icon != null && text.Length > 0) text = " " + text;
            return new GUIContent(text, icon, Tooltip);
        }

        public bool Draw(Func<bool> isVisible, Action onClick) {
            if (!isVisible()) {
                DrawHidden();
                return false;
            }
            if (onClick == null) {
                DrawDisabled();
                return false;
            }
            float w = GetTotalWidth();
            float h = GetTotalHeight();
            Rect r = GUILayoutUtility.GetRect(w, h, GUILayout.Width(w), GUILayout.Height(h));
            bool hover = r.Contains(Event.current.mousePosition);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && hover) _isPressed = true;
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0) _isPressed = false;
            Texture2D icon = (_isPressed && hover) ? IconClicked : (hover ? IconHover : Icon);
            if (icon == null) icon = Icon;
            GUIContent content = BuildContent(icon);
            if (GUI.Button(r, content, GetStyle())) {
                _isPressed = false;
                onClick();
                return true;
            }
            return false;
        }

        public void DrawDisabled() {
            float w = GetTotalWidth();
            float h = GetTotalHeight();
            Rect r = GUILayoutUtility.GetRect(w, h, GUILayout.Width(w), GUILayout.Height(h));
            GUIContent content = BuildContent(IconDisabled ?? Icon);
            bool wasEnabled = GUI.enabled;
            GUI.enabled = false;
            GUI.Button(r, content, GetStyle());
            GUI.enabled = wasEnabled;
        }

        public void DrawHidden() {
            float w = GetTotalWidth();
            float h = GetTotalHeight();
            GUILayoutUtility.GetRect(w, h, GUILayout.Width(w), GUILayout.Height(h));
        }
    }
}
