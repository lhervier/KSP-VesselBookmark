using UnityEngine;
using com.github.lhervier.ksp.shared.ugui.styles;
using static com.github.lhervier.ksp.shared.ugui.styles.Utils;

namespace com.github.lhervier.ksp.bookmarksmod.ui.styles
{
    /// <summary>
    /// Couleurs et métriques de l'UI uGUI, calquées sur la maquette ui_mockup.html (thème sombre,
    /// accent vert #8dbe45). Champs regroupés par zone via préfixes et bandeaux de commentaires.
    /// Aucune couleur/dimension ne doit être codée en dur dans les builders : tout vient d'ici.
    /// </summary>
    public static class VesselBookmarkPalette
    {
        // ==============================================================
        // Fenêtre principale
        // ==============================================================
        public const float WindowWidth = 420f;
        public const float WindowHeight = 540f;
        
        // ==============================================================
        // Title bar
        // ==============================================================
        
        // Badge compteur "X / Y"
        public const int CountFontSize = 10;
        public const float CountPaddingH = 6f;
        // (couleurs = AccentColor / AccentBorderColor / AccentBgColor)

        // Boutons du title bar (＋ ⟳ ⋯ ✕)
        public const float TitleButtonSize = 22f;
        public static readonly Color ButtonColor = Rgb(56, 56, 56);          // #383838
        public static readonly Color ButtonHoverColor = Rgb(72, 72, 72);       // #484848
        // Point vert "filtre actif" sur le bouton ⋯
        public const float FilterDotSize = 6f;

        // ==============================================================
        // Menu des filtres (déroulant "⋯")
        // ==============================================================
        public const float MenuWidth = 260f;
        public const float MenuSpacing = 4f;
        public const float MenuPaddingLeft = 10f;
        public const float MenuPaddingRight = 10f;
        public const float MenuPaddingTop = 6f;
        public const float MenuPaddingBottom = 6f;
        public const int MenuThickness = 1;
        public const int MenuTitleFontSize = 10;
        public const int MenuLabelFontSize = 11;
        public const float MenuComboLableWidth = 46f;

        public static readonly Color MenuBgColor = Rgb(30, 30, 30);            // #1e1e1e
        public static readonly Color MenuBorderColor = Rgb(85, 85, 85);        // #555
        public static readonly Color MenuTitleColor = Rgb(85, 85, 85);         // #555
        public static readonly Color MenuLabelColor = Rgb(153, 153, 153);      // #999
        public static readonly Color MenuSeparatorColor = Rgb(42, 42, 42);     // #2a2a2a

        // Champ de recherche
        public const int SearchFontSize = 12;
        public const float SearchPaddingH = 7f;
        public static readonly Color SearchBgColor = Rgb(13, 13, 13);          // #0d0d0d
        public static readonly Color SearchBorderColor = Rgb(42, 42, 42);      // #2a2a2a
        public static readonly Color SearchTextColor = Rgb(232, 232, 232);     // #e8e8e8
        public static readonly Color SearchPlaceholderColor = Rgb(85, 85, 85); // #555

        // Combos (corps / type)
        public const float ComboHeight = 22f;
        public static readonly Color ComboItemHoverColor = Rgb(42, 42, 42);    // #2a2a2a
        public static readonly Color ComboItemColor = Rgb(221, 221, 221);          // #ddd (option normale)
        public static readonly Color ComboItemSelectedColor = Rgb(141, 190, 69);   // accent (option sélectionnée)
        public static readonly Color ComboItemDisabledColor = Rgb(102, 102, 102);  // #666 (option sans bookmark)
        
        // ==============================================================
        // Corps : liste scrollable
        // ==============================================================
        public const float ScrollbarWidth = 8f;
        public static readonly Color ScrollbarColor = Rgb(136, 136, 136);      // #888

        // En-tête de section (module de commande / vaisseau)
        public const float SectionHeaderHeight = 24f;
        public const int SectionHeaderBorderThickness = 1;
        public const int SectionNameFontSize = 11;
        public const int SectionCountFontSize = 10;
        public static readonly Color SectionHeaderBgColor = Rgb(26, 26, 26);   // #1a1a1a
        public static readonly Color SectionHeaderBorderColor = Rgb(34, 34, 34); // #222
        public static readonly Color SectionNameColor = Rgb(221, 221, 221);    // #ddd
        public static readonly Color SectionCountColor = Rgb(85, 85, 85);      // #555

        // Texte d'aide sous l'en-tête de section
        public const int SectionHintFontSize = 11;
        public const float SectionHintPaddingH = 10f;
        public const float SectionHintPaddingV = 5f;
        public static readonly Color SectionHintBgColor = Rgb(18, 18, 18);     // #121212
        public static readonly Color SectionHintTextColor = Rgb(85, 85, 85);   // #555

        // ==============================================================
        // Ligne de bookmark
        // ==============================================================
        public const float RowPaddingH = 8f;
        public const float RowPaddingV = 6f;
        public const float RowSpacing = 7f;
        public const float RowAccentBarThickness = 2f;        // liseré gauche (sélection / actif)

        public static readonly Color RowHoverColor = Rgba(255, 255, 255, 0.03f);
        public static readonly Color RowSelectedBgColor = Rgba(141, 190, 69, 0.06f);
        public static readonly Color RowActiveBgColor = Rgba(141, 190, 69, 0.09f);

        // Icône type de vaisseau
        public const float TypeIconSize = 20f;
        public const int TypeIconBorderThickness = 1;
        public static readonly Color TypeIconBgColor = Rgb(31, 31, 31);        // #1f1f1f
        public static readonly Color TypeIconBorderColor = Rgb(68, 68, 68);    // #444

        // Icône alarme
        public const float AlarmIconSize = 16f;
        // (couleur = WarmColor)

        // Titre du bookmark
        public const int NameFontSize = 13;
        public static readonly Color NameColor = Rgb(221, 221, 221);           // #ddd
        public static readonly Color NameActiveColor = Rgb(255, 255, 255);     // #fff (vaisseau actif)
        public static readonly Color NameTargetColor = Rgb(141, 190, 69);      // accent (cible)
        public static readonly Color NameMissingColor = Rgb(102, 102, 102);    // #666 (vaisseau disparu)

        // Pastilles d'état (Actif / Cible / Disparu)
        public const int ChipFontSize = 9;
        public const float ChipPaddingH = 5f;
        public const int ChipBorderThickness = 1;
        // Actif/Cible = AccentColor/AccentBorderColor/AccentBgColor
        public static readonly Color ChipMissingTextColor = Rgb(192, 89, 79);
        public static readonly Color ChipMissingBorderColor = Rgb(110, 42, 32);
        public static readonly Color ChipMissingBgColor = Rgba(192, 89, 79, 0.10f);

        // Boutons d'ordre/suppression révélés au survol (▲ ▼ ✕)
        public const float RowButtonSize = 18f;
        public const int RowButtonFontSize = 11;
        public const int RowButtonBorderThickness = 1;
        public const float RowButtonSpacing = 3f;
        public static readonly Color RowButtonBgColor = Rgb(42, 42, 42);       // #2a2a2a
        public static readonly Color RowButtonHoverColor = Rgb(56, 56, 56);    // #383838
        public static readonly Color RowButtonDangerHoverColor = Rgb(90, 36, 29); // #5a241d

        // Ligne 2 : situation (+ nom du vaisseau)
        public const int SituationFontSize = 11;
        public static readonly Color SituationColor = Rgb(136, 136, 136);      // #888
        public static readonly Color VesselNameColor = Rgb(85, 85, 85);        // #555

        // Ligne 3 : commentaire
        public const int CommentFontSize = 11;
        public const float CommentPaddingH = 8f;
        public const float CommentPaddingV = 4f;
        public const int CommentBorderThickness = 2;          // liseré gauche
        public static readonly Color CommentTextColor = Rgb(169, 138, 74);     // #a98a4a
        public static readonly Color CommentBgColor = Rgba(176, 115, 24, 0.06f);
        public static readonly Color CommentBorderColor = Rgb(74, 58, 24);     // #4a3a18

        // ==============================================================
        // Barre d'actions du bas (sur la sélection)
        // ==============================================================
        public const float FooterPaddingH = 9f;
        public const float FooterPaddingV = 7f;
        public const float FooterSpacing = 8f;
        public const int FooterBorderThickness = 1;
        public const float FooterHeight = 40f;   // FooterButtonHeight + 2 * FooterPaddingV
        public const int FooterSelFontSize = 11;
        public const float FooterButtonHeight = 26f;
        public const int FooterButtonFontSize = 22;

        public static readonly Color FooterBgColor = PopupPalette.TitleBarBackgroundColor;     // #2e2e2e
        public static readonly Color FooterBorderColor = PopupPalette.TitleBarSeparatorColor;  // #444
        public static readonly Color FooterSelColor = Rgb(119, 119, 119);      // #777
        
        // ==============================================================
        // Overlays internes (édition commentaire + confirmation suppression)
        // ==============================================================
        
        public const int CardSubFontSize = 11;
        public const int CardMsgFontSize = 13;
        public const float CardFootSpacing = 8f;
        
        public static readonly Color CardSubColor = Rgb(119, 119, 119);       // #777
        public static readonly Color CardMsgColor = Rgb(204, 204, 204);        // #ccc

        // Zone de texte du commentaire (fond/bordure/couleurs/marge fournis par le TextField partagé).
        public const float TextAreaHeight = 90f;
        public const int TextAreaFontSize = 13;

        // Boutons des cartes (OK / Annuler / Supprimer)
        public const float CardButtonHeight = 28f;
        public const float CardButtonPaddingH = 14f;
        public const int CardButtonFontSize = 12;
        public static readonly Color CardButtonOkTextColor = Rgb(141, 190, 69);
        public static readonly Color CardButtonOkBgColor = Rgba(141, 190, 69, 0.12f);
        public static readonly Color CardButtonDangerTextColor = Rgb(192, 89, 79);
        public static readonly Color CardButtonDangerBgColor = Rgba(192, 89, 79, 0.12f);
    }
}
