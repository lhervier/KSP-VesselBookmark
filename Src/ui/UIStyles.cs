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
        public GUIStyle ButtonStyle { get; private set; }
        public GUIStyle TextAreaStyle { get; private set; }
        public GUIStyle TooltipStyle { get; private set; }
        
        public UIStyles() {
            LabelStyle = new GUIStyle(GUI.skin.label) { richText = true };
            ButtonStyle = new GUIStyle(GUI.skin.button);
            TextAreaStyle = new GUIStyle(GUI.skin.textArea);
            TooltipStyle = new GUIStyle(GUI.skin.box);
        
            ApplyWhiteText(LabelStyle);
            ApplyWhiteText(ButtonStyle);
            ApplyWhiteText(TextAreaStyle);
            ApplyWhiteText(TooltipStyle);
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