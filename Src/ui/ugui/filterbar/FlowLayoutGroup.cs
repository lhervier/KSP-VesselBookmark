using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.filterbar
{
    /// <summary>
    /// Layout group laying its children out left to right, wrapping to a new line when the next child
    /// would overflow the available width — what CSS calls "flex-wrap". uGUI ships no such group
    /// (HorizontalLayoutGroup never wraps, GridLayoutGroup imposes uniform cells), and the filter bar
    /// needs variable-width chips that fold onto a second line rather than overflowing the window.
    ///
    /// A child wider than the available width is clamped to it; making its own content shrink (ellipsis)
    /// is the child's business.
    /// </summary>
    public class FlowLayoutGroup : LayoutGroup
    {
        public float SpacingX = 4f;
        public float SpacingY = 3f;

        // Height depends on width, so it can only be computed once the horizontal pass has settled our
        // own rect. uGUI's rebuild order guarantees exactly that: every CalculateLayoutInputHorizontal,
        // then every SetLayoutHorizontal (which sizes us), then the vertical pair. So the flow is
        // simulated against rectTransform.rect.width from CalculateLayoutInputVertical onwards.

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            // Min width 0: the group may be squeezed, children then wrap (or get clamped). The
            // preferred width is everything on a single line, which the parent grants when it can.
            float preferred = padding.horizontal;
            for( int i = 0; i < rectChildren.Count; i++ )
            {
                preferred += LayoutUtility.GetPreferredWidth(rectChildren[i]);
                if( i > 0 ) preferred += SpacingX;
            }
            SetLayoutInputForAxis(0f, preferred, -1f, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            float height = Flow(false, 0);
            SetLayoutInputForAxis(height, height, -1f, 1);
        }

        public override void SetLayoutHorizontal()
        {
            Flow(true, 0);
        }

        public override void SetLayoutVertical()
        {
            Flow(true, 1);
        }

        /// <summary>
        /// Walks the children in flow order and returns the total height the group needs. When
        /// <paramref name="apply"/> is set, also positions and sizes each child along
        /// <paramref name="axis"/> (0 = horizontal, 1 = vertical).
        /// </summary>
        /// <param name="apply">Whether to actually place the children</param>
        /// <param name="axis">The axis to place them along (0 = x/width, 1 = y/height)</param>
        /// <returns>The height needed by the resulting layout, padding included</returns>
        private float Flow(bool apply, int axis)
        {
            float available = rectTransform.rect.width - padding.horizontal;
            float x = padding.left;
            float y = padding.top;
            float lineHeight = 0f;

            for( int i = 0; i < rectChildren.Count; i++ )
            {
                RectTransform child = rectChildren[i];
                float width = Mathf.Min(LayoutUtility.GetPreferredWidth(child), available);
                float height = LayoutUtility.GetPreferredHeight(child);

                // Wrap, unless we are already at the start of a line (a lone oversized child stays put:
                // it was clamped to the available width anyway).
                bool startOfLine = Mathf.Approximately(x, padding.left);
                if( !startOfLine && x + width > padding.left + available )
                {
                    x = padding.left;
                    y += lineHeight + SpacingY;
                    lineHeight = 0f;
                }

                if( apply )
                {
                    if( axis == 0 ) SetChildAlongAxis(child, 0, x, width);
                    else SetChildAlongAxis(child, 1, y, height);
                }

                x += width + SpacingX;
                lineHeight = Mathf.Max(lineHeight, height);
            }

            return y + lineHeight + padding.bottom;
        }
    }
}
