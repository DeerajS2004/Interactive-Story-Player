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
    private readonly StoryEngine  _engine;
    private readonly StoryLoader  _storyLoader;
    private readonly SaveSystem   _saveSystem;

    // UI refs
    private Grid?       _mainMenuGrid;
    private Grid?       _storyPlayerGrid;
    private TextBlock?  _menuTitleText;
    private TextBlock?  _topBarTitle;
    private TextBlock?  _sceneLabel;
    private TextBlock?  _sceneChapterLabel;
    private TextBlock?  _sceneText;
    private TextBlock?  _fontSizeLabel;
    private StackPanel? _imagesPanel;
    private StackPanel? _choicesPanel;
    private TextBlock?  _statusText;

    // Font size state
    private int _fontSize     = 17;
    private const int FontMin = 12;
    private const int FontMax = 28;

    private readonly string _defaultStoryPath;

    public MainWindow()
    {
        InitializeComponent();

        _engine      = new StoryEngine();
        _storyLoader = new StoryLoader();
        _saveSystem  = new SaveSystem();

        _defaultStoryPath = Path.Combine(
            Directory.GetCurrentDirectory(), "Assets", "story.json");

        InitializeControls();
        SetupEventHandlers();
    }

    private void InitializeControls()
    {
        _mainMenuGrid      = this.FindControl<Grid>("MainMenuGrid");
        _storyPlayerGrid   = this.FindControl<Grid>("StoryPlayerGrid");
        _menuTitleText     = this.FindControl<TextBlock>("MenuTitleText");
        _topBarTitle       = this.FindControl<TextBlock>("TopBarTitle");
        _sceneLabel        = this.FindControl<TextBlock>("SceneLabel");
        _sceneChapterLabel = this.FindControl<TextBlock>("SceneChapterLabel");
        _sceneText         = this.FindControl<TextBlock>("SceneText");
        _fontSizeLabel     = this.FindControl<TextBlock>("FontSizeLabel");
        _imagesPanel       = this.FindControl<StackPanel>("ImagesPanel");
        _choicesPanel      = this.FindControl<StackPanel>("ChoicesPanel");
        _statusText        = this.FindControl<TextBlock>("StatusText");
    }

    private void SetupEventHandlers()
    {
        _engine.SceneChanged          += OnSceneChanged;
        _storyLoader.StoryFileChanged += OnStoryFileChanged;
    }

    // ── Font Size ─────────────────────────────────────────────────────────────

    private void OnFontDecClick(object? sender, RoutedEventArgs e) =>
        SetFontSize(_fontSize - 1);

    private void OnFontIncClick(object? sender, RoutedEventArgs e) =>
        SetFontSize(_fontSize + 1);

    private void SetFontSize(int size)
    {
        _fontSize = Math.Clamp(size, FontMin, FontMax);
        if (_sceneText    != null) _sceneText.FontSize    = _fontSize;
        if (_fontSizeLabel != null) _fontSizeLabel.Text   = _fontSize.ToString();
        // Adjust line height proportionally
        if (_sceneText    != null) _sceneText.LineHeight  = _fontSize * 1.75;
    }

    // ── Menu Actions ──────────────────────────────────────────────────────────

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

    // ── Scene Rendering ───────────────────────────────────────────────────────

    private void OnSceneChanged(object? sender, EventArgs e) => UpdateSceneUI();

    private void UpdateSceneUI()
    {
        var scene = _engine.CurrentScene;
        if (scene == null) return;

        var sceneId = _engine.CurrentSceneId;

        if (_sceneText != null)
        {
            _sceneText.Text     = scene.Text;
            _sceneText.FontSize = _fontSize;
            _sceneText.LineHeight = _fontSize * 1.75;
        }

        if (_sceneLabel        != null) _sceneLabel.Text        = sceneId.ToUpperInvariant();
        if (_sceneChapterLabel != null) _sceneChapterLabel.Text = sceneId.ToUpperInvariant();

        UpdateSceneImages(scene);
        UpdateChoices(scene.Choices);
        UpdateStatus($"Scene: {sceneId}");
    }

    private void UpdateSceneImages(Scene scene)
    {
        if (_imagesPanel == null) return;
        _imagesPanel.Children.Clear();

        var list = new List<string>();
        if (scene.Images?.Count > 0) list.AddRange(scene.Images);
        else if (!string.IsNullOrEmpty(scene.Image)) list.Add(scene.Image);

        if (list.Count == 0) return;

        foreach (var rel in list)
        {
            try
            {
                var dir      = _storyLoader.GetStoryDirectory();
                var fullPath = dir != null ? Path.Combine(dir, rel) : rel;
                if (!File.Exists(fullPath)) continue;

                var img = new Image
                {
                    Source    = new Bitmap(fullPath),
                    Stretch   = Avalonia.Media.Stretch.UniformToFill,
                    Height    = list.Count == 1 ? 320 : 220,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
                };

                // Subtle label overlay at bottom
                var overlay = new Border
                {
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                    Height            = 40,
                    Background        = new LinearGradientBrush
                    {
                        StartPoint = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Relative),
                        EndPoint   = new Avalonia.RelativePoint(0, 1, Avalonia.RelativeUnit.Relative),
                        GradientStops =
                        {
                            new GradientStop(Color.Parse("#00000000"), 0),
                            new GradientStop(Color.Parse("#CC000000"), 1)
                        }
                    }
                };

                var stack = new Panel();
                stack.Children.Add(img);
                stack.Children.Add(overlay);

                var card = new Border
                {
                    Child        = stack,
                    ClipToBounds = true,
                    BoxShadow    = BoxShadows.Parse("0 2 20 0 #000000")
                };

                _imagesPanel.Children.Add(card);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image error [{rel}]: {ex.Message}");
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
            var line = new Border
            {
                Height     = 1,
                Background = new SolidColorBrush(Color.Parse("#c9a84c22")),
                Margin     = new Avalonia.Thickness(0, 6, 0, 18)
            };

            var end = new TextBlock
            {
                Text          = "— The End —",
                FontSize      = 15,
                FontWeight    = Avalonia.Media.FontWeight.Light,
                Foreground    = new SolidColorBrush(Color.Parse("#c9a84c")),
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                LetterSpacing = 2,
                Margin        = new Avalonia.Thickness(0, 0, 0, 14),
                Opacity       = 0.75
            };

            var replay = new Button
            {
                Content = "↩  PLAY AGAIN",
                Classes = { "restart" },
                HorizontalAlignment       = Avalonia.Layout.HorizontalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            replay.Click += (_, _) =>
            {
                _engine.ResetToStart();
                UpdateStatus("Restarted.");
            };

            _choicesPanel.Children.Add(line);
            _choicesPanel.Children.Add(end);
            _choicesPanel.Children.Add(replay);
        }
    }

    private Button CreateChoiceButton(Choice choice)
    {
        var btn = new Button
        {
            Content = $"›   {choice.Text}",
            Classes = { "choice" },
            HorizontalAlignment        = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Tag = choice.Next
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
        var fp  = _storyLoader.GetCurrentFilePath();
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
