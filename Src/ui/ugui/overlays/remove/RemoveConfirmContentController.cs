using UnityEngine;
using TMPro;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.remove
{
    /// <summary>
    /// Holds the remove-confirmation popup message label so the overlay orchestrator can fill it with
    /// the pending bookmark's name.
    /// </summary>
    public class RemoveConfirmContentController : MonoBehaviour
    {
        private TextMeshProUGUI _message;
        public RemoveConfirmContentController Message(TextMeshProUGUI message)
        {
            this._message = message;
            return this;
        }

        public TextMeshProUGUI GetMessage()
        {
            return _message;
        }
    }
}
