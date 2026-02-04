using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    public class UIStyles {
        // UI styles with white text
        public GUIStyle LabelStyle { get; private set; }
        public GUIStyle LabelTitleStyle { get; private set; }
        public GUIStyle LabelTitleNoVesselStyle { get; private set; }
        public GUIStyle LabelCommentStyle { get; private set; }
        public GUIStyle LabelCommentNoVesselStyle { get; private set; }
        public GUIStyle ToggleStyle { get; private set; }
        public GUIStyle ButtonStyle { get; private set; }
        public GUIStyle TextAreaStyle { get; private set; }
        public GUIStyle TooltipStyle { get; private set; }
        public GUIStyle ComboPopupStyle { get; private set; }
        public GUIStyle ComboGridStyle { get; private set; }
        /// <summary>Style for the currently selected item in the combobox list.</summary>
        public GUIStyle ComboGridSelectedStyle { get; private set; }

        public UIStyles() {
            LabelStyle = new GUIStyle(GUI.skin.label) { richText = true };
            ApplyColor(LabelStyle, Color.white);

            // Style for the title of the bookmark
            LabelTitleStyle = new GUIStyle(LabelStyle) { fontStyle = FontStyle.Bold };
            ApplyColor(LabelTitleStyle, Color.white);

            // Style for the title of the bookmark when there is a comment
            LabelCommentStyle = new GUIStyle(LabelStyle) { fontStyle = FontStyle.Bold };
            ApplyColor(LabelCommentStyle, Color.red);

            // Style for the title of the bookmark when the vessel is missing
            LabelTitleNoVesselStyle = new GUIStyle(LabelStyle) { fontStyle = FontStyle.Italic | FontStyle.Bold };
            ApplyColor(LabelTitleNoVesselStyle, Color.gray);

            // Style for the title of the bookmark when there is a comment and the vessel is missing
            LabelCommentNoVesselStyle = new GUIStyle(LabelStyle) { fontStyle = FontStyle.Italic | FontStyle.Bold };
            ApplyColor(LabelCommentNoVesselStyle, Color.red);

            ToggleStyle = new GUIStyle(GUI.skin.toggle);
            ApplyColor(ToggleStyle, Color.white);

            ButtonStyle = new GUIStyle(GUI.skin.button);
            ApplyColor(ButtonStyle, Color.white);

            TextAreaStyle = new GUIStyle(GUI.skin.textArea);
            ApplyColor(TextAreaStyle, Color.white);

            TooltipStyle = new GUIStyle(GUI.skin.box);
            ApplyColor(TooltipStyle, Color.white);

            ComboPopupStyle = new GUIStyle(GUI.skin.window);
            ComboPopupStyle.border.top = ComboPopupStyle.border.bottom;
            ComboPopupStyle.padding.top = ComboPopupStyle.padding.bottom;
            ApplyColor(ComboPopupStyle, Color.white);

            // Style grille : aspect libellé (sans bordure), cliquable avec hover
            ComboGridStyle = new GUIStyle(GUI.skin.button);
            ComboGridStyle.border = new RectOffset(0, 0, 0, 0);
            ComboGridStyle.normal.background = null;
            var hoverBg = new Texture2D(1, 1);
            hoverBg.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.4f, 0.6f));
            hoverBg.Apply();
            ComboGridStyle.hover.background = hoverBg;
            ComboGridStyle.active.background = hoverBg;
            ApplyColor(ComboGridStyle, Color.white);

            // Style grille : aspect libellé (sans bordure), cliquable avec hover
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