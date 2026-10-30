![EXILED](https://img.shields.io/badge/EXILED-9.12.6%2B-blue)
![Version](https://img.shields.io/badge/Version-2.2.0-blue)
![Author](https://img.shields.io/badge/Author-Matysiak-green)

# SCP-153 | BULSON

Grywalna CustomRole oparta na SCP-049-2: ugryzienia, ewolucja, regenerujacy sie HumeShield, model ProjectMER i dzwiek HAPS.ogg.

## Wersja 2.2.0

- **400 bazowego HS**, **+50 do maksymalnego i aktualnego HS za skuteczne ugryzienie**, limit **2000 HS**.
- Nagrody sa przyznawane po faktycznie przyjetych obrazeniach. Anulowana proba ataku nie daje ewolucji, HS ani leczenia.
- Kazde nadanie roli ma nowy stan. Powtorne give oraz nadpisanie innej CustomRole sa odrzucane.
- Wylaczenie pluginu usuwa tylko jego wlasna role i zasoby.
- Awaria schematu pozostawia widoczny model Zombie.
- Widocznosc jest uzgadniana z aktualnym celem obserwatora i listami observerow Mirror.
- Teleport wymaga podloza i wolnego miejsca na gracza. Bez poprawnego punktu pozostaje on na miejscu. Po detonacji warhead nie wykonuje sie teleportacji ani przeniesienia do Pocket Dimension.

## Wymagania i sprawdzenie

- Serwer 64-bitowy, EXILED z wlaczonym CustomRoles i Permissions.
- ProjectMER oraz AudioPlayerApi wraz z ich zaleznosciami.
- Projekt kompilowany przeciw referencjom EXILED **9.12.6**, gry **14.2.5** i LabAPI **1.1.4** dolaczonym w REFERENCES.
- Nowsze wersje serwera wymagaja sprawdzenia zgodnosci. Nie deklarujemy obslugi wszystkich wersji 13.x+ ani dowolnego EXILED Latest.
- Biblioteka Pooling.dll jest referencja kompilacyjna gry. Nie kopiuj folderu REFERENCES na serwer.
- Testy logiki oraz kompilacja nie zastepuja testu klienta: [plan testow](docs/TESTING.md), [powiazanie z audytem](docs/AUDIT_FIXES.md).

## Instalacja

1. Wgraj **SCP153.dll** z paczki do EXILED/Plugins.
2. Wgraj schemat jako folder SCP153 zawierajacy SCP153.json do katalogu ProjectMER, zwykle:
   SCP Secret Laboratory/LabAPI/configs/ProjectMER/Schematics/SCP153.
3. Wgraj HAPS.ogg do EXILED/Configs/Plugins/scp153/Audio/HAPS.ogg.
4. Zrestartuj serwer i sprawdz log uruchomienia. Niepoprawna konfiguracja lub kolizja ID zatrzymuje uruchamianie logiki SCP-153.

**Aktualizacja istniejacego configu:** EXILED zachowuje zapisane wartosci. Zmien trzy istniejace wpisy, jesli chcesz nowy balans:

```yaml
scp153:
  base_hume_shield: 400
  hume_shield_per_hit: 50
  max_hume_shield_limit: 2000
```

To fragment do podmiany we wlasnym configu, nie kompletny plik. Pelny przyklad: [config/default.yml](config/default.yml).

## Dzialanie i balans

Po 5 sekundach aktywnej rundy plugin losuje spawn (domyslnie 10%). Wybiera zywego SCP bez innej CustomRole, pomijajac SCP-079. Nadaje role i probuje znalezc sprawdzony punkt w korytarzu HCZ.

Bazowe HP: **4500**. Tarcza regeneruje sie do wypracowanego limitu w tempie **15 HS/s** po **5 sekundach bez przyjetych obrazen**. Natywna regeneracja Zombie jest wylaczona tylko dla SCP-153; wlasna regeneracja respektuje blokady kontrolera HumeShield.

Jeden natywny zamach moze przyznac najwyzej jedno ugryzienie. Liczymy skuteczne trafienia, nie zabojstwa ani unikalnych graczy. HS pochlaniajacy obrazenia ofiary rowniez oznacza przyjete trafienie.

| Ugryzienia | Maksymalny HS | Ewolucja |
| --- | --- | --- |
| 0 | 400 | Slowness 20; 50 DMG |
| 3 | 550 | Polowa poczatkowego spowolnienia |
| 5 | 650 | Mnoznik obrazen 1.5 (domyslnie 75 DMG) |
| 7 | 750 | Koniec wlasnego spowolnienia, MovementBoost 10, krotszy cooldown |
| 10 | 900 | MovementBoost 20, regeneracja x2, leczenie 250 HP za trafienie, Pocket Dimension i rozbrojenie, blackout 15 s |
| 32 | 2000 | Limit maksymalnego HS |

Po osiagnieciu limitu ugryzienia nadal uzupelniaja aktualny HS o 50, maksymalnie do 2000. Wiekszosc parametrow i progi ewolucji mozna zmienic w konfiguracji. Ewolucje nie zmniejszaja skonfigurowanych obrazen ani nie zwiekszaja poczatkowego spowolnienia.

**Cooldown:** serwer uzywa natywnego cooldownu Zombie z parametrem 3/2 s dla SCP-153, bez drugiej blokady w Hurting. Niezmodyfikowany klient przewiduje lokalne animacje wedlug swojego timera ok. 1.3 s. Serwerowy plugin nie zmienia kodu klienta: rytm przy przytrzymaniu przycisku i puste animacje wymagaja sprawdzenia w grze; nie gwarantujemy identycznego timera HUD. Konfiguracja ponizej 1.3 s jest odrzucana.

**Zdolnosci bazowe:** Bloodlust i zjadanie zwlok Zombie pozostaja aktywne. Schemat jest wizualna dekoracja; hitboxy i animacje nadal naleza do bazowego SCP-049-2.

**Teleport:** uzywa zwyklych korytarzy LCZ/HCZ i nie wybiera LCZ po dekontaminacji. Przy braku bezpiecznego punktu nie teleportuje. Po detonacji warhead nadal dzialaja obrazenia i progresja, ale nie przemieszczenie ofiary. Wejscie do Pocket Dimension oraz upuszczenie przedmiotu moga byc zablokowane przez inne systemy serwera.

## Komendy

Wymagane uprawnienie: **scp153.manage**. Nadanie roli jest dostepne podczas aktywnej rundy.

| Komenda | Alias | Dzialanie |
| --- | --- | --- |
| scp153 give <id/nick> | 153 g <id/nick> | Nadaje role graczowi bez CustomRole |
| scp153 remove <id/nick> | 153 r <id/nick> | Domyslnie usuwa role i przenosi gracza do Spectator |

Opcja removal_kills_player: false pozostawia zwyklego Zombie, usuwa wlasne efekty i oddaje kontrole nad HP/HS mechanizmom bazowym.

## Kompilacja

Wymagany .NET SDK 8:

```sh
dotnet run --project Tests/SCP153.Tests.csproj --configuration Release
dotnet build BULISCP.csproj --configuration Release
```

Wynik: bin/Release/SCP153.dll. CI wykonuje testy i kompilacje na Windows oraz udostepnia paczke zawierajaca plugin, model i audio. Nie publikuje ani nie wdraza wydania na serwer.

## English

SCP-153 is an EXILED CustomRole based on SCP-049-2. Version 2.2.0 starts with **400 HumeShield**, gains **50 maximum/current HS per confirmed bite**, and caps maximum HS at **2000** (32 bites; final evolution at 10 bites has 900 HS). Existing YAML values are preserved by EXILED: update base_hume_shield, hume_shield_per_hit and max_hume_shield_limit when upgrading.

Rewards commit only after actual damage, once per native swing. Each role assignment owns fresh state; shutdown unregisters only this plugin's role. Missing/failed schematics leave a visible Zombie fallback. Teleports validate ground and capsule clearance, skip unavailable destinations, and stop after warhead detonation. Spectator visibility is reconciled with current state and Mirror observers.

Build references are EXILED 9.12.6 / SCP:SL 14.2.5. Runtime compatibility with newer versions requires server validation. Native Zombie Bloodlust/corpse consumption and hitboxes remain. Server cooldown is patched for this role only; the unmodified client's predicted animations still use its own timer. See docs/TESTING.md for mandatory in-game checks.

Install SCP153.dll into EXILED/Plugins, the SCP153 schematic folder into ProjectMER/Schematics, and HAPS.ogg into EXILED/Configs/Plugins/scp153/Audio. Administrative commands require scp153.manage; remove defaults to moving the player to Spectator.
