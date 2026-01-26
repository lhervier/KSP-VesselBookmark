using System;
using KSP.UI.Screens;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Helper for simple confirmation dialogs.
    /// </summary>
    public static class VesselBookmarkUIDialog {
        
        private const string ConfirmDialogTag = "VesselBookmarkConfirmRemove";
        
        public static void ConfirmRemoval(Action onConfirm, Action onCancel = null, string title = "Supprimer le bookmark", string message = "Voulez-vous supprimer ce bookmark ?") {
            if (onConfirm == null) return;
            
            DialogGUIBase[] options = new DialogGUIBase[] {
                new DialogGUIButton("Supprimer", () => {
                    onConfirm();
                }, true),
                new DialogGUIButton("Annuler", () => {
                    if (onCancel != null) {
                        onCancel();
                    }
                }, true)
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
