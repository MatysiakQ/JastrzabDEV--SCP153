# SCP-153 BULSON v2.2.0

## Polski

Wydanie zawiera poprawki po audycie kodu i nowy balans HumeShield.

### Balans HS
- 400 HS na start.
- Każde potwierdzone ugryzienie dodaje 50 do maksymalnego i aktualnego HS.
- Regeneracja odnawia tarczę do wypracowanego maksimum; limit wynosi 2000 HS.
- Po 10 ugryzieniach maksimum wynosi 900 HS; limit 2000 osiągany jest po 32.
- Domyślna regeneracja: 15 HS/s po 5 s bez przyjętych obrażeń, 30 HS/s w końcowej ewolucji.

### Poprawki
- Nagrody za ugryzienie są przyznawane dopiero po rzeczywistych obrażeniach; anulowane trafienia nie dają HS, leczenia ani ewolucji.
- Każde nadanie roli otrzymuje nowy stan. Usuwanie i wyłączanie SCP-153 nie wyrejestrowuje cudzych CustomRoles.
- Usunięto opóźnione operacje mogące nadać statystyki niewłaściwej postaci lub pozostawić osierocone audio.
- Poprawiono obserwatorów Mirror i dodano widoczny model Zombie przy awarii schematu.
- Teleportacja sprawdza podłoże i wolne miejsce; bez poprawnego punktu nie przenosi gracza. Po detonacji warhead nie wysyła ofiar pod ziemię ani do Pocket Dimension.
- Regeneracja HS respektuje blokady kontrolera; bazowa regeneracja Zombie jest wyłączona tylko dla SCP-153.
- Dodano walidację konfiguracji, kontrolę kolizji ID, konfigurowalne ewolucje, 34 testy logiki i automatyczną kompilację/pakowanie.

### Instalacja i aktualizacja
Pobierz SCP153.dll albo pełną paczkę SCP153-2.2.0.zip. Paczka zawiera także model, audio, przykładową konfigurację i dokumentację. Plik SHA256SUMS.txt pozwala sprawdzić załączniki.

Istniejący config EXILED zachowuje poprzednie wartości. W sekcji scp153 podmień:
```yaml
base_hume_shield: 400
hume_shield_per_hit: 50
max_hume_shield_limit: 2000
```

### Weryfikacja i ograniczenia
Kompilacja .NET Framework 4.8/x64, 34 testy logiki oraz pakowanie są sprawdzane w GitHub Actions. Referencje kompilacyjne: EXILED 9.12.6, SCP:SL 14.2.5. Zależności: CustomRoles, Permissions, ProjectMER i AudioPlayerApi.

Nie wykonano testów serwera/klienta gry. Widoczność, geometria teleportacji, hitboxy i integracje wymagają sprawdzenia na docelowym serwerze. Cooldown serwerowy jest zmieniony dla tej roli, natomiast klient przewiduje animacje według swojego natywnego timera Zombie.

---

## English

This release addresses the code audit findings and rebalances HumeShield.

### HS balance
- 400 starting HS.
- Each confirmed bite adds 50 maximum and current HS.
- Regeneration restores the earned maximum, capped at 2000 HS.
- At 10 bites the maximum is 900 HS; 32 bites reach the 2000 cap.
- Default regeneration: 15 HS/s after 5 seconds without actual damage, increased to 30 HS/s at final evolution.

### Fixes
- Bite rewards commit only after actual damage. Cancelled hits grant no HS, healing or evolution progress.
- Every role assignment owns fresh state. Removing or disabling SCP-153 no longer unregisters other plugins' CustomRoles.
- Removed delayed operations that could assign stats to the wrong character or leave orphaned audio.
- Reconciled Mirror spectators and added a visible Zombie fallback when the schematic fails.
- Teleports validate ground and player clearance, skip invalid destinations, and stop displacing victims after warhead detonation.
- HS regeneration respects controller blockers; native Zombie regeneration is suppressed only for SCP-153.
- Added configuration validation, role-ID collision checks, configurable evolution, 34 logic regression tests and automated builds/packages.

### Installation and upgrade
Download SCP153.dll or the complete SCP153-2.2.0.zip package, which also includes the schematic, audio, sample configuration and documentation. Verify the attachments with SHA256SUMS.txt.

EXILED preserves existing configuration values. Update these keys in the scp153 section:
```yaml
base_hume_shield: 400
hume_shield_per_hit: 50
max_hume_shield_limit: 2000
```

### Verification and limitations
GitHub Actions checks the .NET Framework 4.8/x64 build, 34 logic tests and package generation. Build references: EXILED 9.12.6 / SCP:SL 14.2.5. Dependencies: CustomRoles, Permissions, ProjectMER and AudioPlayerApi.

No game server/client tests have been performed. Visibility, teleport geometry, hitboxes and integrations require validation on the deployed server. The server cooldown is adjusted for this role; unmodified clients still predict animations using their native Zombie timer.
