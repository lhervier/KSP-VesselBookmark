using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.textfield;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment
{
    /// <summary>
    /// Holds the edit-comment popup content widgets (the bookmark subtitle and the comment text area)
    /// so the overlay orchestrator can bind them to the ViewModel.
    /// </summary>
    public class EditCommentContentController : MonoBehaviour
    {
        private TextMeshProUGUI _sub;
        public EditCommentContentController WithSubComponent(TextMeshProUGUI sub)
        {
            this._sub = sub;
            return this;
        }

        private TextFieldController _textFieldController;
        public EditCommentContentController WithTextFieldController(TextFieldController textFieldController)
        {
            this._textFieldController = textFieldController;
            return this;
        }

        public TextMeshProUGUI GetSub()
        {
            return _sub;
        }

        public TextFieldController GetTextFieldController()
        {
            return _textFieldController;
        }

    }
}
