using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment
{
    /// <summary>
    /// Holds the edit-comment popup content widgets (the bookmark subtitle and the comment text area)
    /// so the overlay orchestrator can bind them to the ViewModel.
    /// </summary>
    public class EditCommentContentController : MonoBehaviour
    {
        private Text _sub;
        public Text Sub => _sub;
        public EditCommentContentController BindSub(Text sub)
        {
            this._sub = sub;
            return this;
        }

        private InputField _input;
        public InputField Input => _input;
        public EditCommentContentController BindInput(InputField input)
        {
            this._input = input;
            return this;
        }
    }
}
