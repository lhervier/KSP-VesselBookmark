using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Represents a clickable button with icon, hover icon, and tooltip
    /// </summary>
    public class BookmarkButton {
        
        public Texture2D Icon { get; private set; }
        public Texture2D IconHover { get; private set; }
        public Texture2D IconClicked { get; private set; }
        public Texture2D IconDisabled { get; private set; }
        public string Tooltip { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        /// <summary>
        /// Creates a new bookmark button
        /// </summary>
        /// <param name="iconPath">Base path to the icon (e.g., "VesselBookmarkMod/buttons/remove"). 
        /// The class will automatically load variants: _hover, _clicked, _disabled if they exist.</param>
        /// <param name="tooltip">Tooltip text (optional)</param>
        /// <param name="width">Button width in pixels</param>
        /// <param name="height">Button height in pixels</param>
        public BookmarkButton(
            string iconPath,
            string tooltip = "", 
            int width = 20,
            int height = 20
        ) {
            // Load base icon
            Icon = GameDatabase.Instance.GetTexture(iconPath, false);
            
            // Load hover variant if it exists
            IconHover = GameDatabase.Instance.GetTexture(iconPath + "_hover", false);
            
            // Load clicked variant if it exists
            IconClicked = GameDatabase.Instance.GetTexture(iconPath + "_clicked", false);
            
            // Load disabled variant if it exists
            IconDisabled = GameDatabase.Instance.GetTexture(iconPath + "_disabled", false);
            
            Tooltip = tooltip;
            Width = width;
            Height = height;
        }
        
        /// <summary>
        /// Draws the button and handles interactions
        /// </summary>
        /// <param name="onClick">Callback to execute on click</param>
        /// <returns>True if the button was clicked</returns>
        public bool Draw(System.Action OnClick) {
            if (Icon == null) {
                return false;
            }
            
            // Reserve space for button and get its rect
            Rect iconRect = GUILayoutUtility.GetRect(Width, Height, GUILayout.Width(Width), GUILayout.Height(Height));
            
            bool isHovering = iconRect.Contains(Event.current.mousePosition);
            bool isClicking = Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition);
            
            // Determine which icon to use based on state (clicked > hover > normal)
            Texture2D iconToUse = Icon;
            if (isClicking && IconClicked != null) {
                iconToUse = IconClicked;
            } else if (isHovering && IconHover != null) {
                iconToUse = IconHover;
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
                if (OnClick != null) {
                    OnClick();
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
    }
}
