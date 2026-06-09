using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Shared ButtonBuilder pre-seeded with VesselBookmark's default button size and colors. It only
    /// sets default values in its constructor and adds nothing else; the base contract is unchanged.
    /// Callers use the inherited fluent API and override any default as needed.
    /// </summary>
    public class VBMButtonBuilder : ButtonBuilder
    {
        public VBMButtonBuilder()
        {
            Size(VesselBookmarkPalette.TitleButtonSize);
            BackgroundColor(VesselBookmarkPalette.ButtonBgColor);
            HoverColor(VesselBookmarkPalette.ButtonHoverColor);
        }
    }
}
