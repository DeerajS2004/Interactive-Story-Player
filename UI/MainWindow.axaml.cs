using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using InteractiveStoryPlayer.Engine;
using InteractiveStoryPlayer.Models;
using InteractiveStoryPlayer.Services;

namespace InteractiveStoryPlayer.UI;

public partial class MainWindow : Window
{
    // ── Services ──────────────────────────────────────────────────────────────
    private readonly StoryEngine _engine;
    private readonly StoryLoader _storyLoader;
    private readonly SaveSystem  _saveSystem;

    // ── Layout mode ───────────────────────────────────────────────────────────
    private enum LayoutMode { NoImage, Portrait, Landscape }
    private LayoutMode _currentLayout = LayoutMode.NoImage;

    // ── Top-level named controls (from AXAML) ─────────────────────────────────
    private Grid?      _mainMenuGrid;
    private Grid?      _storyPlayerGrid;
    private TextBlock? _menuTitleText;
    private TextBlock? _topBarTitle;
    private TextBlock? _sceneLabel;
    private TextBlock? _statusText;
    private Grid?      _contentGrid;

    // ── Story panel controls (built in code) ──────────────────────────────────
    private ScrollViewer _storyScrollViewer = null!;
    private TextBlock    _sceneChapterLabel = null!;
    private TextBlock    _sceneText         = null!;
    private TextBlock    _fontSizeLabel     = null!;
    private StackPanel   _choicesPanel      = null!;

    // ── Image panel controls (built in code) ──────────────────────────────────
    private Border       _imageContainer    = null!;
    private ScrollViewer _imageScrollViewer = null!;
    private StackPanel   _imageStack        = null!;

    // ── Font state ────────────────────────────────────────────────────────────
    private int       _fontSize     = 17;
    private const int FontMin       = 12;
    private const int FontMax       = 28;

    private readonly string _defaultStoryPath;

    // ─────────────────────────────────────────────────────────────────────────
    public MainWindow()
    {
        InitializeComponent();

        _engine      = new StoryEngine();
        _storyLoader = new StoryLoader();
        _saveSystem  = new SaveSystem();

        _defaultStoryPath = Path.Combine(
            Directory.GetCurrentDirectory(), "Assets", "story.json");

        BindAxamlControls();
        BuildStoryPanel();
        BuildImagePanel();
        SetupEventHandlers();

        // Start with no-image layout (empty content grid)
        ApplyLayout(LayoutMode.NoImage);
    }

    // ── Bind controls that exist in AXAML ────────────────────────────────────
    private void BindAxamlControls()
    {
        _mainMenuGrid    = this.FindControl<Grid>("MainMenuGrid");
        _storyPlayerGrid = this.FindControl<Grid>("StoryPlayerGrid");
        _menuTitleText   = this.FindControl<TextBlock>("MenuTitleText");
        _topBarTitle     = this.FindControl<TextBlock>("TopBarTitle");
        _sceneLabel      = this.FindControl<TextBlock>("SceneLabel");
        _statusText      = this.FindControl<TextBlock>("StatusText");
        _contentGrid     = this.FindControl<Grid>("ContentGrid");
    }

    // ── Build story reading panel entirely in code ───────────────────────────
    private void BuildStoryPanel()
    {
        // ── Scene chapter label ──
        _sceneChapterLabel = new TextBlock
        {
            FontSize            = 9,
            Foreground          = new SolidColorBrush(Color.Parse("#2e2e40")),
            VerticalAlignment   = VerticalAlignment.Center,
            LetterSpacing       = 2
        };

        // ── Font size label ──
        _fontSizeLabel = new TextBlock
        {
            Text                  = _fontSize.ToString(),
            FontSize              = 10,
            FontWeight            = FontWeight.SemiBold,
            Foreground            = new SolidColorBrush(Color.Parse("#45455a")),
            HorizontalAlignment   = HorizontalAlignment.Center,
            VerticalAlignment     = VerticalAlignment.Center
        };

        // ── A− button ──
        var decBtn = new Button { Content = "A−" };
        decBtn.Classes.Add("font-btn");
        decBtn.Click += (_, _) => SetFontSize(_fontSize - 1);

        // ── A+ button ──
        var incBtn = new Button { Content = "A+" };
        incBtn.Classes.Add("font-btn");
        incBtn.Click += (_, _) => SetFontSize(_fontSize + 1);

        // ── Font size counter badge ──
        var sizeBadge = new Border
        {
            Background      = new SolidColorBrush(Color.Parse("#0e0e16")),
            BorderBrush     = new SolidColorBrush(Color.Parse("#1e1e2a")),
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(3),
            Width           = 32,
            Height          = 24,
            Child           = _fontSizeLabel
        };

        // ── Font label + controls row ──
        var fontLabel = new TextBlock
        {
            Text              = "FONT",
            FontSize          = 9,
            Foreground        = new SolidColorBrush(Color.Parse("#25253a")),
            LetterSpacing     = 1.5,
            VerticalAlignment = VerticalAlignment.Center
        };

        var fontRow = new StackPanel
        {
            Orientation       = Orientation.Horizontal,
            Spacing           = 6,
            VerticalAlignment = VerticalAlignment.Center
        };
        fontRow.Children.Add(fontLabel);
        fontRow.Children.Add(decBtn);
        fontRow.Children.Add(sizeBadge);
        fontRow.Children.Add(incBtn);

        // ── Font bar grid: chapter label left, font controls right ──
        var fontBarGrid = new Grid();
        fontBarGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        fontBarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        Grid.SetColumn(_sceneChapterLabel, 0);
        Grid.SetColumn(fontRow, 1);
        fontBarGrid.Children.Add(_sceneChapterLabel);
        fontBarGrid.Children.Add(fontRow);

        var fontBar = new Border
        {
            Background      = new SolidColorBrush(Color.Parse("#0a0a0d")),
            BorderBrush     = new SolidColorBrush(Color.Parse("#141420")),
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(5),
            Padding         = new Thickness(12, 7),
            Margin          = new Thickness(0, 0, 0, 10),
            Child           = fontBarGrid
        };

        // ── Scene text ──
        _sceneText = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize     = _fontSize,
            FontFamily   = new FontFamily("Georgia, 'Times New Roman', serif"),
            Foreground   = new SolidColorBrush(Color.Parse("#c4c4d4")),
            LineHeight   = _fontSize * 1.75
        };

        var textCard = new Border
        {
            Background      = new SolidColorBrush(Color.Parse("#0e0e12")),
            CornerRadius    = new CornerRadius(6),
            BorderBrush     = new SolidColorBrush(Color.Parse("#181822")),
            BorderThickness = new Thickness(1),
            BoxShadow       = BoxShadows.Parse("0 4 40 0 #000000"),
            Padding         = new Thickness(32, 28),
            Child           = _sceneText
        };

        // ── Choices panel ──
        _choicesPanel = new StackPanel
        {
            Spacing = 7,
            Margin  = new Thickness(0, 14, 0, 0)
        };

        // ── Outer story stack ──
        var storyStack = new StackPanel
        {
            Margin  = new Thickness(32, 24, 32, 32),
            Spacing = 0
        };
        storyStack.Children.Add(fontBar);
        storyStack.Children.Add(textCard);
        storyStack.Children.Add(_choicesPanel);

        _storyScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility   = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content                       = storyStack
        };
    }

    // ── Build image panel (persistent, reconfigured per layout) ──────────────
    private void BuildImagePanel()
    {
        _imageStack = new StackPanel { Spacing = 0 };

        _imageScrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility   = ScrollBarVisibility.Auto,
            Content                       = _imageStack
        };

        _imageContainer = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#09090b")),
            Child      = _imageScrollViewer
        };
    }

    // ── Layout reconfiguration ────────────────────────────────────────────────
    private void ApplyLayout(LayoutMode mode)
    {
        if (_contentGrid == null) return;

        // Remove both panels (safe even if not currently children)
        _contentGrid.Children.Remove(_storyScrollViewer);
        _contentGrid.Children.Remove(_imageContainer);
        _contentGrid.ColumnDefinitions.Clear();
        _contentGrid.RowDefinitions.Clear();

        switch (mode)
        {
            // ── No image: story fills the whole area ──────────────────────────
            case LayoutMode.NoImage:
                _contentGrid.Children.Add(_storyScrollViewer);
                break;

            // ── Portrait: story left, image panel right (380 px) ─────────────
            case LayoutMode.Portrait:
                _contentGrid.ColumnDefinitions.Add(
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                _contentGrid.ColumnDefinitions.Add(
                    new ColumnDefinition(new GridLength(380, GridUnitType.Pixel)));

                _imageContainer.BorderThickness = new Thickness(1, 0, 0, 0);
                _imageContainer.BorderBrush     =
                    new SolidColorBrush(Color.Parse("#13131c"));

                _imageStack.Orientation       = Orientation.Vertical;
                _imageStack.HorizontalAlignment = HorizontalAlignment.Stretch;
                _imageStack.VerticalAlignment   = VerticalAlignment.Top;
                _imageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                Grid.SetColumn(_storyScrollViewer, 0);
                Grid.SetColumn(_imageContainer,    1);
                _contentGrid.Children.Add(_storyScrollViewer);
                _contentGrid.Children.Add(_imageContainer);
                break;

            // ── Landscape: image strip on top, story below ───────────────────
            case LayoutMode.Landscape:
                _contentGrid.RowDefinitions.Add(
                    new RowDefinition(GridLength.Auto));
                _contentGrid.RowDefinitions.Add(
                    new RowDefinition(new GridLength(1, GridUnitType.Star)));

                _imageContainer.BorderThickness = new Thickness(0, 0, 0, 1);
                _imageContainer.BorderBrush     =
                    new SolidColorBrush(Color.Parse("#13131c"));

                _imageStack.Orientation         = Orientation.Horizontal;
                _imageStack.HorizontalAlignment = HorizontalAlignment.Center;
                _imageStack.VerticalAlignment   = VerticalAlignment.Center;
                _imageScrollViewer.VerticalScrollBarVisibility   = ScrollBarVisibility.Disabled;
                _imageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

                Grid.SetRow(_imageContainer,    0);
                Grid.SetRow(_storyScrollViewer, 1);
                _contentGrid.Children.Add(_imageContainer);
                _contentGrid.Children.Add(_storyScrollViewer);
                break;
        }

        _currentLayout = mode;
    }

    // ── Image loading and rendering ───────────────────────────────────────────
    private void RenderImages(Scene scene)
    {
        _imageStack.Children.Clear();

        // Collect paths
        var paths = new List<string>();
        if (scene.Images?.Count > 0)
            paths.AddRange(scene.Images);
        else if (!string.IsNullOrEmpty(scene.Image))
            paths.Add(scene.Image);

        // Resolve full paths and load bitmaps
        var bitmaps = new List<Bitmap>();
        foreach (var rel in paths)
        {
            try
            {
                var dir  = _storyLoader.GetStoryDirectory();
                var full = dir != null ? Path.Combine(dir, rel) : rel;
                if (File.Exists(full))
                    bitmaps.Add(new Bitmap(full));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image load error [{rel}]: {ex.Message}");
            }
        }

        // Determine layout from first image's aspect ratio
        LayoutMode targetLayout;
        if (bitmaps.Count == 0)
        {
            targetLayout = LayoutMode.NoImage;
        }
        else
        {
            var first  = bitmaps[0];
            var w      = first.PixelSize.Width;
            var h      = first.PixelSize.Height;
            targetLayout = (w > h) ? LayoutMode.Landscape : LayoutMode.Portrait;
        }

        // Only reconfigure grid if layout mode changed
        if (targetLayout != _currentLayout)
            ApplyLayout(targetLayout);

        // Populate image stack
        foreach (var bmp in bitmaps)
        {
            var img = new Image
            {
                Source              = bmp,
                Stretch             = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center
            };

            if (targetLayout == LayoutMode.Portrait)
            {
                // Fill the 380 px column width; height is auto via Uniform
                img.HorizontalAlignment = HorizontalAlignment.Stretch;
                img.VerticalAlignment   = VerticalAlignment.Top;
            }
            else if (targetLayout == LayoutMode.Landscape)
            {
                // Fixed height; width scales by Uniform to keep full image visible
                img.Height = 260;
                img.Margin = new Thickness(4, 4, 4, 4);
            }

            _imageStack.Children.Add(img);
        }
    }

    // ── Font size ─────────────────────────────────────────────────────────────
    private void SetFontSize(int size)
    {
        _fontSize            = Math.Clamp(size, FontMin, FontMax);
        _sceneText.FontSize  = _fontSize;
        _sceneText.LineHeight = _fontSize * 1.75;
        _fontSizeLabel.Text  = _fontSize.ToString();
    }

    // ── Menu actions ──────────────────────────────────────────────────────────
    private async void OnStartNewStoryClick(object? sender, RoutedEventArgs e)
    {
        if (!File.Exists(_defaultStoryPath))
        {
            UpdateStatus("Story file not found.");
            return;
        }

        var story = await _storyLoader.LoadStoryAsync(_defaultStoryPath);
        if (story == null) { UpdateStatus("Failed to load story."); return; }

        _engine.Load(story);
        ApplyStoryTitle(story.Meta?.Title ?? "Interactive Story");
        ShowStoryPlayer();
        UpdateStatus("New game started.");
    }

    private async void OnLoadProgressClick(object? sender, RoutedEventArgs e)
    {
        var save = await _saveSystem.LoadAsync();
        if (save?.StoryFilePath == null || !File.Exists(save.StoryFilePath))
        {
            UpdateStatus("No saved progress found.");
            return;
        }

        var story = await _storyLoader.LoadStoryAsync(save.StoryFilePath);
        if (story == null) { UpdateStatus("Failed to load story file."); return; }

        _engine.Load(story);
        if (!string.IsNullOrEmpty(save.CurrentSceneId))
            _engine.GoTo(save.CurrentSceneId);

        ApplyStoryTitle(story.Meta?.Title ?? "Interactive Story");
        ShowStoryPlayer();
        UpdateStatus($"Resumed from {save.SavedDate:g}.");
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

    // ── Scene rendering ───────────────────────────────────────────────────────
    private void SetupEventHandlers()
    {
        _engine.SceneChanged          += OnSceneChanged;
        _storyLoader.StoryFileChanged += OnStoryFileChanged;
    }

    private void OnSceneChanged(object? sender, EventArgs e) => UpdateSceneUI();

    private void UpdateSceneUI()
    {
        var scene = _engine.CurrentScene;
        if (scene == null) return;

        var sceneId = _engine.CurrentSceneId;

        _sceneText.Text       = scene.Text;
        _sceneText.FontSize   = _fontSize;
        _sceneText.LineHeight = _fontSize * 1.75;

        if (_sceneLabel        != null) _sceneLabel.Text        = sceneId.ToUpperInvariant();
        _sceneChapterLabel.Text = sceneId.ToUpperInvariant();

        RenderImages(scene);
        UpdateChoices(scene.Choices);
        UpdateStatus($"Scene: {sceneId}");
    }

    // ── Choices ───────────────────────────────────────────────────────────────
    private void UpdateChoices(List<Choice>? choices)
    {
        _choicesPanel.Children.Clear();

        bool isEnding = choices == null || choices.Count == 0;

        if (!isEnding)
        {
            foreach (var choice in choices!)
                _choicesPanel.Children.Add(CreateChoiceButton(choice));
        }
        else
        {
            var line = new Border
            {
                Height          = 1,
                Background      = new SolidColorBrush(Color.Parse("#c9a84c22")),
                Margin          = new Thickness(0, 6, 0, 18)
            };

            var endLabel = new TextBlock
            {
                Text              = "— The End —",
                FontSize          = 15,
                FontWeight        = FontWeight.Light,
                Foreground        = new SolidColorBrush(Color.Parse("#c9a84c")),
                TextAlignment     = TextAlignment.Center,
                LetterSpacing     = 2,
                Margin            = new Thickness(0, 0, 0, 14),
                Opacity           = 0.75
            };

            var replay = new Button { Content = "↩  PLAY AGAIN" };
            replay.Classes.Add("restart");
            replay.HorizontalAlignment        = HorizontalAlignment.Stretch;
            replay.HorizontalContentAlignment = HorizontalAlignment.Center;
            replay.Click += (_, _) =>
            {
                _engine.ResetToStart();
                UpdateStatus("Restarted.");
            };

            _choicesPanel.Children.Add(line);
            _choicesPanel.Children.Add(endLabel);
            _choicesPanel.Children.Add(replay);
        }
    }

    private Button CreateChoiceButton(Choice choice)
    {
        var btn = new Button
        {
            Content                    = $"›   {choice.Text}",
            HorizontalAlignment        = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Tag                        = choice.Next
        };
        btn.Classes.Add("choice");
        btn.Click += OnChoiceClick;
        return btn;
    }

    private void OnChoiceClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string nextId)
            _engine.GoTo(nextId);
    }

    // ── Hot reload ────────────────────────────────────────────────────────────
    private async void OnStoryFileChanged(object? sender, EventArgs e)
    {
        var fp = _storyLoader.GetCurrentFilePath();
        if (fp == null) return;

        var cur   = _engine.CurrentSceneId;
        var story = await _storyLoader.LoadStoryAsync(fp);
        if (story == null) return;

        _engine.Load(story);
        if (!string.IsNullOrEmpty(cur) && story.Scenes.ContainsKey(cur))
            _engine.GoTo(cur);
        UpdateStatus("Story reloaded.");
    }

    private void UpdateStatus(string message)
    {
        if (_statusText != null) _statusText.Text = message;
    }

    protected override void OnClosed(EventArgs e)
    {
        _storyLoader.Dispose();
        base.OnClosed(e);
    }
}
