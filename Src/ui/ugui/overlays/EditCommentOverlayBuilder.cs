using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.bookmarksmod;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays
{
    /// <summary>
    /// Fausse popup interne d'édition du commentaire (calque par-dessus la fenêtre). Pilotée par
    /// ViewModel.EditingComment ; la zone de texte est liée à ViewModel.Comment ; OK enregistre,
    /// Annuler restaure.
    /// </summary>
    public class EditCommentOverlayBuilder
    {
        // Verrou des commandes de jeu pendant que le champ a le focus : sinon KSP lit le clavier en
        // parallèle (« c » bascule la caméra, etc.). Le champ Unity reçoit les frappes quoi qu'il arrive.
        internal const string LOCK_ID = "VesselBookmarkMod_EditComment";

        private readonly BookmarksViewModel _viewModel;

        public EditCommentOverlayBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public EditCommentOverlayController Create(Transform parent)
        {
            GameObject panel;
            RectTransform card;
            GameObject root = OverlayCard.Build(parent, "Bookmarks.EditOverlay", out panel, out card);
            EditCommentOverlayController controller = root.AddComponent<EditCommentOverlayController>();
            controller.Initialize(_viewModel);
            controller.BindPanel(panel);

            OverlayCard.AddText(card, "Title",
                ModLocalization.GetString("editWindowTitle"),
                VesselBookmarkPalette.CardTitleFontSize, VesselBookmarkPalette.AccentColor, FontStyle.Bold);

            Text sub = OverlayCard.AddText(card, "Sub", string.Empty,
                VesselBookmarkPalette.CardSubFontSize, VesselBookmarkPalette.CardSubColor);
            controller.BindSub(sub);

            InputField input = BuildTextArea(card, controller);
            controller.BindInput(input);

            GameObject foot = OverlayCard.AddFootRow(card);

            // Cancel keeps the default button colors (VBMButtonBuilder defaults); only the auto-width
            // text-button shape, height and font size are set here.
            ButtonController cancel = new VBMButtonBuilder()
                .ObjectName("Cancel")
                .Label(ModLocalization.GetString("buttonCancel"))
                .AutoWidth(VesselBookmarkPalette.CardButtonPaddingH)
                .Size(VesselBookmarkPalette.CardButtonHeight)
                .FontSize(VesselBookmarkPalette.CardButtonFontSize)
                .Build();
            cancel.OnClick.Add(() => _viewModel.CancelBookmarkCommentEdition());
            cancel.transform.SetParent(foot.transform, false);

            ButtonController ok = new VBMButtonBuilder()
                .ObjectName("OK")
                .Label(ModLocalization.GetString("buttonSave"))
                .AutoWidth(VesselBookmarkPalette.CardButtonPaddingH)
                .Size(VesselBookmarkPalette.CardButtonHeight)
                .FontSize(VesselBookmarkPalette.CardButtonFontSize)
                .BackgroundColor(VesselBookmarkPalette.CardButtonOkBgColor)
                .HoverColor(VesselBookmarkPalette.CardButtonOkBgColor)
                .TextColor(VesselBookmarkPalette.CardButtonOkTextColor)
                .Build();
            ok.OnClick.Add(() => _viewModel.SaveBookmarkComment());
            ok.transform.SetParent(foot.transform, false);

            return controller;
        }

        // Zone de texte multiligne (InputField + viewport masqué + texte + placeholder)
        private InputField BuildTextArea(Transform card, EditCommentOverlayController controller)
        {
            var inputGo = new GameObject("TextArea", typeof(RectTransform));
            inputGo.transform.SetParent(card, false);
            var le = inputGo.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.TextAreaHeight;

            var bg = inputGo.AddComponent<Image>();
            bg.sprite = Sprites.Border(VesselBookmarkPalette.TextAreaBgColor, VesselBookmarkPalette.TextAreaBorderColor, 1);
            bg.type = Image.Type.Sliced;
            bg.color = Color.white;
            bg.raycastTarget = true;

            // Montage classique (la version d'InputField livrée avec KSP n'a pas textViewport) :
            // RectMask2D sur le champ pour clipper, Text + Placeholder enfants avec marges.
            inputGo.AddComponent<RectMask2D>();

            var input = inputGo.AddComponent<InputField>();
            input.lineType = InputField.LineType.MultiLineNewline;

            int pad = Mathf.RoundToInt(VesselBookmarkPalette.TextAreaPadding);

            var placeholder = NewAreaText(inputGo.transform, "Placeholder", pad);
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.color = VesselBookmarkPalette.SearchPlaceholderColor;
            placeholder.text = string.Empty;

            var text = NewAreaText(inputGo.transform, "Text", pad);
            text.color = VesselBookmarkPalette.TextAreaTextColor;

            input.textComponent = text;
            input.placeholder = placeholder;

            input.onValueChanged.AddListener(controller.OnInputChanged);

            // Lock clavier au focus, unlock au blur (Unity envoie Select/Deselect au champ sélectionné)
            var trigger = inputGo.AddComponent<EventTrigger>();
            var selectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            selectEntry.callback.AddListener(_ => InputLockManager.SetControlLock(ControlTypes.All, LOCK_ID));
            trigger.triggers.Add(selectEntry);
            var deselectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
            deselectEntry.callback.AddListener(_ => InputLockManager.RemoveControlLock(LOCK_ID));
            trigger.triggers.Add(deselectEntry);

            return input;
        }

        private static Text NewAreaText(Transform parent, string objectName, int pad)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(pad, pad);
            rect.offsetMax = new Vector2(-pad, -pad);
            var text = go.AddComponent<Text>();
            text.font = HighLogic.UISkin.font;
            text.fontSize = VesselBookmarkPalette.TextAreaFontSize;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }

        public class EditCommentOverlayController : BaseController
        {
            private GameObject _panel;
            private Text _sub;
            private InputField _input;
            private bool _loading;

            public void BindPanel(GameObject panel) => this._panel = panel;
            public void BindSub(Text sub) => this._sub = sub;
            public void BindInput(InputField input) => this._input = input;

            public void OnInputChanged(string value)
            {
                if (_loading) return;
                ViewModel.Comment = value;
            }

            public void Start()
            {
                ViewModel.OnEditingCommentChanged.Add(OnEditingCommentChanged);
                OnEditingCommentChanged();
            }

            public void OnDestroy()
            {
                ViewModel?.OnEditingCommentChanged.Remove(OnEditingCommentChanged);
                InputLockManager.RemoveControlLock(LOCK_ID);   // sécurité si détruit en cours d'édition
            }

            private void OnEditingCommentChanged()
            {
                bool editing = ViewModel.EditingComment;
                if (_panel != null) _panel.SetActive(editing);

                if (!editing) {
                    // Sécurité : si on ferme alors que le champ avait encore le focus (le Deselect
                    // peut ne pas partir quand on désactive le panneau).
                    InputLockManager.RemoveControlLock(LOCK_ID);
                    return;
                }

                Bookmark sel = ViewModel.SelectedBookmark;
                if (_sub != null) _sub.text = sel != null ? sel.BookmarkTitle : string.Empty;

                _loading = true;
                if (_input != null) _input.text = ViewModel.Comment ?? string.Empty;
                _loading = false;

                if (_input != null) _input.ActivateInputField();
            }
        }
    }
}
