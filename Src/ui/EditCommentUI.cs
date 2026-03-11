using ClickThroughFix;
using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    public class EditCommentUI {
        
        private Rect _editWindowRect = new Rect(200, 200, 400, 200);
        private int _editWindowID;
        private UIStyles _uiStyles;

        public EditCommentUIController Controller { get; private set; } = new EditCommentUIController();
        private VesselBookmarkButton _saveButton;
        private VesselBookmarkButton _cancelButton;

        public EditCommentUI(UIStyles uiStyles) {
            _editWindowID = UnityEngine.Random.Range(1000, 2000);
            _saveButton = VesselBookmarkButton.Builder()
                .WithIconPath("VesselBookmarkMod/buttons/save")
                .WithLabel(ModLocalization.GetString("buttonSave"))
                .WithTooltip(ModLocalization.GetString("buttonSave"))
                .WithIconSize(20, 20)
                .Build();
            _cancelButton = VesselBookmarkButton.Builder()
                .WithIconPath("VesselBookmarkMod/buttons/cancel")
                .WithLabel(ModLocalization.GetString("buttonCancel"))
                .WithTooltip(ModLocalization.GetString("buttonCancel"))
                .WithIconSize(20, 20)
                .Build();
            
            _uiStyles = uiStyles;
        }

        public void OnGUI() {
            if (!Controller.IsEditingComment()) {
                return;
            }
            _editWindowRect = ClickThruBlocker.GUILayoutWindow(
                _editWindowID,
                _editWindowRect,
                DrawEditWindow,
                ModLocalization.GetString("editWindowTitle"),
                GUILayout.MinWidth(400),
                GUILayout.MinHeight(200)
            );
            
            // Prevent window from going off screen
            _editWindowRect.x = Mathf.Clamp(_editWindowRect.x, 0, Screen.width - _editWindowRect.width);
            _editWindowRect.y = Mathf.Clamp(_editWindowRect.y, 0, Screen.height - _editWindowRect.height);
        }

        /// <summary>
        /// Draws the edit window
        /// </summary>
        private void DrawEditWindow(int windowID) {
            GUILayout.BeginVertical();
            
            GUILayout.Label(ModLocalization.GetString("labelComment"), _uiStyles.LabelStyle);
            Controller.EditedComment = GUILayout.TextArea(
                Controller.EditedComment, 
                _uiStyles.TextAreaStyle,
                GUILayout.Height(100), 
                GUILayout.ExpandWidth(true)
            );
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            _saveButton.Draw(() => true, Controller.SaveComment);
            _cancelButton.Draw(() => true, Controller.CancelCommentEdition);
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            // Allow window dragging
            GUI.DragWindow();
        }
        
    }
}