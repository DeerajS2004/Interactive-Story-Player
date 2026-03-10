# Interactive Story Player

A cross-platform desktop application for playing branching narrative stories, built with C# and Avalonia UI. Stories are defined entirely in JSON — no recompilation needed to add new stories or change existing ones.

![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Avalonia](https://img.shields.io/badge/Avalonia-11.3.4-teal)
![License](https://img.shields.io/badge/license-MIT-green)

---

## Features

- **JSON-driven stories** — write or swap stories without touching code
- **Branching choices** — multiple choices per scene, each leading to a different path
- **Multiple endings** — scenes with no choices display an ending screen with a replay option
- **Save & load** — progress is saved to `AppData` and can be resumed at any time
- **Multiple images per scene** — each scene supports an `images` array or a single `image` field
- **Adaptive image layout** — portrait images appear on the right, landscape images appear on top, no images means full-width story view
- **Adjustable font size** — increase or decrease story text size with in-app controls
- **Hot reload** — editing the story JSON while the app is open automatically reloads it
- **Dark theme** — fully styled dark UI with hover and press effects on all buttons

---

## Getting Started

### Prerequisites

Install the .NET 8.0 SDK for your operating system:

**Windows**
```powershell
winget install Microsoft.DotNet.SDK.8
```

**macOS**
```bash
brew install --cask dotnet-sdk
```
> If you don't have Homebrew: `/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"`

**Ubuntu / Debian / Pop!_OS**
```bash
sudo apt update && sudo apt install -y dotnet-sdk-8.0
```
> If `dotnet-sdk-8.0` is not found in your repos:
> ```bash
> wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
> sudo dpkg -i packages-microsoft-prod.deb
> sudo apt update && sudo apt install -y dotnet-sdk-8.0
> ```

**Fedora / RHEL**
```bash
sudo dnf install dotnet-sdk-8.0
```

**Arch Linux**
```bash
sudo pacman -S dotnet-sdk
```

**Verify installation**
```bash
dotnet --version
# should print 8.x.x
```

> **Linux only** — Avalonia requires this font package:
> ```bash
> sudo apt install -y libfontconfig1   # Debian/Ubuntu/Pop!_OS
> sudo dnf install -y fontconfig       # Fedora
> sudo pacman -S fontconfig            # Arch
> ```

---

### Run

```bash
git clone https://github.com/DeerajS2004/Interactive-Story-Player.git
cd Interactive-Story-Player
dotnet run
```

### Build

```bash
dotnet build -c Release
```

---

## Project Structure

```
InteractiveStoryPlayer/
├── Assets/
│   ├── story.json          # Default story file
│   └── images/             # Scene images (referenced in story.json)
├── Engine/
│   └── StoryEngine.cs      # Core engine: scene state, navigation, events
├── Models/
│   ├── Scene.cs            # Scene data model
│   ├── Choice.cs           # Choice data model
│   └── StoryRoot.cs        # Story root + metadata model
├── Services/
│   ├── StoryLoader.cs      # JSON loader with file watcher (hot reload)
│   └── SaveSystem.cs       # Save/load progress to AppData
└── UI/
    ├── MainWindow.axaml     # UI layout and styles
    └── MainWindow.axaml.cs  # UI logic and event handling
```

---

## Included Story — The Last Blade of Ashikaga

> *Kyoto, 1467. The Onin War has torn the capital apart. Your lord is dead, his sacred sword stolen. You are the last samurai of your clan — and the last one who can set things right.*

24 scenes, 5 distinct endings, multiple branching paths. The flowchart below maps every route through the story.

```mermaid
flowchart TD
    START([🗡️ START<br/>Kyoto Burns]) --> prologue

    prologue["**PROLOGUE**<br/>Ruins of the Ashikaga estate.<br/>Kurogane is stolen."]

    prologue -->|Head to merchant district| merchant_district
    prologue -->|Go straight north| northern_road

    merchant_district["**MERCHANT DISTRICT**<br/>Ferryman has information.<br/>Yamana soldiers appear."]
    merchant_district -->|Slip away quietly| ryoanji_temple
    merchant_district -->|Stay hidden and listen| eavesdrop
    merchant_district -->|Confront the soldiers| confront_soldiers

    eavesdrop["**EAVESDROP**<br/>You learn: mountain fortress,<br/>Lord Okita, 40 men."]
    eavesdrop -->|Go to Ryoan-ji| ryoanji_temple
    eavesdrop -->|Go directly to the pass| kurama_pass

    confront_soldiers["**CONFRONT SOLDIERS**<br/>Fight in the market.<br/>You win but are exposed."]
    confront_soldiers -->|Rush to Ryoan-ji| ryoanji_temple
    confront_soldiers -->|Go straight to the pass| kurama_pass

    northern_road["**NORTHERN ROAD**<br/>Yamana scouts camped<br/>in the cedar forest."]
    northern_road -->|Confront scouts openly| scouts_confrontation
    northern_road -->|Steal the map| steal_map
    northern_road -->|Follow them| follow_scouts

    scouts_confrontation["**SCOUTS CONFRONTATION**<br/>Two scouts fight.<br/>Third gives up the location."]
    scouts_confrontation -->|Go to Kurama pass| kurama_pass
    scouts_confrontation -->|Go to Ryoan-ji first| ryoanji_temple

    steal_map["**STEAL THE MAP**<br/>Map shows a secret<br/>cliff entry to the fortress."]
    steal_map -->|Take the cliff path| cliff_path
    steal_map -->|Go to the main gate| kurama_pass

    follow_scouts["**FOLLOW SCOUTS**<br/>Scouts lead you to the garrison.<br/>You spot the cliff supply route."]
    follow_scouts -->|Take the cliff path| cliff_path
    follow_scouts -->|Issue formal challenge at gate| gate_challenge

    ryoanji_temple["**RYOAN-JI TEMPLE**<br/>Old monk knows everything.<br/>Offers a map for an oath."]
    ryoanji_temple -->|Swear the oath — take map| cliff_path
    ryoanji_temple -->|Accept map, no promise| kurama_pass
    ryoanji_temple -->|Decline map| kurama_pass

    kurama_pass["**KURAMA PASS**<br/>Fortress on the ridge.<br/>40 guards visible."]
    kurama_pass -->|Issue formal challenge| gate_challenge
    kurama_pass -->|Look for another way| cliff_path

    cliff_path["**CLIFF PATH**<br/>Scale the rock face at night.<br/>You see Kurogane inside.<br/>A man reads beside it."]
    cliff_path -->|Take sword while he reads| stealth_retrieval
    cliff_path -->|Observe the man first| observe_okita

    gate_challenge["**GATE CHALLENGE**<br/>You issue a formal challenge.<br/>Lord Okita answers the gate himself."]
    gate_challenge -->|Accept his hospitality — wait for dawn| the_duel_eve
    gate_challenge -->|Demand the sword immediately| tense_standoff

    observe_okita["**OBSERVE OKITA**<br/>His journal reads:<br/>'I hope he is the honourable kind.'<br/>He left the door open."]
    observe_okita -->|Enter through the open door| the_conversation
    observe_okita -->|Take the sword and go| stealth_retrieval

    stealth_retrieval["**STEALTH RETRIEVAL**<br/>You grab Kurogane.<br/>Okita speaks without alarm:<br/>'The sword is yours. But listen.'"]
    stealth_retrieval -->|Hear him out| the_revelation
    stealth_retrieval -->|Leave now| ENDING_SWORD

    the_conversation["**THE CONVERSATION**<br/>Okita explains Commander Ryoichi<br/>acted without his orders.<br/>He slides the sword to you."]
    the_conversation -->|Listen to what he has to say| the_revelation
    the_conversation -->|Take sword and leave| ENDING_SWORD

    the_duel_eve["**EVE OF THE DUEL**<br/>At midnight Kurogane is<br/>delivered to your room.<br/>Gate is unguarded at dawn."]
    the_duel_eve -->|Leave before anyone rises| ENDING_QUIET
    the_duel_eve -->|Stay for the duel| the_duel

    tense_standoff["**TENSE STANDOFF**<br/>40 soldiers watching.<br/>Okita orders the sword brought.<br/>He offers to explain."]
    tense_standoff -->|Give him two minutes| the_revelation
    tense_standoff -->|Leave now| ENDING_SWORD

    the_duel["**THE DUEL**<br/>Dawn. Stone courtyard.<br/>Okita is exceptional but yields.<br/>He invites you to breakfast."]
    the_duel -->|Accept — hear him out| the_revelation

    the_revelation["**THE REVELATION**<br/>The theft was ordered by<br/>Tanaka Goro — an inside job.<br/>Okita has the letters to prove it."]
    the_revelation -->|Confront Tanaka Goro in Kyoto| ENDING_JUSTICE
    the_revelation -->|Take documents to Ashikaga elders| ENDING_HONOUR
    the_revelation -->|Go to Ryoan-ji as sworn| ENDING_RELEASE

    ENDING_JUSTICE(["⚖️ ENDING: JUSTICE<br/>Tanaka is tried and exiled.<br/>You walk out of Kyoto honourably."])
    ENDING_HONOUR(["🏯 ENDING: HONOUR RESTORED<br/>Elders receive the truth.<br/>Kurogane hangs in the monastery."])
    ENDING_RELEASE(["🪷 ENDING: RELEASE<br/>You kneel and give the sword<br/>to the monk. You leave with nothing<br/>but strange lightness."])
    ENDING_SWORD(["🌑 ENDING: THE SWORD ONLY<br/>You walk north into the mountains.<br/>The truth stays buried.<br/>You don't look back."])
    ENDING_QUIET(["🌫️ ENDING: QUIET DEPARTURE<br/>You leave before anyone wakes.<br/>Okita lets you go.<br/>You'll never know what he wanted to say."])

    classDef scene   fill:#13131a,stroke:#2a2a3a,color:#c4c4d4
    classDef hub     fill:#0e1a0e,stroke:#2a4a2a,color:#9acc9a
    classDef entry   fill:#0e0e1a,stroke:#2a2a5a,color:#9a9acc
    classDef good    fill:#0e1a14,stroke:#c9a84c,color:#c9a84c
    classDef neutral fill:#1a140e,stroke:#c9a84c,color:#c9a84c
    classDef dark    fill:#1a0e0e,stroke:#6a3a3a,color:#aa7070

    class prologue,merchant_district,northern_road,eavesdrop,confront_soldiers scene
    class scouts_confrontation,steal_map,follow_scouts,kurama_pass scene
    class observe_okita,stealth_retrieval,the_conversation,tense_standoff,the_duel_eve,the_duel,the_revelation scene
    class ryoanji_temple hub
    class cliff_path,gate_challenge entry
    class ENDING_JUSTICE,ENDING_HONOUR,ENDING_RELEASE good
    class ENDING_QUIET neutral
    class ENDING_SWORD dark
```

---

## Story JSON Format

Stories are plain JSON files. Place them anywhere and load via the app, or replace `Assets/story.json` as the default.

```json
{
  "meta": {
    "title": "My Story",
    "version": "1.0",
    "author": "Your Name"
  },
  "start": "scene_id",
  "scenes": {
    "scene_id": {
      "text": "The scene description shown to the player.",
      "images": ["images/scene.jpg"],
      "choices": [
        { "text": "Choice label", "next": "another_scene_id" },
        { "text": "Another choice", "next": "ending_scene_id" }
      ]
    },
    "ending_scene_id": {
      "text": "The story ends here.",
      "images": ["images/ending.jpg"],
      "choices": []
    }
  }
}
```

**Notes:**
- `start` — the ID of the first scene to display
- `images` — array of paths relative to the story JSON file's directory; falls back to a single `image` field for compatibility
- `choices: []` — an empty choices array marks a scene as an ending; the app shows a "Play Again" button automatically
- Portrait images (taller than wide) appear in a right-side panel; landscape images appear as a top banner; missing images hide the panel entirely

---

## Save File Location

| OS | Path |
|----|------|
| Windows | `%APPDATA%\InteractiveStoryPlayer\save.json` |
| Linux | `~/.config/InteractiveStoryPlayer/save.json` |
| macOS | `~/Library/Application Support/InteractiveStoryPlayer/save.json` |

---

## Built With

- [C#](https://learn.microsoft.com/en-us/dotnet/csharp/) — .NET 8.0
- [Avalonia UI](https://avaloniaui.net/) — 11.3.4, cross-platform UI framework

---

## License

MIT — see [LICENSE](LICENSE) for details.
