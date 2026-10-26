![EXILED Version](https://img.shields.io/badge/EXILED-Latest-blue)
![Game Version](https://img.shields.io/badge/SCP:SL-13.x-red)
![Author](https://img.shields.io/badge/Author-Matysiak-green)
![Version](https://img.shields.io/badge/Version-2.0.0-blue)

# SCP-153 | BULSON

SCP-153, internally designated "Bulson", is a biological anomaly classified as a humanoid parasitic organism. The entity resembles a deformed, massive humanoid figure — significantly larger than an average human, moving in a characteristic, lumbering manner.

Bulson displays predatory behavior — actively hunting facility personnel and any other individuals encountered. Its attack method is unique and currently unexplained: upon contact with a victim, the entity partially consumes them, after which the victim is instantly displaced to a random location within the facility. The teleportation mechanism is believed to be related to the anomalous structure of the entity's digestive system.

Each successful "consumption" regenerates Bulson's protective layer, suggesting that consuming victims is essential to its survival. The entity moves slower than the average human, however its durability is extremely high.

During an attack, the entity emits a characteristic sound — a deep, vibrating absorption noise.

**Class:** Euclid  
**Containment Status:** Active

> **LOOKING FOR PRO MODELER, TO REPAIR THE MODEL!!! (SORY IM BAD WITH UNITY!!!!)**

---

🇵🇱 [Polski](#polski) | 🇬🇧 [English](#english)

---

## 🇵🇱 Polski

Plugin do SCP: Secret Laboratory oparty na frameworku EXILED. Dodaje do gry grywalną postać SCP-153 z unikalnymi mechanikami.

### ✨ Nowości i Poprawki (Aktualizacja Stabilności)
* **Anti-Crash:** Całkowicie przebudowano system Bota Audio. Usunięto awaryjny system "parentowania", który dławił serwer.
* **Naprawiony Offset:** Zoptymalizowano pozycjonowanie modelu — Buli trzyma się teraz idealnie pleców gracza i nie przenika przez ściany.
* **Balans i Optymalizacja:** Zmieniono bazowe HP na 4500 oraz usprawniono system ukrywania oryginalnego modelu zombie.

### 📋 Wymagania

Aby plugin działał poprawnie, upewnij się, że posiadasz zainstalowane:
* EXILED (Najnowsza wersja)
* Exiled.CustomRoles (Standardowo dołączone do EXILED)
* ProjectMER
* SCPSLAudioApi

### ⚙️ Instalacja

#### 1. Plugin
Umieść skompilowany plik `SCP153.dll` w folderze pluginów:

    EXILED/Plugins/

#### 2. Schematic (Model)
Umieść folder ze schematem `SCP153` w katalogu MapEditorReborn. Folder musi zawierać plik `.json` oraz wszystkie powiązane assety.

    SCP Secret Laboratory/LabAPI/configs/ProjectMER/Schematics/SCP153

#### 3. Audio
Umieść plik dźwiękowy w dedykowanym folderze pluginu:

    EXILED/Configs/Plugins/scp153/Audio/HAPS.ogg

⚠️ **Ważne:** Plik musi być w formacie `.ogg`, **mono**, o częstotliwości **48kHz**. Inne formaty lub ustawienia nie będą działać z SCPSLAudioApi.

### 📂 Struktura folderów (końcowy wynik)

    EXILED/
    ├── Plugins/
    │   └── SCP153.dll
    ├── Configs/
    │   ├── Plugins/
    │   │   └── scp153/
    │   │       └── Audio/
    │   │         └── HAPS.ogg
    SCPSecretLaboratory/
    ├──LabAPI/
    │   └── config/
    │     └── ProjectMER/
    │       └── Schematics/
    │           └── SCP153/
    │               └── SCP153.json

### 📝 Konfiguracja
Po pierwszym uruchomieniu serwera config pojawi się automatycznie w: `EXILED/Configs/config_gameplay.yml`

| Opcja | Domyślnie | Opis |
| :--- | :--- | :--- |
| `is_enabled` | `true` | Włącza/wyłącza plugin |
| `debug` | `false` | Logi debugowania w konsoli |
| `schematic_name` | `SCP153` | Nazwa schematu z MapEditorReborn |
| `spawn_chance` | `0.15` | Szansa spawnu (0.15 = 15%) |
| `damage` | `50` | Obrażenia zadawane przy ataku |
| `eat_cooldown` | `3` | Cooldown między atakami (sekundy) |

### 🎮 Jak działa

**Spawn:** Na starcie każdej rundy plugin rzuca kością — **15% szansy**, że SCP-153 w ogóle się pojawi. Jeśli wylosuje spawn, wybiera losowego SCPa z puli (pomija SCP-079 i graczy, którzy już mają inną CustomRole np. SCP-066). Wybrany gracz zostaje zamieniony w SCP-153 — zachowuje swój oryginalny spawn point.

**Statystyki:**
* **Prędkość:** ~80% normalnej prędkości
* **HP:** 4500
* **HumeShield:** 500

**Atak:**
* Zadaje **50 obrażeń**.
* Odtwarza dźwięk `HAPS.ogg`.
* Teleportuje ofiarę do losowego pokoju w Light lub Heavy Containment.
* Za każdy atak regeneruje **100 HumeShield**.
* Cooldown ataku: **3 sekundy**.

### 💻 Komendy (Remote Admin)

| Komenda | Opis |
| :--- | :--- |
| `.scp153 give <nick/id>` | Nadaje rolę SCP-153 graczowi. |
| `.scp153 remove <nick/id>` | Odbiera rolę SCP-153 graczowi. |

---
---

## 🇬🇧 English

Plugin for SCP: Secret Laboratory based on the EXILED framework. Adds the playable character SCP-153 with unique mechanics.

### ✨ What's New (Stability Update)
* **Anti-Crash:** Completely rebuilt the Audio Bot system. Removed the "parenting" bug that was crashing the server.
* **Fixed Offset:** Optimized model positioning — Buli now sticks perfectly to the player's back and no longer clips through walls.
* **Balance & Optimization:** Reduced base HP to 4500 and improved the default zombie model invisibility system.

### 📋 Requirements

* EXILED (latest version)
* Exiled.CustomRoles (included with EXILED)
* ProjectMER
* SCPSLAudioApi

### ⚙️ Installation

#### 1. Plugin
Place the compiled `SCP153.dll` file in:

    EXILED/Plugins/

#### 2. Schematic (model)
Place the `SCP153` schematic folder in the MapEditorReborn directory:

    SCP Secret Laboratory/LabAPI/configs/ProjectMER/Schematics/SCP153

The folder must contain the schematic `.json` file and all related assets.

#### 3. Audio
Place the audio file in:

    EXILED/Configs/Plugins/scp153/Audio/HAPS.ogg

⚠️ **Important:** The file must be in `.ogg` format, **mono**, frequency **48kHz**. Other formats or settings will not work with SCPSLAudioApi.

### 📂 Folder Structure (final result)

    EXILED/
    ├── Plugins/
    │   └── SCP153.dll
    ├── Configs/
    │   ├── Plugins/
    │   │   └── scp153/
    │   │       └── Audio/
    │   │         └── HAPS.ogg
    SCPSecretLaboratory/
    ├──LabAPI/
    │   └── config/
    │     └── ProjectMER/
    │       └── Schematics/
    │           └── SCP153/
    │               └── SCP153.json

### 📝 Configuration
After the first server launch, the config will appear automatically in: `EXILED/Configs/config_gameplay.yml`

| Option | Default | Description |
| :--- | :--- | :--- |
| `is_enabled` | `true` | Enables/disables the plugin |
| `debug` | `false` | Debug logs in console |
| `schematic_name` | `SCP153` | Schematic name from MapEditorReborn |
| `spawn_chance` | `0.15` | Spawn chance (0.15 = 15%) |
| `damage` | `50` | Damage dealt per attack |
| `eat_cooldown` | `3` | Cooldown between attacks (seconds) |

### 🎮 How it works

**Spawn:** At the start of each round the plugin rolls the dice — **15% chance** that SCP-153 will appear at all. If it rolls a spawn, it picks one random SCP from the pool (skips SCP-079 and players who already have another CustomRole e.g. SCP-066). The chosen player is replaced by SCP-153 — they keep their original spawn point.

**Stats:**
* Moves at ~80% of normal speed.
* Has **4500 HP** and **500 HumeShield**.

**Attack:**
* On attack deals **50 damage**.
* Plays `HAPS.ogg`.
* Teleports the victim to a random room in Light or Heavy Containment.
* Each attack regenerates **100 HumeShield**.
* Attack cooldown: **3 seconds**.

### 💻 Commands (Remote Admin)

| Command | Description |
| :--- | :--- |
| `.scp153 give <nick/id>` | Grants the SCP-153 role to a player. |
| `.scp153 remove <nick/id>` | Removes the SCP-153 role from a player. |

**Author:** Matysiak  
**Version:** 2.0.0
