using System;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    /// <summary>
    /// Represents a simple icon with two visual states (enabled/disabled).
    /// No click, no tooltip, no hover. Use for indicators (e.g. vessel type, alarm).
    /// </summary>
    public class VesselBookmarkIcon {

        public Texture2D Icon { get; private set; }
        public Texture2D IconDisabled { get; private set; }
        public Texture2D EmptyIcon { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// Starts building an icon. Use Build() when done configuring.
        /// </summary>
        public static VesselBookmarkIconBuilder Builder() {
            return new VesselBookmarkIconBuilder();
        }

        public sealed class VesselBookmarkIconBuilder {
            private string _iconPath = null;
            public string IconPath => _iconPath;
            private int _width = 20;
            public int Width => _width;
            private int _height = 20;
            public int Height => _height;

            internal VesselBookmarkIconBuilder() {
            }

            public VesselBookmarkIconBuilder WithIconPath(string iconPath) {
                _iconPath = iconPath;
                return this;
            }

            public VesselBookmarkIconBuilder WithSize(int width, int height) {
                _width = width;
                _height = height;
                return this;
            }

            public VesselBookmarkIcon Build() {
                return new VesselBookmarkIcon(this);
            }
        }

        private VesselBookmarkIcon(VesselBookmarkIconBuilder builder) {
            EmptyIcon = new Texture2D(1, 1);
            EmptyIcon.SetPixel(0, 0, Color.clear);
            EmptyIcon.Apply();

            if (!string.IsNullOrEmpty(builder.IconPath)) {
                Icon = GameDatabase.Instance.GetTexture(builder.IconPath, false);
                IconDisabled = GameDatabase.Instance.GetTexture(builder.IconPath + "_disabled", false);
            } else {
                Icon = null;
                IconDisabled = null;
            }

            Width = builder.Width;
            Height = builder.Height;
        }

        /// <summary>
        /// Draws the icon in enabled state (normal icon).
        /// </summary>
        public void Draw() {
            Draw(true);
        }

        /// <summary>
        /// Draws the icon in enabled or disabled state.
        /// </summary>
        /// <param name="enabled">True for normal icon, false for disabled icon</param>
        public void Draw(bool enabled) {
            Texture2D iconToUse = enabled ? Icon : (IconDisabled != null ? IconDisabled : Icon);
            if (iconToUse == null) {
                return;
            }
            Rect iconRect = GUILayoutUtility.GetRect(Width, Height, GUILayout.Width(Width), GUILayout.Height(Height));
            GUI.DrawTexture(iconRect, iconToUse);
        }

        /// <summary>
        /// Reserves space without drawing (transparent). Use when the icon should be hidden but layout must be preserved.
        /// </summary>
        public void DrawHidden() {
            Rect iconRect = GUILayoutUtility.GetRect(Width, Height, GUILayout.Width(Width), GUILayout.Height(Height));
            GUI.DrawTexture(iconRect, EmptyIcon);
        }

        /// <summary>
        /// Draws the icon only when visible; otherwise draws hidden. Convenience for conditional display.
        /// </summary>
        /// <param name="isVisible">True to draw the icon, false to draw hidden</param>
        public void Draw(Func<bool> isVisible) {
            if (isVisible()) {
                Draw();
            } else {
                DrawHidden();
            }
        }
    }
}
