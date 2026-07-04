using KSP.UI.Screens;
using UnityEngine;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.popup;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    /// <summary>
    /// UI entry point: toolbar button + uGUI window. The window is driven by a (shared) PopupController
    /// that handles its own lazy spawn, position, and open state; we only open/close it and react to
    /// OnOpenChanged to sync the button and the business side effects (reloading the list, cancelling
    /// an edit).
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class BookmarksUI : MonoBehaviour {

        private static readonly ModLogger LOGGER = new ModLogger("MainUI");
        private const string DIALOG_ID = "VesselBookmarksUGUI";

        private ApplicationLauncherButton _toolbarButton;

        private BookmarksViewModel _viewModel;
        private PopupController _popupController;
        private BookmarksSettings _settings;

        private void Start() {
            GameEvents.onGUIApplicationLauncherReady.Add(OnLauncherReady);

            _viewModel = this.gameObject.AddComponent<BookmarksViewModel>();
            _viewModel.Initialize(BookmarksManager.Instance);

            _settings = new BookmarksSettings();
            _settings.Load();

            // Restore the persisted search criteria into the view model before wiring the save
            // handler, so replaying the restored values does not trigger a save.
            if (_settings.HasCriteria) {
                _viewModel.SelectedBody = _settings.SelectedBody;
                _viewModel.SelectedVesselType = _settings.SelectedVesselType;
                _viewModel.SelectedSituation = _settings.SelectedSituation;
                _viewModel.SearchText = _settings.SearchText;
                _viewModel.FilterHasComment = _settings.FilterHasComment;
            }
            _viewModel.OnSelectedBodyChanged.Add(OnCriteriaChanged);
            _viewModel.OnSelectedVesselTypeChanged.Add(OnCriteriaChanged);
            _viewModel.OnSelectedSituationChanged.Add(OnCriteriaChanged);
            _viewModel.OnSearchTextChanged.Add(OnCriteriaChanged);
            _viewModel.OnFilterHasCommentChanged.Add(OnCriteriaChanged);

            // The popup controller is a component on THIS GameObject: it survives KSP destroying the
            // window (Escape) and persists its own open state, so we no longer track visibility ourselves.
            _popupController = new PopupBuilder<TitleBarController, ContentController, BookmarksOverlaysController>()
                .WithHost(this.gameObject)
                .WithPopupID(DIALOG_ID)
                .WithTitle(ModLocalization.GetString("windowTitle"))
                .WithTitleBarBuilder(new TitleBarBuilder().WithViewModel(_viewModel))
                .WithContentBuilder(new ContentBuilder().WithViewModel(_viewModel))
                .WithOverlayBuilder(new BookmarksOverlaysBuilder().WithViewModel(_viewModel))
                .WithSize(new Vector2(VesselBookmarkPalette.WindowWidth, VesselBookmarkPalette.WindowHeight))
                .Build();
            // The controller restores its own open state (in its Start, after this method returns), so we
            // only subscribe: a restored-open window then syncs the toolbar and reloads through this handler.
            if (_popupController != null) {
                _popupController.OnOpenChanged.Add(OnPopupOpenChanged);
            }
        }

        private void OnDestroy() {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnLauncherReady);
            OnLauncherUnready();

            if (_viewModel != null) {
                _viewModel.OnSelectedBodyChanged.Remove(OnCriteriaChanged);
                _viewModel.OnSelectedVesselTypeChanged.Remove(OnCriteriaChanged);
                _viewModel.OnSelectedSituationChanged.Remove(OnCriteriaChanged);
                _viewModel.OnSearchTextChanged.Remove(OnCriteriaChanged);
                _viewModel.OnFilterHasCommentChanged.Remove(OnCriteriaChanged);
            }
            // _popup is a component on this GO: Unity destroys it with us, and it dismisses a still-open
            // window in its own OnDestroy. We only drop our reference and unsubscribe.
            if (_popupController != null) {
                _popupController.OnOpenChanged.Remove(OnPopupOpenChanged);
                _popupController = null;
            }
        }

        // ==========================================================================
        // VISIBILITY
        // ==========================================================================

        /// <summary>
        /// The window's open state changed (button, ×, Escape, or restore-at-load): sync the toolbar
        /// button, reload the list on open, and cancel any comment edit on close.
        /// </summary>
        private void OnPopupOpenChanged() {
            bool open = _popupController != null && _popupController.IsOpen;
            // Keep the toolbar button pressed state in sync, notably when the change is driven by KSP
            // (Escape) or by restore-at-load rather than by a click. SetTrue/SetFalse(false): do not
            // re-fire the toggle callbacks.
            if (_toolbarButton != null) {
                if (open) {
                    _toolbarButton.SetTrue(false);
                } else {
                    _toolbarButton.SetFalse(false);
                }
            }
            if (open) {
                _viewModel.ForceReload();
            } else {
                _viewModel.CancelBookmarkCommentEdition();
            }
        }

        /// <summary>
        /// A search criterion changed : memorize the whole current criteria set into the global settings.
        /// </summary>
        private void OnCriteriaChanged() {
            _settings.SetCriteria(
                _viewModel.SelectedBody,
                _viewModel.SelectedVesselType,
                _viewModel.SelectedSituation,
                _viewModel.SearchText,
                _viewModel.FilterHasComment);
            _settings.Save();
        }

        // ==========================================================================
        // TOOLBAR
        // ==========================================================================

        private void OnLauncherReady() {
            if (_toolbarButton == null) {
                try {
                    _toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                        OnToggleOn,
                        OnToggleOff,
                        null,
                        null,
                        null,
                        null,
                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.SPACECENTER,
                        GameDatabase.Instance.GetTexture("VesselBookmarkMod/icon", false) ?? Texture2D.whiteTexture
                    );
                } catch (System.Exception e) {
                    LOGGER.LogError($"Error creating Toolbar button: {e.Message}");
                }
                // The launcher may become ready after the window state was restored at scene load :
                // press the button now to reflect an already-open window (false : no callback).
                if (_toolbarButton != null && _popupController != null && _popupController.IsOpen) {
                    _toolbarButton.SetTrue(false);
                }
            }
        }

        private void OnLauncherUnready() {
            if (_toolbarButton != null) {
                try {
                    ApplicationLauncher.Instance.RemoveModApplication(_toolbarButton);
                } catch (System.Exception e) {
                    LOGGER.LogError($"Error removing Toolbar button: {e.Message}");
                }
                _toolbarButton = null;
            }
        }

        private void OnToggleOn() {
            if (_popupController != null) _popupController.Show();
        }

        private void OnToggleOff() {
            if (_popupController != null) _popupController.Hide();
        }
    }
}
