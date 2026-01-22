using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Represents a clickable button with icon, hover icon, and tooltip
    /// </summary>
    public class BookmarkButton {
        
        public Texture2D Icon { get; private set; }
        public Texture2D IconHover { get; private set; }
        public string Tooltip { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        /// <summary>
        /// Creates a new bookmark button
        /// </summary>
        /// <param name="icon">Normal icon texture</param>
        /// <param name="iconHover">Hover icon texture (optional)</param>
        /// <param name="tooltip">Tooltip text (optional)</param>
        /// <param name="width">Button width in pixels</param>
        /// <param name="height">Button height in pixels</param>
        public BookmarkButton(
            Texture2D icon, 
            Texture2D iconHover = null, 
            string tooltip = "", 
            int width = 20,
            int height = 20
        ) {
            Icon = icon;
            IconHover = iconHover;
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
            
            // Determine which icon to use based on hover state
            Texture2D iconToUse = Icon;
            bool isHovering = iconRect.Contains(Event.current.mousePosition);
            if (isHovering && IconHover != null) {
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
            if (Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition)) {
                if (OnClick != null) {
                    OnClick();
                }
                Event.current.Use();
                wasClicked = true;
            }
            
            return wasClicked;
        }
    }
}
