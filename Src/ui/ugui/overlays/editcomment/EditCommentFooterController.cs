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
        private ButtonController _cancelButtonController;
        public EditCommentFooterController WithCancelButtonController(ButtonController cancel)
        {
            this._cancelButtonController = cancel;
            return this;
        }

        private ButtonController _okButtonController;
        public EditCommentFooterController WithOkButtonController(ButtonController ok)
        {
            this._okButtonController = ok;
            return this;
        }

        public ButtonController GetCancelButtonController()
        {
            return _cancelButtonController;
        }

        public ButtonController GetOkButtonController()
        {
            return _okButtonController;
        }
    }
}
