using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using InteractiveStoryPlayer.Engine;
using InteractiveStoryPlayer.Models;
using InteractiveStoryPlayer.Services;

namespace InteractiveStoryPlayer.UI;

public partial class MainWindow : Window
{
    private readonly StoryEngine _engine;
    private readonly StoryLoader _storyLoader;
    private readonly SaveSystem _saveSystem;

    private Grid?      _mainMenuGrid;
    private Grid?      _storyPlayerGrid;
    private TextBlock? _menuTitleText;
    private TextBlock? _topBarTitle;
    private TextBlock? _sceneLabel;
    private TextBlock? _sceneText;
    private StackPanel? _imagesPanel;
    private StackPanel? _choicesPanel;
    private TextBlock? _statusText;

    private readonly string _defaultStoryPath;

    public MainWindow()
    {
        InitializeComponent();

        _engine      = new StoryEngine();
        _storyLoader = new StoryLoader();
        _saveSystem  = new SaveSystem();

        _defaultStoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "story.json");

        InitializeControls();
        SetupEventHandlers();
    }

    private void InitializeControls()
    {
        _mainMenuGrid    = this.FindControl<Grid>("MainMenuGrid");
        _storyPlayerGrid = this.FindControl<Grid>("StoryPlayerGrid");
        _menuTitleText   = this.FindControl<TextBlock>("MenuTitleText");
        _topBarTitle     = this.FindControl<TextBlock>("TopBarTitle");
        _sceneLabel      = this.FindControl<TextBlock>("SceneLabel");
        _sceneText       = this.FindControl<TextBlock>("SceneText");
        _imagesPanel     = this.FindControl<StackPanel>("ImagesPanel");
        _choicesPanel    = this.FindControl<StackPanel>("ChoicesPanel");
        _statusText      = this.FindControl<TextBlock>("StatusText");
    }

    private void SetupEventHandlers()
    {
        _engine.SceneChanged      += OnSceneChanged;
        _storyLoader.StoryFileChanged += OnStoryFileChanged;
    }

    // ── Menu Actions ─────────────────────────────────────────────────────────

    private async void OnStartNewStoryClick(object? sender, RoutedEventArgs e)
    {
        if (!File.Exists(_defaultStoryPath))
        {
            UpdateStatus("Story file not found.");
            return;
        }

        var story = await _storyLoader.LoadStoryAsync(_defaultStoryPath);
        if (story == null)
        {
            UpdateStatus("Failed to load story.");
            return;
        }

        var title = story.Meta?.Title ?? "Interactive Story";
        _engine.Load(story);
        ApplyStoryTitle(title);
        ShowStoryPlayer();
        UpdateStatus("New game started.");
    }

    private async void OnLoadProgressClick(object? sender, RoutedEventArgs e)
    {
        var saveData = await _saveSystem.LoadAsync();
        if (saveData?.StoryFilePath == null || !File.Exists(saveData.StoryFilePath))
        {
            UpdateStatus("No saved progress found.");
            return;
        }

        var story = await _storyLoader.LoadStoryAsync(saveData.StoryFilePath);
        if (story == null)
        {
            UpdateStatus("Failed to load story file.");
            return;
        }

        var title = story.Meta?.Title ?? "Interactive Story";
        _engine.Load(story);

        if (!string.IsNullOrEmpty(saveData.CurrentSceneId))
            _engine.GoTo(saveData.CurrentSceneId);

        ApplyStoryTitle(title);
        ShowStoryPlayer();
        UpdateStatus($"Resumed from {saveData.SavedDate:g}.");
    }

    private void OnExitClick(object? sender, RoutedEventArgs e) => Close();

    private void OnBackToMenuClick(object? sender, RoutedEventArgs e)
    {
        ShowMainMenu();
        UpdateStatus(string.Empty);
    }

    private async void OnSaveProgressClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_engine.CurrentSceneId))
        {
            UpdateStatus("Nothing to save.");
            return;
        }

        await _saveSystem.SaveAsync(_defaultStoryPath, _engine.CurrentSceneId);
        UpdateStatus($"Saved  ·  {DateTime.Now:t}");
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private void ShowMainMenu()
    {
        if (_mainMenuGrid    != null) _mainMenuGrid.IsVisible    = true;
        if (_storyPlayerGrid != null) _storyPlayerGrid.IsVisible = false;
    }

    private void ShowStoryPlayer()
    {
        if (_mainMenuGrid    != null) _mainMenuGrid.IsVisible    = false;
        if (_storyPlayerGrid != null) _storyPlayerGrid.IsVisible = true;
    }

    private void ApplyStoryTitle(string title)
    {
        if (_menuTitleText != null) _menuTitleText.Text = title.ToUpperInvariant();
        if (_topBarTitle   != null) _topBarTitle.Text   = title;
    }

    // ── Scene Rendering ───────────────────────────────────────────────────────

    private void OnSceneChanged(object? sender, EventArgs e) => UpdateSceneUI();

    private void UpdateSceneUI()
    {
        var scene = _engine.CurrentScene;
        if (scene == null) return;

        if (_sceneText  != null) _sceneText.Text  = scene.Text;
        if (_sceneLabel != null) _sceneLabel.Text  = _engine.CurrentSceneId.ToUpperInvariant();

        UpdateSceneImages(scene);
        UpdateChoices(scene.Choices);
        UpdateStatus($"Scene: {_engine.CurrentSceneId}");
    }

    private void UpdateSceneImages(Scene scene)
    {
        if (_imagesPanel == null) return;
        _imagesPanel.Children.Clear();

        var imagesToShow = new List<string>();
        if (scene.Images != null && scene.Images.Count > 0)
            imagesToShow.AddRange(scene.Images);
        else if (!string.IsNullOrEmpty(scene.Image))
            imagesToShow.Add(scene.Image);

        foreach (var imagePath in imagesToShow)
        {
            try
            {
                var storyDir = _storyLoader.GetStoryDirectory();
                var fullPath = storyDir != null
                    ? Path.Combine(storyDir, imagePath)
                    : imagePath;

                if (!File.Exists(fullPath)) continue;

                var img = new Image
                {
                    Source = new Bitmap(fullPath),
                    Stretch = Avalonia.Media.Stretch.Uniform,
                    MaxWidth  = 356,
                    MaxHeight = 280,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                var card = new Border
                {
                    Child        = img,
                    CornerRadius = new Avalonia.CornerRadius(6),
                    ClipToBounds = true,
                    BoxShadow    = BoxShadows.Parse("0 2 16 0 #000000"),
                    Background   = new SolidColorBrush(Color.FromRgb(12, 12, 16))
                };

                _imagesPanel.Children.Add(card);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image load error [{imagePath}]: {ex.Message}");
            }
        }
    }

    private void UpdateChoices(List<Choice>? choices)
    {
        if (_choicesPanel == null) return;
        _choicesPanel.Children.Clear();

        bool isEnding = choices == null || choices.Count == 0;

        if (!isEnding)
        {
            foreach (var choice in choices!)
                _choicesPanel.Children.Add(CreateChoiceButton(choice));
        }
        else
        {
            // ── Ending screen ──────────────────────────────────────────
            var divider = new Border
            {
                Height     = 1,
                Background = new SolidColorBrush(Color.Parse("#c9a84c20")),
                Margin     = new Avalonia.Thickness(0, 4, 0, 16)
            };

            var endLabel = new TextBlock
            {
                Text          = "— The End —",
                FontSize      = 16,
                FontWeight    = Avalonia.Media.FontWeight.Light,
                Foreground    = new SolidColorBrush(Color.Parse("#c9a84c")),
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                LetterSpacing = 2,
                Margin        = new Avalonia.Thickness(0, 0, 0, 14),
                Opacity       = 0.85
            };

            var restartBtn = new Button
            {
                Content                  = "↩  Play Again",
                Classes                  = { "restart" },
                HorizontalAlignment      = Avalonia.Layout.HorizontalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            restartBtn.Click += (_, _) =>
            {
                _engine.ResetToStart();
                UpdateStatus("Restarted.");
            };

            _choicesPanel.Children.Add(divider);
            _choicesPanel.Children.Add(endLabel);
            _choicesPanel.Children.Add(restartBtn);
        }
    }

    private Button CreateChoiceButton(Choice choice)
    {
        var btn = new Button
        {
            Content                  = $"›   {choice.Text}",
            Classes                  = { "choice" },
            HorizontalAlignment      = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Tag                      = choice.Next
        };
        btn.Click += OnChoiceClick;
        return btn;
    }

    private void OnChoiceClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string nextId)
            _engine.GoTo(nextId);
    }

    // ── Hot Reload ────────────────────────────────────────────────────────────

    private async void OnStoryFileChanged(object? sender, EventArgs e)
    {
        var filePath = _storyLoader.GetCurrentFilePath();
        if (filePath == null) return;

        var currentId = _engine.CurrentSceneId;
        var story     = await _storyLoader.LoadStoryAsync(filePath);

        if (story != null)
        {
            _engine.Load(story);
            if (!string.IsNullOrEmpty(currentId) && story.Scenes.ContainsKey(currentId))
                _engine.GoTo(currentId);
            UpdateStatus("Story reloaded.");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void UpdateStatus(string message)
    {
        if (_statusText != null)
            _statusText.Text = message;
    }

    protected override void OnClosed(EventArgs e)
    {
        _storyLoader.Dispose();
        base.OnClosed(e);
    }
}
