using System;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui {
    
    /// <summary>
    /// Represents a clickable button with icon, hover icon, and tooltip
    /// </summary>
    public class VesselBookmarkButton {
        
        public Texture2D Icon { get; private set; }
        public Texture2D IconHover { get; private set; }
        public Texture2D IconClicked { get; private set; }
        public Texture2D IconDisabled { get; private set; }
        public Texture2D EmptyIcon { get; private set; }
        public string Tooltip { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        /// <summary>
        /// Starts building a button. Use Build() when done configuring.
        /// </summary>
        public static VesselBookmarkButtonBuilder Builder() {
            return new VesselBookmarkButtonBuilder();
        }

        public sealed class VesselBookmarkButtonBuilder {
            private string _iconPath = null;
            public string IconPath => _iconPath;
            private string _tooltip = "";
            public string Tooltip => _tooltip;
            private int _width = 20;
            public int Width => _width;
            private int _height = 20;
            public int Height => _height;

            internal VesselBookmarkButtonBuilder() {
            }

            public VesselBookmarkButtonBuilder WithIconPath(string iconPath) {
                _iconPath = iconPath;
                return this;
            }

            public VesselBookmarkButtonBuilder WithTooltip(string tooltip) {
                _tooltip = tooltip ?? "";
                return this;
            }

            public VesselBookmarkButtonBuilder WithSize(int width, int height) {
                _width = width;
                _height = height;
                return this;
            }

            public VesselBookmarkButton Build() {
                return new VesselBookmarkButton(this);
            }
        }

        // =============================================================================

        /// <summary>
        /// Creates a new button
        /// </summary>
        /// <param name="builder">The builder to use to configure the button</param>
        private VesselBookmarkButton(VesselBookmarkButtonBuilder builder) {
            // Create an empty and transparent texture
            EmptyIcon = new Texture2D(1, 1);
            EmptyIcon.SetPixel(0, 0, Color.clear);
            EmptyIcon.Apply();

            if (!string.IsNullOrEmpty(builder.IconPath)) {
                Icon = GameDatabase.Instance.GetTexture(builder.IconPath, false);
                IconHover = GameDatabase.Instance.GetTexture(builder.IconPath + "_hover", false);
                IconClicked = GameDatabase.Instance.GetTexture(builder.IconPath + "_clicked", false);
                IconDisabled = GameDatabase.Instance.GetTexture(builder.IconPath + "_disabled", false);
            } else {
                Icon = null;
                IconHover = null;
                IconClicked = null;
                IconDisabled = null;
            }

            Tooltip = string.IsNullOrEmpty(builder.Tooltip) ? null : builder.Tooltip;
            Width = builder.Width;
            Height = builder.Height;
        }
        
        /// <summary>
        /// Draws the button
        /// </summary>
        /// <param name="isVisible">A function that returns true if the button should be visible, false otherwise</param>
        /// <param name="onClick">The action to perform when the button is clicked</param>
        /// <returns>True if the button was clicked, false otherwise</returns>
        public bool Draw(
            Func<bool> isVisible,
            Action onClick
        ) {
            if( !isVisible() ) {
                this.DrawHidden();
                return false;
            }

            if( onClick == null ) {
                this.DrawDisabled();
                return false;
            } else {
                return this.DrawEnabled(onClick);
            }
        }

        /// <summary>
        /// Draws the button in enabled state (can be clicked)
        /// </summary>
        /// <param name="onClick">The action to perform when the button is clicked</param>
        /// <returns>True if the button was clicked, false otherwise</returns>
        private bool DrawEnabled(Action onClick) {
            // Reserve space for button and get its rect
            Rect iconRect = GUILayoutUtility.GetRect(Width, Height, GUILayout.Width(Width), GUILayout.Height(Height));
            
            bool isHovering = iconRect.Contains(Event.current.mousePosition);
            bool isClicking = Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition);
            
            // Determine which icon to use based on state (clicked > hover > normal)
            Texture2D iconToUse = null;
            if (isClicking && IconClicked != null) {
                iconToUse = IconClicked;
            } else if (isHovering && IconHover != null) {
                iconToUse = IconHover;
            } else if( Icon != null ) {
                iconToUse = Icon;
            } else {
                iconToUse = EmptyIcon;
            }

            // Set tooltip if provided
            if (!string.IsNullOrEmpty(Tooltip)) {
                GUIContent iconContent = new GUIContent("", Tooltip);
                GUI.Label(iconRect, iconContent);
            }
            
            // Draw icon
            GUI.DrawTexture(iconRect, iconToUse);
            
            // Handle click
            bool wasClicked = false;
            if (isClicking) {
                if (onClick != null) {
                    onClick();
                }
                Event.current.Use();
                wasClicked = true;
            }
            
            return wasClicked;
        }
        
        /// <summary>
        /// Draws the button in disabled state (no interactions, no tooltip, no hover)
        /// </summary>
        public void DrawDisabled() {
            // Use disabled icon if available, otherwise fall back to normal icon
            Texture2D iconToUse = IconDisabled != null ? IconDisabled : Icon;
            
            if (iconToUse == null) {
                return;
            }
            
            // Reserve space for button and get its rect
            Rect iconRect = GUILayoutUtility.GetRect(Width, Height, GUILayout.Width(Width), GUILayout.Height(Height));
            
            // Draw icon only (no tooltip, no hover, no click handling)
            GUI.DrawTexture(iconRect, iconToUse);
        }

        /// <summary>
        /// Draws the button in hidden state (no interactions, no tooltip, no hover)
        /// </summary>
        public void DrawHidden() {
            Rect iconRect = GUILayoutUtility.GetRect(Width, Height, GUILayout.Width(Width), GUILayout.Height(Height));
            GUI.DrawTexture(iconRect, EmptyIcon);
        }
    }
}
