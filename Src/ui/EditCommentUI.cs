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

        public EditCommentUIController Controller { get; private set; }

        public EditCommentUI() {
            _editWindowID = UnityEngine.Random.Range(1000, 2000);
            Controller = new EditCommentUIController();
        }

        public void Initialize(UIStyles uiStyles) {
            _uiStyles = uiStyles;
        }

        public void OnGUI() {
            if (!Controller.IsEditingComment()) {
                return;
            }
            _editWindowRect = GUILayout.Window(
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
            if (GUILayout.Button(ModLocalization.GetString("buttonSave"), _uiStyles.ButtonStyle)) {
                Controller.SaveComment();
            }
            if (GUILayout.Button(ModLocalization.GetString("buttonCancel"), _uiStyles.ButtonStyle)) {
                Controller.CancelCommentEdition();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            // Allow window dragging
            GUI.DragWindow();
        }
        
    }
}