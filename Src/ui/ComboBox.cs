using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    /// <summary>
    /// IMGUI combobox: a button that opens a popup with a list of options.
    /// Inspired by MechJeb's GuiUtils.ComboBox (Unity has no native ComboBox).
    /// Call DrawGUI() every frame in OnGUI when the parent window is visible.
    /// </summary>
    public static class ComboBox {

        private static Rect _rect;
        private static object _popupOwner;
        private static string[] _entries;
        private static bool _popupActive;
        private static int _selectedItem;
        private static readonly int _id = GUIUtility.GetControlID(FocusType.Passive);
        private static GUIStyle _popupStyle;
        private static GUIStyle _gridStyle;
        private static GUIStyle _gridStyleSelected;

        static ComboBox() {
            _popupStyle = new GUIStyle(GUI.skin.window);
            _popupStyle.border.top = _popupStyle.border.bottom;
            _popupStyle.padding.top = _popupStyle.padding.bottom;
            _gridStyle = new GUIStyle(GUI.skin.button);
            _gridStyleSelected = new GUIStyle(GUI.skin.button);
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, new Color(0.35f, 0.35f, 0.5f, 0.9f));
            t.Apply();
            _gridStyleSelected.normal.background = t;
        }

        /// <summary>
        /// Must be called every frame in OnGUI when the bookmarks window is visible,
        /// so the popup is drawn and can receive clicks.
        /// </summary>
        public static void DrawGUI() {
            if (_popupOwner == null || _rect.height == 0 || !_popupActive) {
                return;
            }

            _rect.x = Mathf.Clamp(_rect.x, 0, Screen.width - _rect.width);
            _rect.y = Mathf.Clamp(_rect.y, 0, Screen.height - _rect.height);

            _rect = GUILayout.Window(_id, _rect, _DrawPopup, "", _popupStyle);

            if (Event.current.type == EventType.MouseDown && !_rect.Contains(Event.current.mousePosition)) {
                _popupOwner = null;
            }
        }

        private static void _DrawPopup(int windowID) {
            for (int i = 0; i < _entries.Length; i++) {
                GUIStyle style = (i == _selectedItem) ? _gridStyleSelected : _gridStyle;
                if (GUILayout.Button(_entries[i], style)) {
                    _selectedItem = i;
                    _popupActive = false;
                }
            }
        }

        /// <summary>
        /// Draws the combobox button and returns the selected index (possibly updated after user choice).
        /// </summary>
        /// <param name="selectedIndex">Current selected index</param>
        /// <param name="entries">Display strings for each option</param>
        /// <param name="caller">Unique object identifying this combobox (e.g. "body" or "type")</param>
        /// <param name="buttonStyle">Style for the main button</param>
        /// <param name="expandWidth">Whether the button expands horizontally</param>
        /// <param name="popupStyle">Style for the popup window (e.g. UIStyles.ComboPopupStyle). If null, uses default.</param>
        /// <param name="gridStyle">Style for the list items in the popup (e.g. UIStyles.ComboGridStyle). If null, uses default.</param>
        /// <param name="gridStyleSelected">Style for the currently selected item in the list (e.g. UIStyles.ComboGridSelectedStyle). If null, uses default.</param>
        /// <returns>New selected index (same or updated after popup selection)</returns>
        public static int Box(int selectedIndex, string[] entries, object caller, GUIStyle buttonStyle, bool expandWidth = true, GUIStyle popupStyle = null, GUIStyle gridStyle = null, GUIStyle gridStyleSelected = null) {
            if (entries == null || entries.Length == 0) {
                return 0;
            }
            if (entries.Length == 1) {
                GUILayout.Label(entries[0], buttonStyle ?? GUI.skin.label, expandWidth ? GUILayout.ExpandWidth(true) : GUILayout.ExpandWidth(false));
                return 0;
            }

            if (selectedIndex >= entries.Length) {
                selectedIndex = entries.Length - 1;
            }
            if (selectedIndex < 0) {
                selectedIndex = 0;
            }

            if (_popupOwner == caller && !_popupActive) {
                _popupOwner = null;
                selectedIndex = _selectedItem;
                GUI.changed = true;
            }

            bool guiChanged = GUI.changed;
            var layoutOption = expandWidth ? GUILayout.ExpandWidth(true) : GUILayout.ExpandWidth(false);
            if (GUILayout.Button("↓ " + entries[selectedIndex] + " ↓", buttonStyle ?? GUI.skin.button, layoutOption)) {
                GUI.changed = guiChanged;
                _popupOwner = caller;
                _popupActive = true;
                _entries = entries;
                _selectedItem = selectedIndex;
                _rect = new Rect(0, 0, 0, 0);
                _popupStyle = popupStyle ?? _popupStyle;
                _gridStyle = gridStyle ?? _gridStyle;
                _gridStyleSelected = gridStyleSelected ?? _gridStyleSelected;
            }

            if (Event.current.type == EventType.Repaint && _popupOwner == caller && _rect.height == 0) {
                _rect = GUILayoutUtility.GetLastRect();
                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;
                Vector2 clippedMousePos = Event.current.mousePosition;
                _rect.x = _rect.x + mousePos.x - clippedMousePos.x;
                _rect.y = _rect.y + mousePos.y - clippedMousePos.y;
            }

            return selectedIndex;
        }
    }
}
