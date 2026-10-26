![EXILED Version](https://img.shields.io/badge/EXILED-Latest-blue)
![Game Version](https://img.shields.io/badge/SCP:SL-13.x%2B-red)
![Author](https://img.shields.io/badge/Author-Matysiak-green)
![Version](https://img.shields.io/badge/Version-2.1.0-blue)

# SCP-153 | BULSON

SCP-153, internally designated "Bulson", is a biological anomaly classified as a humanoid parasitic organism. The entity resembles a deformed, massive humanoid figure — significantly larger than an average human, moving in a characteristic, lumbering manner.

Bulson displays predatory behavior — actively hunting facility personnel and any other individuals encountered. Its attack method is unique and currently unexplained: upon contact with a victim, the entity partially consumes them, after which the victim is instantly displaced to a random corridor within the facility. The teleportation mechanism is believed to be related to the anomalous structure of the entity's digestive system.

Each successful "consumption" permanently expands and regenerates Bulson's protective HumeShield layer, while driving the anomaly into a state of progressive evolution. Initially slower than an average human, a well-fed SCP-153 rapidly gains movement speed, attack power, and eventually the ability to tear open rifts directly into the Pocket Dimension.

During an attack, the entity emits a characteristic sound — a deep, vibrating absorption noise.

**Class:** Euclid / Keter (upon reaching final evolution stage)  
**Containment Status:** Active

> **LOOKING FOR PRO MODELER, TO REPAIR THE MODEL!!! (SORY IM BAD WITH UNITY!!!!)**

---

🇵🇱 [Polski](#polski) | 🇬🇧 [English](#english)

---

## 🇵🇱 Polski

Plugin do SCP: Secret Laboratory oparty na frameworku EXILED. Dodaje do gry grywalną postać SCP-153 z systemem ewolucji w trakcie rundy, rosnącą tarczą HumeShield oraz ukrywaniem modelu na poziomie pakietów sieciowych.

### ✨ Nowości i Poprawki (Aktualizacja Ewolucji i Optymalizacji)
- **4-Poziomowy System Ewolucji:** Buli rośnie w siłę z każdą kolejną pożartą ofiarą (szybszy ruch, większe obrażenia, krótszy cooldown, aż po formę ostateczną „BULI SIĘ NAŻARŁ” z Wymiarem Łzowym, wampiryzmem i blackoutem placówki).
- **Przebudowany HumeShield:** Tarcza startuje od `300` i każde ugryzienie **na stałe zwiększa jej maksymalny limit o +100** (aż do pułapu `1500`). Poza walką tarcza płynnie regeneruje się do aktualnie wypracowanego limitu.
- **Ukrycie modelu dla Gracza i Obserwatorów (Mirror):** Wykorzystano pakiety sieciowe (`ObjectDestroyMessage` / `SendSpawnMessage`), dzięki czemu model 3D jest całkowicie niewidoczny dla osoby grającej jako SCP-153 oraz dla obserwujących go Spectatorów (zero zasłaniania ekranu przy `OffsetZ = 0`), pozostając w pełni widocznym dla reszty serwera.
- **100% Bezpieczna Teleportacja:** Zastosowano białą listę płaskich korytarzy oraz detekcję podłogi przez `Physics.Raycast`. Ofiary nie wypadają już poza mapę i nie są teleportowane do skażonego LCZ po dekontaminacji.
- **Stabilność Audio i Wydajność:** Zabezpieczono `AudioClipStorage` przed duplikacją klipu `HAPS.ogg` w kolejnych rundach, usunięto zbędne pętle `LateUpdate` oraz zablokowano nabijanie ugryzień przez samookaleczenie (np. od Tesli/upadku).

### 📋 Wymagania

Aby plugin działał poprawnie, upewnij się, że posiadasz zainstalowane:
- EXILED (Najnowsza wersja)
- Exiled.CustomRoles & Exiled.Permissions (Standardowo dołączone do EXILED)
- ProjectMER
- AudioPlayerApi / SCPSLAudioApi

### ⚙️ Instalacja

#### 1. Plugin
Umieść skompilowany plik `SCP153.dll` w folderze pluginów:

    EXILED/Plugins/

#### 2. Schematic (Model)
Umieść folder ze schematem `SCP153` w katalogu ProjectMER. Folder musi zawierać plik `.json` oraz wszystkie powiązane assety.

    SCP Secret Laboratory/LabAPI/configs/ProjectMER/Schematics/SCP153

#### 3. Audio
Umieść plik dźwiękowy w dedykowanym folderze pluginu:

    EXILED/Configs/Plugins/scp153/Audio/HAPS.ogg

⚠️ **Ważne:** Plik musi być w formacie `.ogg`, **mono**, o częstotliwości **48kHz**. Inne formaty lub ustawienia nie będą działać poprawnie.

### 📂 Struktura folderów (końcowy wynik)

    EXILED/
    ├── Plugins/
    │   └── SCP153.dll
    ├── Configs/
    │   ├── Plugins/
    │   │   └── scp153/
    │   │       └── Audio/
    │   │           └── HAPS.ogg
    SCPSecretLaboratory/
    ├── LabAPI/
    │   └── configs/
    │       └── ProjectMER/
    │           └── Schematics/
    │               └── SCP153/
    │                   └── SCP153.json

### 📝 Konfiguracja
Po pierwszym uruchomieniu serwera config pojawi się automatycznie w pliku konfiguracyjnym EXILED:

| Opcja | Domyślnie | Opis |
| :--- | :--- | :--- |
| `is_enabled` | `true` | Włącza/wyłącza plugin. |
| `debug` | `false` | Logi debugowania w konsoli. |
| `schematic_name` | `SCP153` | Nazwa schematu z ProjectMER. |
| `spawn_chance` | `0.1` | Szansa spawnu na starcie rundy (`0.1` = 10%). |
| `max_health` | `4500` | Maksymalne bazowe HP dla SCP-153. |
| `base_hume_shield` | `300` | Początkowy HumeShield na starcie rundy. |
| `max_hume_shield_limit` | `1500` | Maksymalny pułap HumeShielda, jaki Buli może osiągnąć. |
| `hume_shield_per_hit` | `100` | Stały bonus do maksymalnego i aktualnego HumeShielda za każde ugryzienie. |
| `hume_shield_regen_rate` | `15` | Prędkość regeneracji HumeShielda na sekundę. |
| `hume_shield_regen_delay` | `5` | Czas (w sekundach) bez otrzymywania obrażeń potrzebny do startu regeneracji tarczy. |
| `slowness_intensity` | `20` | Początkowa siła efektu spowolnienia (`Slowness`). |
| `damage` | `50` | Bazowe obrażenia ugryzienia (przed 2 poziomem ewolucji). |
| `eat_cooldown` | `3` | Bazowy czas odnowienia ugryzienia w sekundach. |

### 🎮 Jak działa

**Spawn:** 5 sekund po starcie rundy plugin losuje szansę na pojawienie się SCP-153 (domyślnie **10%**). Jeśli losowanie się powiedzie, wybiera losowego gracza z drużyny SCP (pomijając SCP-079 oraz graczy posiadających już inną CustomRole, np. SCP-066), zamienia go w SCP-153 i teleportuje do zbrojowni/korytarza w Heavy Containment Zone.

**Statystyki Bazowe:**
- **HP:** 4500
- **HumeShield:** 300 (rośnie o `+100` na stałe za każde ugryzienie, max `1500`; regeneruje się poza walką w tempie `15/s` po `5s` spokoju)
- **Prędkość początkowa:** Spowolniony (`Slowness 20`)

**Atak podstawowy:**
- Zadaje **50 obrażeń** i odtwarza dźwięk `HAPS.ogg`.
- Dodaje na stałe **+100 do maksymalnego oraz aktualnego HumeShielda**.
- Teleportuje ofiarę do bezpiecznego korytarza w LCZ (jeśli nie było dekontaminacji) lub HCZ.
- Cooldown ataku: **3 sekundy**.

### 🧬 System Ewolucji (Pożeranie Ofiar)

| Poziom | Wymagane Ugryzienia | Efekty Ewolucji |
| :--- | :--- | :--- |
| **Poziom 1** | **3 / 10** | Zmniejszenie spowolnienia (`Slowness` spada z `20` na `10`). |
| **Poziom 2** | **5 / 10** | Obrażenia ugryzienia rosną na stałe z `50` do **75 DMG**. |
| **Poziom 3** | **7 / 10** | Całkowite zdjęcie spowolnienia, nałożenie przyspieszenia (`MovementBoost 10`) oraz skrócenie cooldownu ataku o **1 sekundę** (do `2s`). |
| **Poziom 4 (MAX)** | **10 / 10** | **„BULI SIĘ NAŻARŁ!”:**<br>• Przyspieszenie rośnie do `MovementBoost 20`, a regeneracja tarczy przyspiesza dwukrotnie (`30/s`).<br>• **Blackout:** Światła w całej placówce gasną na 15 sekund wraz z globalnym ostrzeżeniem.<br>• **Wymiar Łzowy i Rozbrojenie:** Każde ugryzienie wytrąca ofierze trzymany przedmiot z rąk i wysyła ją prosto do **Wymiaru Łzowego** (`PocketCorroding`).<br>• **Wampiryzm:** Każde ugryzienie leczy Buliemu **+250 HP**. |

### 💻 Komendy (Remote Admin)
Wymagane uprawnienie: `scp153.manage`

| Komenda | Alias | Opis |
| :--- | :--- | :--- |
| `scp153 give <nick/id>` | `153 g <nick/id>` | Nadaje rolę SCP-153 wybranemu graczowi. |
| `scp153 remove <nick/id>` | `153 r <nick/id>` | Odbiera rolę SCP-153 wybranemu graczowi. |

---
---

## 🇬🇧 English

Plugin for SCP: Secret Laboratory based on the EXILED framework. Adds the playable character SCP-153 featuring an in-round evolution system, permanently scaling HumeShield, and network-level schematic hiding.

### ✨ What's New (Evolution & Optimization Update)
- **4-Tier Evolution System:** Buli scales in power with every devoured victim (faster movement, increased damage, shorter attack cooldown, culminating in the ultimate form featuring Pocket Dimension rifts, lifesteal, disarming, and a facility-wide blackout).
- **Overhauled HumeShield:** Starts at `300` and every bite **permanently increases the maximum shield cap by +100** (up to `1500`). Out of combat, the shield smoothly regenerates up to its current permanent cap.
- **Owner & Spectator Model Hiding (Mirror):** Uses Mirror network messages (`ObjectDestroyMessage` / `SendSpawnMessage`) to completely hide the 3D schematic from the player controlling SCP-153 and anyone spectating them (zero camera obstruction with `OffsetZ = 0`), while keeping it fully visible to everyone else.
- **100% Safe Teleportation:** Uses a flat-corridor whitelist combined with `Physics.Raycast` ground detection. Victims will never fall out of bounds or get teleported into decontaminated LCZ.
- **Audio Stability & Performance:** Protected `AudioClipStorage` against duplicate key exceptions across multiple rounds, removed redundant `LateUpdate` loops, and prevented self-damage (e.g., Tesla gates/fall damage) from triggering bites.

### 📋 Requirements

- EXILED (Latest version)
- Exiled.CustomRoles & Exiled.Permissions (Included with EXILED)
- ProjectMER
- AudioPlayerApi / SCPSLAudioApi

### ⚙️ Installation

#### 1. Plugin
Place the compiled `SCP153.dll` file in:

    EXILED/Plugins/

#### 2. Schematic (Model)
Place the `SCP153` schematic folder in the ProjectMER directory:

    SCP Secret Laboratory/LabAPI/configs/ProjectMER/Schematics/SCP153

The folder must contain the schematic `.json` file and all related assets.

#### 3. Audio
Place the audio file in:

    EXILED/Configs/Plugins/scp153/Audio/HAPS.ogg

⚠️ **Important:** The file must be in `.ogg` format, **mono**, frequency **48kHz**. Other formats or settings will not work properly.

### 📂 Folder Structure (final result)

    EXILED/
    ├── Plugins/
    │   └── SCP153.dll
    ├── Configs/
    │   ├── Plugins/
    │   │   └── scp153/
    │   │       └── Audio/
    │   │           └── HAPS.ogg
    SCPSecretLaboratory/
    ├── LabAPI/
    │   └── configs/
    │       └── ProjectMER/
    │           └── Schematics/
    │               └── SCP153/
    │                   └── SCP153.json

### 📝 Configuration
After the first server launch, the config will be generated automatically in your EXILED config file:

| Option | Default | Description |
| :--- | :--- | :--- |
| `is_enabled` | `true` | Enables/disables the plugin. |
| `debug` | `false` | Debug logs in the server console. |
| `schematic_name` | `SCP153` | Schematic name from ProjectMER. |
| `spawn_chance` | `0.1` | Spawn chance at round start (`0.1` = 10%). |
| `max_health` | `4500` | Base maximum HP for SCP-153. |
| `base_hume_shield` | `300` | Starting HumeShield at the beginning of the round. |
| `max_hume_shield_limit` | `1500` | Maximum HumeShield cap that SCP-153 can reach. |
| `hume_shield_per_hit` | `100` | Permanent bonus to max and current HumeShield per bite. |
| `hume_shield_regen_rate` | `15` | HumeShield regeneration rate per second. |
| `hume_shield_regen_delay` | `5` | Time (in seconds) without taking damage required to start regenerating shield. |
| `slowness_intensity` | `20` | Initial `Slowness` effect intensity. |
| `damage` | `50` | Base bite damage (prior to Evolution Tier 2). |
| `eat_cooldown` | `3` | Base cooldown between attacks in seconds. |

### 🎮 How it works

**Spawn:** 5 seconds after the round starts, the plugin rolls the dice (default **10% chance**). If successful, it picks a random SCP from the active pool (skipping SCP-079 and players who already have another CustomRole, e.g., SCP-066), turns them into SCP-153, and teleports them to the Heavy Containment Zone armory/corridor.

**Base Stats:**
- **HP:** 4500
- **HumeShield:** 300 (permanently increases by `+100` per bite, up to `1500`; regenerates out of combat at `15/s` after `5s` of not taking damage)
- **Initial Speed:** Slowed (`Slowness 20`)

**Base Attack:**
- Deals **50 damage** and plays `HAPS.ogg`.
- Permanently adds **+100 to maximum and current HumeShield**.
- Teleports the victim to a safe corridor in LCZ (if not decontaminated) or HCZ.
- Base attack cooldown: **3 seconds**.

### 🧬 Evolution System (Devouring Victims)

| Tier | Required Bites | Evolution Effects |
| :--- | :--- | :--- |
| **Tier 1** | **3 / 10** | Movement penalty reduced (`Slowness` drops from `20` to `10`). |
| **Tier 2** | **5 / 10** | Bite damage permanently increased from `50` to **75 DMG**. |
| **Tier 3** | **7 / 10** | `Slowness` completely removed, `MovementBoost 10` applied, and attack cooldown reduced by **1 second** (down to `2s`). |
| **Tier 4 (MAX)** | **10 / 10** | **Ultimate Form:**<br>• Speed increased to `MovementBoost 20` and HumeShield regeneration rate doubled (`30/s`).<br>• **Facility Blackout:** All facility lights turn off for 15 seconds accompanied by a global warning broadcast.<br>• **Pocket Dimension & Disarm:** Every bite forces the victim to drop their held item and sends them straight to the **Pocket Dimension** (`PocketCorroding`).<br>• **Lifesteal:** Every bite heals SCP-153 for **+250 HP**. |

### 💻 Commands (Remote Admin)
Required permission: `scp153.manage`

| Command | Alias | Description |
| :--- | :--- | :--- |
| `scp153 give <nick/id>` | `153 g <nick/id>` | Grants the SCP-153 role to a player. |
| `scp153 remove <nick/id>` | `153 r <nick/id>` | Removes the SCP-153 role from a player. |

**Author:** Matysiak  
**Version:** 2.1.0
