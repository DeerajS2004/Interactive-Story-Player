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

    // UI Controls
    private Grid? _mainMenuGrid;
    private Grid? _storyPlayerGrid;
    private TextBlock? _sceneText;
    private Image? _backgroundImage;
    private StackPanel? _imagesPanel; // Changed to support multiple images
    private StackPanel? _choicesPanel;
    private TextBlock? _statusText;

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

        AttachEventHandlers();

        UpdateStatus("Welcome to The Enchanted Forest");
    }

    private void AttachEventHandlers()
{
    // Manually wire up all button click events
    var startButton = this.FindControl<Button>("StartNewStoryButton");
    var loadButton = this.FindControl<Button>("LoadProgressButton");
    var exitButton = this.FindControl<Button>("ExitGameButton");
    var backToMenuButton = this.FindControl<Button>("BackToMenuButton");
    var saveProgressButton = this.FindControl<Button>("SaveProgressButton");

    if (startButton != null)
        startButton.Click += OnStartNewStoryClick;
    
    if (loadButton != null)
        loadButton.Click += OnLoadProgressClick;
        
    if (exitButton != null)
        exitButton.Click += OnExitClick;
        
    if (backToMenuButton != null)
        backToMenuButton.Click += OnBackToMenuClick;
        
    if (saveProgressButton != null)
        saveProgressButton.Click += OnSaveProgressClick;
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
            {
                _backgroundImage.Source = new Bitmap(imagePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading background image: {ex.Message}");
        }
    }

    private async void OnStartNewStoryClick(object? sender, RoutedEventArgs e)
{
    Console.WriteLine("Start New Story clicked"); // Debug
    
    if (File.Exists(_defaultStoryPath))
    {
        var story = await _storyLoader.LoadStoryAsync(_defaultStoryPath);
        if (story != null)
        {
            _engine.Load(story);
            // Start fresh - don't load any saved progress
            ShowStoryPlayer();
            ForceUIRefresh(); // Force UI update
            UpdateStatus("New story started!");
        }
    }
    else
    {
        UpdateStatus("Story file not found");
    }
}


private void ForceUIRefresh()
{
    // Force layout update
    this.InvalidateVisual();
    this.UpdateLayout();
    
    // Update scene display
    UpdateSceneUI();
}




    private async Task StartNewStory()
    {
        if (File.Exists(_defaultStoryPath))
        {
            var story = await _storyLoader.LoadStoryAsync(_defaultStoryPath);
            if (story != null)
            {
                _engine.Load(story);
                ShowStoryPlayer();
                UpdateStatus("Story started!");
            }
            else
            {
                UpdateStatus("Failed to load story");
            }
        }
        else
        {
            UpdateStatus("Default story not found");
        }
    }

   private async void OnLoadProgressClick(object? sender, RoutedEventArgs e)
{
    Console.WriteLine("Load Progress clicked"); // Debug
    
    var saveData = await _saveSystem.LoadAsync();
    if (saveData?.StoryFilePath != null && File.Exists(saveData.StoryFilePath))
    {
        var story = await _storyLoader.LoadStoryAsync(saveData.StoryFilePath);
        if (story != null)
        {
            _engine.Load(story);
            
            // CRITICAL: Navigate to saved scene, not start scene
            if (!string.IsNullOrEmpty(saveData.CurrentSceneId))
            {
                _engine.GoTo(saveData.CurrentSceneId);
                Console.WriteLine($"Loaded scene: {saveData.CurrentSceneId}");
            }
            
            ShowStoryPlayer();
            ForceUIRefresh(); // Force UI update
            UpdateStatus($"Loaded from: {saveData.CurrentSceneId}");
        }
    }
    else
    {
        UpdateStatus("No saved progress found");
    }
}



    private async Task LoadProgress()
    {
        var saveData = await _saveSystem.LoadAsync();
        if (saveData?.StoryFilePath != null && File.Exists(saveData.StoryFilePath))
        {
            var story = await _storyLoader.LoadStoryAsync(saveData.StoryFilePath);
            if (story != null)
            {
                _engine.Load(story);
                if (saveData.CurrentSceneId != null)
                {
                    _engine.GoTo(saveData.CurrentSceneId);
                }
                ShowStoryPlayer();
                UpdateStatus("Progress loaded!");
            }
        }
        else
        {
            UpdateStatus("No saved progress found");
        }
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnBackToMenuClick(object? sender, RoutedEventArgs e)
    {
        ShowMainMenu();
        UpdateStatus("Back to main menu");
    }

    private async void OnSaveProgressClick(object? sender, RoutedEventArgs e)
{
    Console.WriteLine($"Save Progress clicked - Current scene: {_engine.CurrentSceneId}"); // Debug
    
    if (!string.IsNullOrEmpty(_defaultStoryPath))
    {
        await _saveSystem.SaveAsync(_defaultStoryPath, _engine.CurrentSceneId);
        UpdateStatus($"Progress saved at scene: {_engine.CurrentSceneId}");
    }
    else
    {
        UpdateStatus("Cannot save - no story loaded");
    }
}

    private async Task SaveProgress()
    {
        if (!string.IsNullOrEmpty(_defaultStoryPath))
        {
            await _saveSystem.SaveAsync(_defaultStoryPath, _engine.CurrentSceneId);
            UpdateStatus("Progress saved!");
        }
    }

    private void ShowMainMenu()
    {
        if (_mainMenuGrid != null && _storyPlayerGrid != null)
        {
            _storyPlayerGrid.IsVisible = false;
            _mainMenuGrid.IsVisible = true;
        }
    }

    private void ShowStoryPlayer()
    {
        if (_mainMenuGrid != null && _storyPlayerGrid != null)
        {
            _mainMenuGrid.IsVisible = false;
            _storyPlayerGrid.IsVisible = true;
        }
    }

    private void OnSceneChanged(object? sender, EventArgs e)
    {
        UpdateSceneUI();
    }

   private void UpdateSceneUI()
{
    var scene = _engine.CurrentScene;
    if (scene == null) 
    {
        Console.WriteLine("UpdateSceneUI: No current scene");
        return;
    }

    Console.WriteLine($"UpdateSceneUI: Loading scene with text: {scene.Text.Substring(0, Math.Min(50, scene.Text.Length))}...");
    
    // Update text
    if (_sceneText != null)
    {
        _sceneText.Text = scene.Text;
    }
        // Update multiple images
        UpdateSceneImages(scene);

        // Update choices
        UpdateChoices(scene.Choices);
        
        UpdateStatus($"Scene: {_engine.CurrentSceneId}");
    }

   private void UpdateSceneImages(Scene scene)
{
    if (_imagesPanel == null) return;

    _imagesPanel.Children.Clear();

    // Handle multiple images
    var imagesToShow = new List<string>();
    
    // Check for new Images list first
    if (scene.Images != null && scene.Images.Count > 0)
    {
        imagesToShow.AddRange(scene.Images);
    }
    // Fall back to single Image for backward compatibility
    else if (!string.IsNullOrEmpty(scene.Image))
    {
        imagesToShow.Add(scene.Image);
    }

    // Create and add image controls
    foreach (var imagePath in imagesToShow)
    {
        try
        {
            var storyDir = _storyLoader.GetStoryDirectory();
            var fullImagePath = storyDir != null ? Path.Combine(storyDir, imagePath) : imagePath;
            
            if (File.Exists(fullImagePath))
            {
                var imageControl = new Image
                {
                    Source = new Bitmap(fullImagePath),
                    Width = 300,
                    MaxHeight = 250,
                    Stretch = Avalonia.Media.Stretch.UniformToFill,
                    Margin = new Avalonia.Thickness(0, 5),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                var border = new Border
                {
                    Child = imageControl,
                    Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)), // Dark background
                    CornerRadius = new Avalonia.CornerRadius(12),
                    Padding = new Avalonia.Thickness(5),
                    ClipToBounds = true
                };

                // Dark theme shadow
                border.BoxShadow = BoxShadows.Parse("0 4 12 0 #80000000");

                _imagesPanel.Children.Add(border);
            }
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

        if (choices != null)
        {
            foreach (var choice in choices)
            {
                var button = CreateChoiceButton(choice);
                _choicesPanel.Children.Add(button);
            }
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
        {
            _engine.GoTo(nextSceneId);
        }
    }

    private async void OnStoryFileChanged(object? sender, EventArgs e)
    {
        var filePath = _storyLoader.GetCurrentFilePath();
        if (filePath != null)
        {
            var currentSceneId = _engine.CurrentSceneId;
            var story = await _storyLoader.LoadStoryAsync(filePath);
            
            if (story != null)
            {
                _engine.Load(story);
                
                if (!string.IsNullOrEmpty(currentSceneId) && story.Scenes.ContainsKey(currentSceneId))
                {
                    _engine.GoTo(currentSceneId);
                }
                UpdateStatus("Story file updated!");
            }
        }
    }

    private void UpdateStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.Text = message;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _storyLoader.Dispose();
        base.OnClosed(e);
    }
}
