using UnityEngine;
using TMPro;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins.remove
{
    /// <summary>
    /// Holds the remove-confirmation popup message label so the popin orchestrator can fill it with
    /// the pending bookmark's name.
    /// </summary>
    public class RemoveConfirmContentController : MonoBehaviour
    {
        private TextMeshProUGUI _message;
        public RemoveConfirmContentController WithMessageComponent(TextMeshProUGUI message)
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
