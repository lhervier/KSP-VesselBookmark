using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.remove
{
    /// <summary>
    /// Holds the remove-confirmation popup message label so the overlay orchestrator can fill it with
    /// the pending bookmark's name.
    /// </summary>
    public class RemoveConfirmContentController : MonoBehaviour
    {
        private Text _message;
        public RemoveConfirmContentController Message(Text message)
        {
            this._message = message;
            return this;
        }

        public Text GetMessage()
        {
            return _message;
        }
    }
}
