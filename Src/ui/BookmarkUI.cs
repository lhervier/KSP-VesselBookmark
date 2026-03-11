using System;
using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    public class BookmarkUI {
        private static readonly int BUTTON_HEIGHT = 20;
        private static readonly int BUTTON_WIDTH = 20;

        // Icon cache (non-clickable indicators)
        private Dictionary<string, VesselBookmarkIcon> _vesselTypeIcons = new Dictionary<string, VesselBookmarkIcon>();
        private VesselBookmarkIcon _alarmIcon;

        // Action buttons (clickable)
        private VesselBookmarkButton _removeButton;
        private VesselBookmarkButton _moveUpButton;
        private VesselBookmarkButton _moveDownButton;
        private VesselBookmarkButton _setTargetAsButton;
        private VesselBookmarkButton _goToButton;
        private VesselBookmarkButton _editButton;
        
        // Textures for active vessel highlighting
        private Texture2D _activeVesselBackground;
        private Texture2D _activeVesselBorder;

        private UIStyles _uiStyles;
        public BookmarkUIController Controller = new BookmarkUIController();

        public BookmarkUI(UIStyles uiStyles) {
            // Initialize vessel type icons (non-clickable)
            foreach( VesselType vesselType in Enum.GetValues(typeof(VesselType)) ) {
                _vesselTypeIcons[vesselType.ToString()] = VesselBookmarkIcon.Builder()
                    .WithIconPath("VesselBookmarkMod/vessel_types/" + vesselType.ToString().ToLower())
                    .WithSize(BUTTON_WIDTH, BUTTON_HEIGHT)
                    .Build();
            }
            
            // Initialize icons (clickable and non-clickable indicator)
            _alarmIcon = VesselBookmarkIcon.Builder()
                .WithIconPath("VesselBookmarkMod/buttons/alarm")
                .WithSize(BUTTON_WIDTH, BUTTON_HEIGHT)
                .Build();
            _removeButton = VesselBookmarkButton.Builder()
                .WithIconPath("VesselBookmarkMod/buttons/remove")
                .WithTooltip(ModLocalization.GetString("tooltipRemove"))
                .WithIconSize(BUTTON_WIDTH, BUTTON_HEIGHT)
                .Build();
            _moveUpButton = VesselBookmarkButton.Builder()
                .WithIconPath("VesselBookmarkMod/buttons/up")
                .WithTooltip(ModLocalization.GetString("tooltipMoveUp"))
                .WithIconSize(BUTTON_WIDTH, BUTTON_HEIGHT)
                .Build();
            _moveDownButton = VesselBookmarkButton.Builder()
                .WithIconPath("VesselBookmarkMod/buttons/down")
                .WithTooltip(ModLocalization.GetString("tooltipMoveDown"))
                .WithIconSize(BUTTON_WIDTH, BUTTON_HEIGHT)
                .Build();
            _setTargetAsButton = VesselBookmarkButton.Builder()
                .WithIconPath("VesselBookmarkMod/buttons/target")
                .WithTooltip(ModLocalization.GetString("tooltipSetTargetAs"))
                .WithIconSize(BUTTON_WIDTH, BUTTON_HEIGHT)
                .Build();
            _goToButton = VesselBookmarkButton.Builder()
                .WithIconPath("VesselBookmarkMod/buttons/switch")
                .WithTooltip(ModLocalization.GetString("tooltipGoTo"))
                .WithIconSize(BUTTON_WIDTH, BUTTON_HEIGHT)
                .Build();
            _editButton = VesselBookmarkButton.Builder()
                .WithIconPath("VesselBookmarkMod/buttons/edit")
                .WithTooltip(ModLocalization.GetString("tooltipEdit"))
                .WithIconSize(BUTTON_WIDTH, BUTTON_HEIGHT)
                .Build();

            // Background texture (slightly tinted blue-green)
            _activeVesselBackground = new Texture2D(1, 1);
            _activeVesselBackground.SetPixel(0, 0, new Color(0.15f, 0.25f, 0.3f, 0.6f)); // Bleu-vert foncé avec transparence
            _activeVesselBackground.Apply();
            
            // Border texture (bright blue-green)
            _activeVesselBorder = new Texture2D(1, 1);
            _activeVesselBorder.SetPixel(0, 0, new Color(0.2f, 0.6f, 0.8f, 1f)); // Bleu-vert clair
            _activeVesselBorder.Apply();

            _uiStyles = uiStyles;
        }

        public void OnGUI(
            Bookmark bookmark, 
            bool isHovered,
            bool isSelected,
            bool isFirst,
            bool isLast
        ) {
            Controller.Initialize(
                bookmark, 
                isHovered, 
                isSelected,
                isFirst,
                isLast
            );

            // Create custom box style with appropriate background
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            
            if (Controller.IsActiveVessel()) {
                // Active vessel: use colored background
                boxStyle.normal.background = _activeVesselBackground;
                // Add border by setting border values
                boxStyle.border = new RectOffset(2, 2, 2, 2);
            } else if (Controller.IsHoveredOrSelected()) {
                // Hovered or selected bookmark: use hover background
                Texture2D hoverBg = new Texture2D(1, 1);
                hoverBg.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
                hoverBg.Apply();
                boxStyle.normal.background = hoverBg;
            }
            
            GUILayout.BeginVertical(boxStyle);
            
            // Line 1: Icon, Module name, and action buttons (aligned right)
            GUILayout.BeginHorizontal();

            // Indentation
            GUILayout.Space(20);
            
            // Vessel type icon (indicator only)
            VesselBookmarkIcon vesselTypeIcon = GetVesselTypeIcon(bookmark.BookmarkVesselType);
            vesselTypeIcon.Draw();
            
            // Alarm icon
            if( bookmark.HasAlarm ) {
                _alarmIcon.Draw();
            }
            
            // Bookmark name: comment always stands out (red); vessel missing = secondary cue (gray or italic)
            bool hasComment = !string.IsNullOrEmpty(bookmark.Comment);
            bool vesselExists = bookmark.Vessel != null;

            GUIStyle titleStyle;
            if( vesselExists ) {
                if( Controller.IsTarget() ) {
                    titleStyle = _uiStyles.LabelTitleTargetStyle;
                } else {
                    titleStyle = _uiStyles.LabelTitleStyle;
                }
            } else {
                titleStyle = _uiStyles.LabelTitleNoVesselStyle;
            }
            string title = bookmark.BookmarkTitle;
            if( !vesselExists ) {
                if( bookmark.BookmarkType == BookmarkType.Vessel ) {
                    title += " (" + ModLocalization.GetString("labelVesselNotFound") + ")";
                } else {
                    title += " (" + ModLocalization.GetString("labelCommandModuleNotFound") + ")";
                }
            }
            GUILayout.Label(
                title, 
                titleStyle, 
                GUILayout.Width(250)
            );
            
            GUILayout.FlexibleSpace();

            // Small spacing before buttons
            GUILayout.Space(5);
            
            // Edit button
            _editButton.Draw(
                Controller.IsHoveredOrSelected,
                () => Controller.OnBookmarkEdit.Fire(bookmark)
            );
            
            GUILayout.Space(3);

            // "Set target as" button
            System.Action setTargetAsAction;
            if( Controller.CanSetTargetAs() ) {
                setTargetAsAction = () => Controller.OnBookmarkSetTargetAs.Fire(bookmark);
            } else {
                setTargetAsAction = null;
            }
            _setTargetAsButton.Draw(Controller.IsHoveredOrSelected, setTargetAsAction);

            // Go to button (disabled if this is the active vessel)
            System.Action goToAction;
            if (Controller.CanSwitchToVessel()) {
                goToAction = () => Controller.OnBookmarkSwitchToVessel.Fire(bookmark);
            } else {
                goToAction = null;
            }
            _goToButton.Draw(Controller.IsHoveredOrSelected, goToAction);
            
            GUILayout.Space(3);

            // Move up button
            System.Action moveUpAction;
            if( Controller.CanMoveUp() ) {
                moveUpAction = () => Controller.OnBookmarkMovedUp.Fire(bookmark);
            } else {
                moveUpAction = null;
            }
            _moveUpButton.Draw(
                Controller.IsHoveredOrSelected,
                moveUpAction
            );
        
            // Move down button
            System.Action moveDownAction;
            if( Controller.CanMoveDown() ) {
                moveDownAction = () => Controller.OnBookmarkMovedDown.Fire(bookmark);
            } else {
                moveDownAction = null;
            }
            _moveDownButton.Draw(
                Controller.IsHoveredOrSelected,
                moveDownAction
            );
            
            GUILayout.Space(3);
            
            // Remove button
            _removeButton.Draw(
                Controller.IsHoveredOrSelected,
                () => Controller.OnBookmarkRemoved.Fire(bookmark)
            );
            
            GUILayout.EndHorizontal();
            Rect line1Rect = GUILayoutUtility.GetLastRect();

            // Line 2: Vessel situation
            GUILayout.BeginHorizontal();
            GUILayout.Space(20 + BUTTON_WIDTH + 4);
            
            // Vessel situation
            string situation = bookmark.VesselSituationLabel;
            bool addVesselName = false;
            if( bookmark is VesselBookmark vesselBookmark ) {
                addVesselName = false;
            } else if( bookmark is CommandModuleBookmark commandModuleBookmark ) {
                addVesselName = commandModuleBookmark.VesselName != commandModuleBookmark.CommandModuleName;
            }
            if( addVesselName ) {
                situation += " (" + bookmark.VesselName + ")";
            }
            GUILayout.Label(
                situation, 
                _uiStyles.LabelStyle
            );

            GUILayout.EndHorizontal();
            Rect line2Rect = GUILayoutUtility.GetLastRect();

            // Line 3: Comment
            if( !string.IsNullOrEmpty(bookmark.Comment) ) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(BUTTON_WIDTH + 4);
                GUILayout.Label(
                    bookmark.Comment, 
                    _uiStyles.LabelCommentStyle, 
                    GUILayout.ExpandWidth(true)
                );
                GUILayout.EndHorizontal();
            }
            Rect line3Rect = GUILayoutUtility.GetLastRect();
            
            // Get the rect of the bookmark area BEFORE closing it
            Rect bookmarkRect = Rect.MinMaxRect(
                Mathf.Min(Mathf.Min(line1Rect.xMin, line2Rect.xMin), line3Rect.xMin),
                Mathf.Min(Mathf.Min(line1Rect.yMin, line2Rect.yMin), line3Rect.yMin),
                Mathf.Max(Mathf.Max(line1Rect.xMax, line2Rect.xMax), line3Rect.xMax),
                Mathf.Max(Mathf.Max(line1Rect.yMax, line2Rect.yMax), line3Rect.yMax)
            );
            
            GUILayout.EndVertical();
            
            // Draw border for active vessel bookmark
            if (Controller.IsActiveVessel() && Event.current.type == EventType.Repaint) {
                // Draw border using lines
                int borderWidth = 2;
                
                // Top border
                GUI.DrawTexture(new Rect(bookmarkRect.x, bookmarkRect.y, bookmarkRect.width, borderWidth), _activeVesselBorder);
                // Bottom border
                GUI.DrawTexture(new Rect(bookmarkRect.x, bookmarkRect.yMax - borderWidth, bookmarkRect.width, borderWidth), _activeVesselBorder);
                // Left border
                GUI.DrawTexture(new Rect(bookmarkRect.x, bookmarkRect.y, borderWidth, bookmarkRect.height), _activeVesselBorder);
                // Right border
                GUI.DrawTexture(new Rect(bookmarkRect.xMax - borderWidth, bookmarkRect.y, borderWidth, bookmarkRect.height), _activeVesselBorder);
            }

            // Detect hover and update the hovered bookmark ID
            if (bookmarkRect.Contains(Event.current.mousePosition)) {
                Controller.OnBookmarkHovered.Fire(bookmark);
            }

            // On left click on the row (when not consumed by a button), set this line as the current bookmark
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && bookmarkRect.Contains(Event.current.mousePosition)) {
                Controller.OnBookmarkSelected.Fire(bookmark);
                Event.current.Use();
            }
        }

        /// <summary>
        /// Gets icon for vessel type
        /// </summary>
        private VesselBookmarkIcon GetVesselTypeIcon(string type) {
            if( string.IsNullOrEmpty(type) ) {
                return _vesselTypeIcons[VesselType.Unknown.ToString()];
            }
            if( !_vesselTypeIcons.ContainsKey(type) ) {
                return _vesselTypeIcons[VesselType.Unknown.ToString()];
            }
            return _vesselTypeIcons[type];
        }

        public void OnDestroy() {
            // Clean up textures
            if (_activeVesselBackground != null) {
                UnityEngine.Object.Destroy(_activeVesselBackground);
            }
            if (_activeVesselBorder != null) {
                UnityEngine.Object.Destroy(_activeVesselBorder);
            }
        }
    }
}