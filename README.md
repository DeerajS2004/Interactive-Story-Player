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
    START([🗡️ START\nKyoto Burns]) --> prologue

    prologue["**PROLOGUE**\nRuins of the Ashikaga estate.\nKurogane is stolen."]

    prologue -->|Head to merchant district| merchant_district
    prologue -->|Go straight north| northern_road

    merchant_district["**MERCHANT DISTRICT**\nFerryman has information.\nYamana soldiers appear."]
    merchant_district -->|Slip away quietly| ryoanji_temple
    merchant_district -->|Stay hidden and listen| eavesdrop
    merchant_district -->|Confront the soldiers| confront_soldiers

    eavesdrop["**EAVESDROP**\nYou learn: mountain fortress,\nLord Okita, 40 men."]
    eavesdrop -->|Go to Ryoan-ji| ryoanji_temple
    eavesdrop -->|Go directly to the pass| kurama_pass

    confront_soldiers["**CONFRONT SOLDIERS**\nFight in the market.\nYou win but are exposed."]
    confront_soldiers -->|Rush to Ryoan-ji| ryoanji_temple
    confront_soldiers -->|Go straight to the pass| kurama_pass

    northern_road["**NORTHERN ROAD**\nYamana scouts camped\nin the cedar forest."]
    northern_road -->|Confront scouts openly| scouts_confrontation
    northern_road -->|Steal the map| steal_map
    northern_road -->|Follow them| follow_scouts

    scouts_confrontation["**SCOUTS CONFRONTATION**\nTwo scouts fight.\nThird gives up the location."]
    scouts_confrontation -->|Go to Kurama pass| kurama_pass
    scouts_confrontation -->|Go to Ryoan-ji first| ryoanji_temple

    steal_map["**STEAL THE MAP**\nMap shows a secret\ncliff entry to the fortress."]
    steal_map -->|Take the cliff path| cliff_path
    steal_map -->|Go to the main gate| kurama_pass

    follow_scouts["**FOLLOW SCOUTS**\nScouts lead you to the garrison.\nYou spot the cliff supply route."]
    follow_scouts -->|Take the cliff path| cliff_path
    follow_scouts -->|Issue formal challenge at gate| gate_challenge

    ryoanji_temple["**RYOAN-JI TEMPLE**\nOld monk knows everything.\nOffers a map for an oath."]
    ryoanji_temple -->|Swear the oath — take map| cliff_path
    ryoanji_temple -->|Accept map, no promise| kurama_pass
    ryoanji_temple -->|Decline map| kurama_pass

    kurama_pass["**KURAMA PASS**\nFortress on the ridge.\n40 guards visible."]
    kurama_pass -->|Issue formal challenge| gate_challenge
    kurama_pass -->|Look for another way| cliff_path

    cliff_path["**CLIFF PATH**\nScale the rock face at night.\nYou see Kurogane inside.\nA man reads beside it."]
    cliff_path -->|Take sword while he reads| stealth_retrieval
    cliff_path -->|Observe the man first| observe_okita

    gate_challenge["**GATE CHALLENGE**\nYou issue a formal challenge.\nLord Okita answers the gate himself."]
    gate_challenge -->|Accept his hospitality — wait for dawn| the_duel_eve
    gate_challenge -->|Demand the sword immediately| tense_standoff

    observe_okita["**OBSERVE OKITA**\nHis journal reads:\n'I hope he is the honourable kind.'\nHe left the door open."]
    observe_okita -->|Enter through the open door| the_conversation
    observe_okita -->|Take the sword and go| stealth_retrieval

    stealth_retrieval["**STEALTH RETRIEVAL**\nYou grab Kurogane.\nOkita speaks without alarm:\n'The sword is yours. But listen.'"]
    stealth_retrieval -->|Hear him out| the_revelation
    stealth_retrieval -->|Leave now| ENDING_SWORD

    the_conversation["**THE CONVERSATION**\nOkita explains Commander Ryoichi\nacted without his orders.\nHe slides the sword to you."]
    the_conversation -->|Listen to what he has to say| the_revelation
    the_conversation -->|Take sword and leave| ENDING_SWORD

    the_duel_eve["**EVE OF THE DUEL**\nAt midnight Kurogane is\ndelivered to your room.\nGate is unguarded at dawn."]
    the_duel_eve -->|Leave before anyone rises| ENDING_QUIET
    the_duel_eve -->|Stay for the duel| the_duel

    tense_standoff["**TENSE STANDOFF**\n40 soldiers watching.\nOkita orders the sword brought.\nHe offers to explain."]
    tense_standoff -->|Give him two minutes| the_revelation
    tense_standoff -->|Leave now| ENDING_SWORD

    the_duel["**THE DUEL**\nDawn. Stone courtyard.\nOkita is exceptional but yields.\nHe invites you to breakfast."]
    the_duel -->|Accept — hear him out| the_revelation

    the_revelation["**THE REVELATION**\nThe theft was ordered by\nTanaka Goro — an inside job.\nOkita has the letters to prove it."]
    the_revelation -->|Confront Tanaka Goro in Kyoto| ENDING_JUSTICE
    the_revelation -->|Take documents to Ashikaga elders| ENDING_HONOUR
    the_revelation -->|Go to Ryoan-ji as sworn| ENDING_RELEASE

    ENDING_JUSTICE(["⚖️ ENDING: JUSTICE\nTanaka is tried and exiled.\nYou walk out of Kyoto honourably."])
    ENDING_HONOUR(["🏯 ENDING: HONOUR RESTORED\nElders receive the truth.\nKurogane hangs in the monastery."])
    ENDING_RELEASE(["🪷 ENDING: RELEASE\nYou kneel and give the sword\nto the monk. You leave with nothing\nbut strange lightness."])
    ENDING_SWORD(["🌑 ENDING: THE SWORD ONLY\nYou walk north into the mountains.\nThe truth stays buried.\nYou don't look back."])
    ENDING_QUIET(["🌫️ ENDING: QUIET DEPARTURE\nYou leave before anyone wakes.\nOkita lets you go.\nYou'll never know what he wanted to say."])

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
