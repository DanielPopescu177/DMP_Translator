# DMP Translator

Real-time Japanese to Korean/English translation mod for Duel Masters Play (MelonLoader)

## ✨ Features

- **Real-time Translation**: Automatically translates Japanese text to Korean or English
- **Multiple Translation Engines**: Support for Google Translate (free), Papago, and DeepL API
- **Korean Font Support**: Custom TextMeshPro Korean font for proper display
- **Translation Cache**: Saves translations locally for faster loading
- **UI Customization**: Adjust character spacing, line spacing, and auto-sizing
- **Hotkeys**: Quick access to translation functions

## 🎮 Hotkeys

| Key | Function |
|-----|----------|
| **F8** | Print all text information on screen |
| **F9** | Refresh translation cache |
| **F10** | Toggle auto-translate ON/OFF |
| **F11** | Manually translate current screen |

## 📦 Installation

### Prerequisites
- Duel Masters Play (PC version)
- [MelonLoader](https://melonwiki.xyz/) installed

### Steps

1. Download the latest `DMP_Translator.dll` from [Releases](../../releases)
2. Copy `DMP_Translator.dll` to `<Game Directory>/Mods/` folder
3. Run the game once to generate config files
4. (Optional) Install Korean font for proper Korean text display

## ⚙️ Configuration

After first run, edit `UserData/DMP_Translator/translator_config.txt`:

### Basic Settings

```ini
# Translation engine: google, papago, or deepl
TRANSLATION_ENGINE=google

# Source and target languages
SOURCE_LANG=ja
TARGET_LANG=ko  # or en for English
```

### Translation Engines

#### Google Translate (Default)
- **Free**: No API key required
- **Setup**: Already configured by default

#### Papago (Naver Cloud)
- **Quality**: High accuracy for Korean
- **Setup**: Get API key from [Naver Cloud](https://www.ncloud.com/)
```ini
TRANSLATION_ENGINE=papago
PAPAGO_CLIENT_ID=your_client_id
PAPAGO_CLIENT_SECRET=your_client_secret
```

#### DeepL
- **Quality**: Best translation quality
- **Free Tier**: 500,000 characters/month
- **Setup**: Get API key from [DeepL](https://www.deepl.com/pro-api)
```ini
TRANSLATION_ENGINE=deepl
DEEPL_API_KEY=your_api_key
SOURCE_LANG=JA  # DeepL uses uppercase
TARGET_LANG=KO  # or EN
```

### UI Customization

```ini
# Character spacing (0-10, default: 2) - TextMeshPro only
# Increase if characters overlap
CHARACTER_SPACING=2

# Line spacing (-10 to 10, default: 0) - All text types
# Positive: wider spacing, Negative: narrower spacing
LINE_SPACING=0

# Auto size adjustment (true/false, default: false) - TextMeshPro only
# Automatically resize text to fit the box
ENABLE_AUTO_SIZING=false
```

## 🔤 Korean Font Installation

For proper Korean text display in TextMeshPro elements:

### Quick Method
1. Download the pre-built font bundle from [Releases](../../releases)
2. Place `notosanscjkjp_bold` file in `UserData/DMP_Translator/` folder

### Build From Unity (Advanced)
1. Open Unity project with TextMeshPro installed
2. Use `Tools/CreateFontBundle.cs` script
3. Select **Noto Sans CJK JP Bold** font
4. Set Character Set to **Unicode Range (Hex)**:
   ```
   20-7E,A0-FF,2000-206F,3000-303F,3040-309F,30A0-30FF,4E00-9FFF,AC00-D7A3,FF00-FFEF
   ```
5. Generate Font Atlas (4096x4096 or higher)
6. Build AssetBundle and place in `UserData/DMP_Translator/`

## 🛠️ Building from Source

### Requirements
- .NET 6.0 SDK
- Visual Studio 2022 or JetBrains Rider
- MelonLoader and Il2Cpp dependencies

### Build Steps

1. Clone the repository:
```bash
git clone https://github.com/yourusername/DMP_Translator.git
cd DMP_Translator
```

2. Update DLL references in `DMP_Translator.csproj`:
   - Update paths to your game's MelonLoader DLLs
   - Located in `<Game Directory>/MelonLoader/`

3. Build:
```bash
build.bat
```

4. Output: `bin/Release/net6.0/DMP_Translator.dll`

## 📁 Project Structure

```
DMP_Translator/
├── TranslatorMod.cs          # Main mod entry point
├── Translator.cs             # Translation API handlers
├── DeepLTranslator.cs        # DeepL API implementation
├── DMP_Translator.csproj     # Project file
├── build.bat                 # Build script
└── Tools/
    ├── CreateFontBundle.cs   # Unity Editor script for font creation
    └── cache_converter_gui.py # Translation cache converter tool
```

## 🐛 Troubleshooting

### Translation not working
- Check `LogOutput.log` in game directory
- Verify `translator_config.txt` settings
- Press **F10** to toggle auto-translate

### Korean text shows as squares (□□)
- Install Korean font following [Korean Font Installation](#-korean-font-installation)
- Font is only needed for Korean translation

### English text shows orange boxes
- This is expected if Korean font is applied to English text
- The mod automatically uses game's default font for English

### Overlapping characters
- Increase `CHARACTER_SPACING` in config file
- Try values: 3, 4, or 5

### Text cut off
- Set `ENABLE_AUTO_SIZING=true` in config file
- TextMeshPro will automatically resize text to fit


## 🙏 Acknowledgments

- [MelonLoader](https://melonwiki.xyz/) - Mod loader framework
- [Il2CppInterop](https://github.com/BepInEx/Il2CppInterop) - IL2CPP interoperability
- Google Translate, Papago, DeepL - Translation services
- Noto Sans CJK - Font family


## ⚠️ Disclaimer

This mod is not affiliated with or endorsed by Duel Masters Play or its developers. Use at your own risk.

---

**Made with ❤️ for the Duel Masters Play community**
