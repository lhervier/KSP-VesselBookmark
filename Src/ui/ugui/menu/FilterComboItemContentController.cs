using TMPro;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared.ugui.combo.itemcontent;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu
{
    /// <summary>
    /// Combo item content for a filter option (body / type / situation): a single label. Reflects
    /// selection by recoloring the label ; a "disabled" option (one that matches no bookmark) stays
    /// dimmed whatever the selection state — the dim is purely informative, the row stays clickable.
    /// </summary>
    public class FilterComboItemContentController : ComboItemContentController
    {
        private TextMeshProUGUI _label;
        public FilterComboItemContentController WithLabelComponent(TextMeshProUGUI label)
        {
            this._label = label;
            return this;
        }

        private bool _enabled = true;
        public FilterComboItemContentController WithEnabled(bool enabled)
        {
            this._enabled = enabled;
            return this;
        }

        public override void SetSelected(bool selected)
        {
            if (_label == null) return;
            // A disabled option is always shown dimmed, whatever the selection state.
            if (!_enabled)
            {
                _label.color = VesselBookmarkPalette.ComboItemDisabledColor;
                return;
            }
            _label.color = selected
                ? VesselBookmarkPalette.ComboItemSelectedColor
                : VesselBookmarkPalette.ComboItemColor;
        }
    }
}
