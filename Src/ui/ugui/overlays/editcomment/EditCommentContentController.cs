using UnityEngine;
using TMPro;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment
{
    /// <summary>
    /// Holds the edit-comment popup content widgets (the bookmark subtitle and the comment text area)
    /// so the overlay orchestrator can bind them to the ViewModel.
    /// </summary>
    public class EditCommentContentController : MonoBehaviour
    {
        private TextMeshProUGUI _sub;
        public EditCommentContentController Sub(TextMeshProUGUI sub)
        {
            this._sub = sub;
            return this;
        }

        private TMP_InputField _input;
        public EditCommentContentController Input(TMP_InputField input)
        {
            this._input = input;
            return this;
        }

        public TextMeshProUGUI GetSub()
        {
            return _sub;
        }

        public TMP_InputField GetInput()
        {
            return _input;
        }
        
    }
}
