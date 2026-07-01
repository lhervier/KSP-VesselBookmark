using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.combo.itemcontent;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu
{
    /// <summary>
    /// Builds a filter combo item (body / type / situation): a single label whose text comes from an
    /// injected label resolver, dimmed when an injected "enabled" resolver reports the option matches
    /// no bookmark. Both resolvers are read live at Build, so re-listing the combo re-evaluates the
    /// dimming against the current bookmarks.
    /// </summary>
    public class FilterComboItemContentBuilder : BaseComboItemContentBuilder
    {
        // Raw option value -> displayed label. Injected dependency, read at Build.
        private Func<string, string> _labelFor;
        public FilterComboItemContentBuilder WithLabelFor(Func<string, string> labelFor)
        {
            this._labelFor = labelFor;
            return this;
        }

        // Raw option value -> whether it matches at least one bookmark. Null → always enabled.
        private Func<string, bool> _enabledFor;
        public FilterComboItemContentBuilder WithEnabledFor(Func<string, bool> enabledFor)
        {
            this._enabledFor = enabledFor;
            return this;
        }

        // Raw option value -> nesting depth (0 = no indent). Left margin = depth * indent step.
        // Injected dependency, read at Build. Null → no indentation.
        private Func<string, int> _indentFor;
        private float _indentStep;
        public FilterComboItemContentBuilder WithIndentFor(Func<string, int> indentFor, float indentStep)
        {
            this._indentFor = indentFor;
            this._indentStep = indentStep;
            return this;
        }

        public override ComboItemContentController Build()
        {
            string id = GetId();
            bool enabled = _enabledFor == null || _enabledFor(id);

            var labelGo = new GameObject("Label", typeof(RectTransform));
            var tmp = UGUILabels.AddLabel(labelGo);
            tmp.text = _labelFor != null ? _labelFor(id) : id;
            tmp.fontSize = ComboPalette.ComboFontSize;
            tmp.alignment = TextAlignmentOptions.Left;

            // Indent satellites under their parent : a left TMP margin shifts the text within its
            // rect, without touching the row height or the layout (unlike padding/leading spaces).
            if( _indentFor != null )
            {
                float indent = _indentFor(id) * _indentStep;
                if( indent > 0f )
                {
                    tmp.margin = new Vector4(indent, 0f, 0f, 0f);
                }
            }

            // The content carries the combo's standard single-line row height.
            var le = labelGo.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = ComboPalette.Height;

            // The label color is applied by SetSelected, which the row calls at startup and on changes.
            return labelGo
                .AddComponent<FilterComboItemContentController>()
                .WithLabelComponent(tmp)
                .WithEnabled(enabled);
        }
    }
}
