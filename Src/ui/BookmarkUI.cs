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

        // Icon cache
        private Dictionary<string, VesselBookmarkButton> _vesselTypeButtons = new Dictionary<string, VesselBookmarkButton>();
        private VesselBookmarkButton _alarmIcon;
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
        private BookmarksListUIController _bookmarksListUIController;
        private EditCommentUIController _editCommentUIController;

        public BookmarkUI() {
            // Initialize vessel type buttons
            _vesselTypeButtons[VesselType.Base.ToString()] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/base", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Debris.ToString()] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/debris", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Lander.ToString()] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/lander", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Plane.ToString()] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/plane", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Probe.ToString()] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/probe", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Relay.ToString()] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/relay", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Rover.ToString()] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/rover", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Ship.ToString()] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/ship", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Station.ToString()] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/station", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Unknown.ToString()] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/empty", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            
            // Initialize alarm icon
            _alarmIcon = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/alarm",
                null,
                BUTTON_WIDTH,
                BUTTON_HEIGHT
            );

            // Initialize remove button
            _removeButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/remove",
                ModLocalization.GetString("tooltipRemove"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize move up button
            _moveUpButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/up",
                ModLocalization.GetString("tooltipMoveUp"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize move down button
            _moveDownButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/down",
                ModLocalization.GetString("tooltipMoveDown"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );

            // Initialize set target as button
            _setTargetAsButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/target",
                ModLocalization.GetString("tooltipSetTargetAs"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize go to button
            _goToButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/switch",
                ModLocalization.GetString("tooltipGoTo"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize edit button
            _editButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/edit",
                ModLocalization.GetString("tooltipEdit"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );

            // Background texture (slightly tinted blue-green)
            _activeVesselBackground = new Texture2D(1, 1);
            _activeVesselBackground.SetPixel(0, 0, new Color(0.15f, 0.25f, 0.3f, 0.6f)); // Bleu-vert foncé avec transparence
            _activeVesselBackground.Apply();
            
            // Border texture (bright blue-green)
            _activeVesselBorder = new Texture2D(1, 1);
            _activeVesselBorder.SetPixel(0, 0, new Color(0.2f, 0.6f, 0.8f, 1f)); // Bleu-vert clair
            _activeVesselBorder.Apply();
        }

        public void Initialize(
            UIStyles uiStyles, 
            BookmarksListUIController bookmarksListUIController, 
            EditCommentUIController editCommentUIController
        ) {
            _uiStyles = uiStyles;
            _bookmarksListUIController = bookmarksListUIController;
            _editCommentUIController = editCommentUIController;
        }

        public void OnGUI(Bookmark bookmark, int currentIndex) {
            BookmarkUIController bookmarkUIController = new BookmarkUIController(
                bookmark,
                currentIndex,
                _bookmarksListUIController,
                _editCommentUIController
            );

            // Create custom box style with appropriate background
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            
            if (bookmarkUIController.IsActiveVessel()) {
                // Active vessel: use colored background
                boxStyle.normal.background = _activeVesselBackground;
                // Add border by setting border values
                boxStyle.border = new RectOffset(2, 2, 2, 2);
            } else if (bookmarkUIController.IsHovered()) {
                // Hovered bookmark: use hover background
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
            
            // Vessel type icon
            VesselBookmarkButton vesselTypeButton = GetVesselTypeButton(bookmark.BookmarkVesselType);
            vesselTypeButton.Draw(
                () => true,
                null
            );
            
            // Alarm icon
            if( bookmark.HasAlarm ) {
                _alarmIcon.Draw(
                    () => bookmark.HasAlarm,
                    null
                );
            }
                
            // Bookmark name: comment always stands out (red); vessel missing = secondary cue (gray or italic)
            bool hasComment = !string.IsNullOrEmpty(bookmark.Comment);
            bool vesselExists = bookmark.Vessel != null;

            GUIStyle titleStyle;
            if( vesselExists ) {
                if( bookmarkUIController.IsTarget() ) {
                    titleStyle = _uiStyles.LabelTitleTargetStyle;
                } else {
                    titleStyle = _uiStyles.LabelTitleStyle;
                }
            } else {
                titleStyle = _uiStyles.LabelTitleNoVesselStyle;
            }
            string title = bookmarkUIController.GetBookmarkTitle();
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
                bookmarkUIController.IsHovered,
                bookmarkUIController.EditComment
            );
            
            GUILayout.Space(3);

            // "Set target as" button
            System.Action setTargetAsAction;
            if( bookmarkUIController.CanSetTargetAs() ) {
                setTargetAsAction = bookmarkUIController.SetTargetAs;
            } else {
                setTargetAsAction = null;
            }
            _setTargetAsButton.Draw(bookmarkUIController.IsHovered, setTargetAsAction);

            // Go to button (disabled if this is the active vessel)
            System.Action goToAction;
            if (bookmarkUIController.CanSwitchToVessel()) {
                goToAction = bookmarkUIController.SwitchToVessel;
            } else {
                goToAction = null;
            }
            _goToButton.Draw(bookmarkUIController.IsHovered, goToAction);
            
            GUILayout.Space(3);

            // Move up button
            System.Action moveUpAction;
            if( bookmarkUIController.CanMoveUp() ) {
                moveUpAction = bookmarkUIController.MoveUp;
            } else {
                moveUpAction = null;
            }
            _moveUpButton.Draw(
                bookmarkUIController.IsHovered,
                moveUpAction
            );
        
            // Move down button
            System.Action moveDownAction;
            if( bookmarkUIController.CanMoveDown() ) {
                moveDownAction = bookmarkUIController.MoveDown;
            } else {
                moveDownAction = null;
            }
            _moveDownButton.Draw(
                bookmarkUIController.IsHovered,
                moveDownAction
            );
            
            GUILayout.Space(3);
            
            // Remove button
            _removeButton.Draw(
                bookmarkUIController.IsHovered,
                bookmarkUIController.Remove
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
            if (bookmarkUIController.IsActiveVessel() && Event.current.type == EventType.Repaint) {
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
                _bookmarksListUIController.HoveredBookmarkID = bookmark.BookmarkID;
                _bookmarksListUIController.HoveredBookmarkType = bookmark.BookmarkType;
            }
        }

        /// <summary>
        /// Gets icon texture for vessel type
        /// </summary>
        private VesselBookmarkButton GetVesselTypeButton(string type) {
            if( string.IsNullOrEmpty(type) ) {
                return _vesselTypeButtons[VesselType.Unknown.ToString()];
            }
            if( !_vesselTypeButtons.ContainsKey(type) ) {
                return _vesselTypeButtons[VesselType.Unknown.ToString()];
            }
            return _vesselTypeButtons[type];
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