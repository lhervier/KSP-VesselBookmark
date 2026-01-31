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
            ToggleStyle = new GUIStyle(GUI.skin.toggle);
            ButtonStyle = new GUIStyle(GUI.skin.button);
            TextAreaStyle = new GUIStyle(GUI.skin.textArea);
            TooltipStyle = new GUIStyle(GUI.skin.box);
            ComboPopupStyle = new GUIStyle(GUI.skin.window);
            ComboGridStyle = new GUIStyle(GUI.skin.button);
            ComboGridSelectedStyle = new GUIStyle(GUI.skin.button);

            ApplyWhiteText(LabelStyle);
            ApplyWhiteText(ToggleStyle);
            ApplyWhiteText(ButtonStyle);
            ApplyWhiteText(TextAreaStyle);
            ApplyWhiteText(TooltipStyle);
            ApplyWhiteText(ComboPopupStyle);
            ApplyWhiteText(ComboGridStyle);
            ApplyWhiteText(ComboGridSelectedStyle);

            ComboPopupStyle.border.top = ComboPopupStyle.border.bottom;
            ComboPopupStyle.padding.top = ComboPopupStyle.padding.bottom;

            // Style grille : aspect libellé (sans bordure), cliquable avec hover
            ComboGridStyle.border = new RectOffset(0, 0, 0, 0);
            ComboGridStyle.normal.background = null;
            var hoverBg = new Texture2D(1, 1);
            hoverBg.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.4f, 0.6f));
            hoverBg.Apply();
            ComboGridStyle.hover.background = hoverBg;
            ComboGridStyle.active.background = hoverBg;

            var selectedBg = new Texture2D(1, 1);
            selectedBg.SetPixel(0, 0, new Color(0.35f, 0.35f, 0.5f, 0.9f));
            selectedBg.Apply();
            ComboGridSelectedStyle.border = new RectOffset(0, 0, 0, 0);
            ComboGridSelectedStyle.normal.background = selectedBg;
            ComboGridSelectedStyle.hover.background = selectedBg;
            ComboGridSelectedStyle.active.background = selectedBg;
        }

        private void ApplyWhiteText(GUIStyle style) {
            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            style.focused.textColor = Color.white;
            style.onNormal.textColor = Color.white;
            style.onHover.textColor = Color.white;
            style.onActive.textColor = Color.white;
            style.onFocused.textColor = Color.white;
        }
    }
}