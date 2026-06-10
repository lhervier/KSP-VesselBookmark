using UnityEngine;
using com.github.lhervier.ksp.shared.ugui.button;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.remove
{
    /// <summary>
    /// Holds the remove-confirmation popup footer buttons (Cancel / Remove) so the overlay orchestrator
    /// can wire their clicks to the ViewModel.
    /// </summary>
    public class RemoveConfirmFooterController : MonoBehaviour
    {
        private ButtonController _cancel;
        public ButtonController Cancel => _cancel;
        public RemoveConfirmFooterController BindCancel(ButtonController cancel)
        {
            this._cancel = cancel;
            return this;
        }

        private ButtonController _remove;
        public ButtonController Remove => _remove;
        public RemoveConfirmFooterController BindRemove(ButtonController remove)
        {
            this._remove = remove;
            return this;
        }
    }
}
