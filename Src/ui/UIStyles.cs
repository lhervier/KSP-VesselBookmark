using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    public class UIStyles {
        /// <summary>
        /// Default style for the label.
        /// </summary>
        public GUIStyle LabelStyle { get; private set; }

        /// <summary>
        /// Style for the title of the bookmark.
        /// </summary>
        public GUIStyle LabelTitleStyle { get; private set; }

        /// <summary>
        /// Style for the title of the bookmark when the vessel is missing.
        /// </summary>
        public GUIStyle LabelTitleNoVesselStyle { get; private set; }

        /// <summary>
        /// Style for the comment of the bookmark.
        /// </summary>
        public GUIStyle LabelCommentStyle { get; private set; }
        
        /// <summary>
        /// Style for the toggle.
        /// </summary>
        public GUIStyle ToggleStyle { get; private set; }
        
        /// <summary>
        /// Style for the button.
        /// </summary>
        public GUIStyle ButtonStyle { get; private set; }
        
        /// <summary>
        /// Style for the text area.
        /// </summary>
        public GUIStyle TextAreaStyle { get; private set; }
        
        /// <summary>
        /// Style for the tooltip.
        /// </summary>
        public GUIStyle TooltipStyle { get; private set; }

        /// <summary>
        /// Style for the combo popup.
        /// </summary>
        public GUIStyle ComboPopupStyle { get; private set; }

        /// <summary>
        /// Style for the combo grid.
        /// </summary>
        public GUIStyle ComboGridStyle { get; private set; }

        /// <summary>
        /// Style for the selected item in the combo grid.
        /// </summary>
        public GUIStyle ComboGridSelectedStyle { get; private set; }

        /// <summary>
        /// Creates a new UIStyles instance.
        /// </summary>
        public UIStyles() {
            // Default label style
            LabelStyle = new GUIStyle(GUI.skin.label) { richText = true };
            ApplyColor(LabelStyle, Color.white);

            // Style for the title of the bookmark
            LabelTitleStyle = new GUIStyle(LabelStyle) { fontStyle = FontStyle.Bold };
            ApplyColor(LabelTitleStyle, Color.white);

            // Style for the title of the bookmark when the vessel is missing
            LabelTitleNoVesselStyle = new GUIStyle(LabelStyle) { fontStyle = FontStyle.Italic | FontStyle.Bold };
            ApplyColor(LabelTitleNoVesselStyle, Color.gray);

            // Style for the comment of the bookmark
            LabelCommentStyle = new GUIStyle(LabelStyle) { fontStyle = FontStyle.Bold };
            ApplyColor(LabelCommentStyle, Color.red);

            // Style for the toggle
            ToggleStyle = new GUIStyle(GUI.skin.toggle);
            ApplyColor(ToggleStyle, Color.white);

            // Default button style
            ButtonStyle = new GUIStyle(GUI.skin.button);
            ApplyColor(ButtonStyle, Color.white);

            // Style for the text area
            TextAreaStyle = new GUIStyle(GUI.skin.textArea);
            ApplyColor(TextAreaStyle, Color.white);

            // Style for the tooltip
            TooltipStyle = new GUIStyle(GUI.skin.box);
            ApplyColor(TooltipStyle, Color.white);

            // Style for the combo popup
            ComboPopupStyle = new GUIStyle(GUI.skin.window);
            ComboPopupStyle.border.top = ComboPopupStyle.border.bottom;
            ComboPopupStyle.padding.top = ComboPopupStyle.padding.bottom;
            ApplyColor(ComboPopupStyle, Color.white);

            // Style for the combo grid
            ComboGridStyle = new GUIStyle(GUI.skin.button);
            ComboGridStyle.border = new RectOffset(0, 0, 0, 0);
            ComboGridStyle.normal.background = null;
            var hoverBg = new Texture2D(1, 1);
            hoverBg.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.4f, 0.6f));
            hoverBg.Apply();
            ComboGridStyle.hover.background = hoverBg;
            ComboGridStyle.active.background = hoverBg;
            ApplyColor(ComboGridStyle, Color.white);

            // Style for the selected item in the combo grid
            ComboGridSelectedStyle = new GUIStyle(GUI.skin.button);
            var selectedBg = new Texture2D(1, 1);
            selectedBg.SetPixel(0, 0, new Color(0.35f, 0.35f, 0.5f, 0.9f));
            selectedBg.Apply();
            ComboGridSelectedStyle.border = new RectOffset(0, 0, 0, 0);
            ComboGridSelectedStyle.normal.background = selectedBg;
            ComboGridSelectedStyle.hover.background = selectedBg;
            ComboGridSelectedStyle.active.background = selectedBg;
            ApplyColor(ComboGridSelectedStyle, Color.white);
        }

        /// <summary>
        /// Applies the color to the style.
        /// </summary>
        /// <param name="style">The style to apply the color to.</param>
        /// <param name="color">The color to apply to the style.</param>
        private void ApplyColor(GUIStyle style, Color color) {
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            style.onNormal.textColor = color;
            style.onHover.textColor = color;
            style.onActive.textColor = color;
            style.onFocused.textColor = color;
        }
    }
}