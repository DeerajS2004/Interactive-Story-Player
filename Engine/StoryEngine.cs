using System;
using System.Collections.Generic;
using InteractiveStoryPlayer.Models;

namespace InteractiveStoryPlayer.Engine;

public class StoryEngine
{
    private StoryRoot? _story;
    private string _currentSceneId = string.Empty;
    private Dictionary<string, object> _state = new();

    public event EventHandler? SceneChanged;

    public Scene? CurrentScene => _story?.Scenes.GetValueOrDefault(_currentSceneId);
    public string CurrentSceneId => _currentSceneId;
    public StoryRoot? Story => _story;
    public Dictionary<string, object> State => _state;

    public bool Load(StoryRoot story)
    {
        try
        {
            _story = story;
            _currentSceneId = story.Start;
            _state.Clear();
            
            SceneChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool GoTo(string sceneId)
    {
        if (_story?.Scenes.ContainsKey(sceneId) == true)
        {
            _currentSceneId = sceneId;
            SceneChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        return false;
    }

    public bool HasValidCurrentScene()
    {
        return _story?.Scenes.ContainsKey(_currentSceneId) == true;
    }

    public void ResetToStart()
    {
        if (_story != null)
        {
            _currentSceneId = _story.Start;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}