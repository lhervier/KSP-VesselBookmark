using UnityEngine;
using com.github.lhervier.ksp.shared.ugui.button;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment
{
    /// <summary>
    /// Holds the edit-comment popup footer buttons (Cancel / Save) so the overlay orchestrator can wire
    /// their clicks to the ViewModel.
    /// </summary>
    public class EditCommentFooterController : MonoBehaviour
    {
        private ButtonController _cancel;
        public ButtonController Cancel => _cancel;
        public EditCommentFooterController BindCancel(ButtonController cancel)
        {
            this._cancel = cancel;
            return this;
        }

        private ButtonController _ok;
        public ButtonController Ok => _ok;
        public EditCommentFooterController BindOk(ButtonController ok)
        {
            this._ok = ok;
            return this;
        }
    }
}
