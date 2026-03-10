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

    private Grid? _mainMenuGrid;
    private Grid? _storyPlayerGrid;
    private TextBlock? _sceneText;
    private Image? _backgroundImage;
    private StackPanel? _imagesPanel;
    private StackPanel? _choicesPanel;
    private TextBlock? _statusText;
    private TextBlock? _titleText;

    private readonly string _defaultStoryPath;

    public MainWindow()
    {
        InitializeComponent();

        _engine = new StoryEngine();
        _storyLoader = new StoryLoader();
        _saveSystem = new SaveSystem();

        _defaultStoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "story.json");

        InitializeControls();
        SetupEventHandlers();
        LoadBackgroundImage();

        UpdateStatus("Welcome! Start a new story or load your progress.");
    }

    private void InitializeControls()
    {
        _mainMenuGrid = this.FindControl<Grid>("MainMenuGrid");
        _storyPlayerGrid = this.FindControl<Grid>("StoryPlayerGrid");
        _sceneText = this.FindControl<TextBlock>("SceneText");
        _backgroundImage = this.FindControl<Image>("BackgroundImage");
        _imagesPanel = this.FindControl<StackPanel>("ImagesPanel");
        _choicesPanel = this.FindControl<StackPanel>("ChoicesPanel");
        _statusText = this.FindControl<TextBlock>("StatusText");
        _titleText = this.FindControl<TextBlock>("TitleText");
    }

    private void SetupEventHandlers()
    {
        _engine.SceneChanged += OnSceneChanged;
        _storyLoader.StoryFileChanged += OnStoryFileChanged;
    }

    private void LoadBackgroundImage()
    {
        try
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "images", "forest_edge.jpg");
            if (File.Exists(imagePath) && _backgroundImage != null)
                _backgroundImage.Source = new Bitmap(imagePath);
        }
        catch { /* Background image is optional */ }
    }

    private async void OnStartNewStoryClick(object? sender, RoutedEventArgs e)
    {
        if (!File.Exists(_defaultStoryPath))
        {
            UpdateStatus("Story file not found.");
            return;
        }

        var story = await _storyLoader.LoadStoryAsync(_defaultStoryPath);
        if (story != null)
        {
            _engine.Load(story);
            if (_titleText != null)
                _titleText.Text = story.Meta?.Title ?? "Interactive Story";
            ShowStoryPlayer();
            UpdateStatus("New story started.");
        }
        else
        {
            UpdateStatus("Failed to load story.");
        }
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
        if (story != null)
        {
            _engine.Load(story);
            if (!string.IsNullOrEmpty(saveData.CurrentSceneId))
                _engine.GoTo(saveData.CurrentSceneId);

            if (_titleText != null)
                _titleText.Text = story.Meta?.Title ?? "Interactive Story";

            ShowStoryPlayer();
            UpdateStatus($"Loaded from: {saveData.SavedDate:g}");
        }
        else
        {
            UpdateStatus("Failed to load story file.");
        }
    }

    private void OnExitClick(object? sender, RoutedEventArgs e) => Close();

    private void OnBackToMenuClick(object? sender, RoutedEventArgs e)
    {
        ShowMainMenu();
        UpdateStatus("Back to main menu.");
    }

    private async void OnSaveProgressClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_engine.CurrentSceneId))
        {
            UpdateStatus("Nothing to save.");
            return;
        }

        await _saveSystem.SaveAsync(_defaultStoryPath, _engine.CurrentSceneId);
        UpdateStatus($"Progress saved. ({DateTime.Now:t})");
    }

    private void ShowMainMenu()
    {
        if (_mainMenuGrid != null) _mainMenuGrid.IsVisible = true;
        if (_storyPlayerGrid != null) _storyPlayerGrid.IsVisible = false;
    }

    private void ShowStoryPlayer()
    {
        if (_mainMenuGrid != null) _mainMenuGrid.IsVisible = false;
        if (_storyPlayerGrid != null) _storyPlayerGrid.IsVisible = true;
    }

    private void OnSceneChanged(object? sender, EventArgs e) => UpdateSceneUI();

    private void UpdateSceneUI()
    {
        var scene = _engine.CurrentScene;
        if (scene == null) return;

        if (_sceneText != null)
            _sceneText.Text = scene.Text;

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
                var fullPath = storyDir != null ? Path.Combine(storyDir, imagePath) : imagePath;
                if (!File.Exists(fullPath)) continue;

                var imageControl = new Image
                {
                    Source = new Bitmap(fullPath),
                    Width = 300,
                    MaxHeight = 250,
                    Stretch = Avalonia.Media.Stretch.UniformToFill,
                    Margin = new Avalonia.Thickness(0, 5),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                var border = new Border
                {
                    Child = imageControl,
                    Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                    CornerRadius = new Avalonia.CornerRadius(12),
                    Padding = new Avalonia.Thickness(5),
                    ClipToBounds = true,
                    BoxShadow = BoxShadows.Parse("0 4 12 0 #80000000")
                };

                _imagesPanel.Children.Add(border);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image {imagePath}: {ex.Message}");
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
            // Ending screen: show a "The End" label and a restart button
            var endLabel = new TextBlock
            {
                Text = "— The End —",
                FontSize = 20,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 150, 90)),
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                Margin = new Avalonia.Thickness(0, 10, 0, 10)
            };

            var restartButton = new Button
            {
                Content = "↩ Play Again",
                Classes = { "action" },
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Height = 55,
                Margin = new Avalonia.Thickness(0, 5),
                CornerRadius = new Avalonia.CornerRadius(8)
            };
            restartButton.Click += (_, _) =>
            {
                _engine.ResetToStart();
                UpdateStatus("Story restarted.");
            };

            _choicesPanel.Children.Add(endLabel);
            _choicesPanel.Children.Add(restartButton);
        }
    }

    private Button CreateChoiceButton(Choice choice)
    {
        var button = new Button
        {
            Content = choice.Text,
            Classes = { "choice" },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Padding = new Avalonia.Thickness(20, 0),
            CornerRadius = new Avalonia.CornerRadius(8),
            Tag = choice.Next
        };
        button.Click += OnChoiceClick;
        return button;
    }

    private void OnChoiceClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string nextSceneId)
            _engine.GoTo(nextSceneId);
    }

    private async void OnStoryFileChanged(object? sender, EventArgs e)
    {
        var filePath = _storyLoader.GetCurrentFilePath();
        if (filePath == null) return;

        var currentSceneId = _engine.CurrentSceneId;
        var story = await _storyLoader.LoadStoryAsync(filePath);

        if (story != null)
        {
            _engine.Load(story);
            if (!string.IsNullOrEmpty(currentSceneId) && story.Scenes.ContainsKey(currentSceneId))
                _engine.GoTo(currentSceneId);
            UpdateStatus("Story file reloaded.");
        }
    }

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
