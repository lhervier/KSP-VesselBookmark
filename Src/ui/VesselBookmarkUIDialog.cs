using System;
using KSP.UI.Screens;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui {
    
    /// <summary>
    /// Helper for simple confirmation dialogs.
    /// </summary>
    public static class VesselBookmarkUIDialog {
        
        private const string ConfirmDialogTag = "VesselBookmarkConfirmRemove";
        
        public static void ConfirmRemoval(Action onConfirm, Action onCancel = null, string title = null, string message = null) {
            if (onConfirm == null) return;
            
            // Use localized strings if not provided
            if (string.IsNullOrEmpty(title)) {
                title = VesselBookmarkLocalization.GetString("dialogRemoveTitle");
            }
            if (string.IsNullOrEmpty(message)) {
                message = VesselBookmarkLocalization.GetString("dialogRemoveMessage");
            }
            
            DialogGUIBase[] options = new DialogGUIBase[] {
                new DialogGUIButton(
                    VesselBookmarkLocalization.GetString("dialogButtonRemove"), 
                    () => {
                        onConfirm();
                    }, 
                    true
                ),
                new DialogGUIButton(
                    VesselBookmarkLocalization.GetString("dialogButtonCancel"), 
                    () => {
                        if (onCancel != null) {
                            onCancel();
                        }
                    }, 
                    true
                )
            };
            
            MultiOptionDialog dialog = new MultiOptionDialog(
                $"{ConfirmDialogTag}_{Time.time}",
                message,
                title,
                HighLogic.UISkin,
                options
            );
            
            PopupDialog.SpawnPopupDialog(
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                dialog,
                false,
                HighLogic.UISkin
            );
        }
    }
}
