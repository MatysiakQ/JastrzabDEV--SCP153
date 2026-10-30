# Weryfikacja 2.2.0

## Automatyczne sprawdzenie

Tests/SCP153.Tests.csproj uruchamia 34 przypadki na tych samych klasach BalanceSettings i BiteProgress, ktorych uzywa plugin: krzywa 400/+50/2000, regeneracja, blokady, deduplikacja potwierdzen, rozne zycia, progi, mnozniki i walidacja konfiguracji.

To testy logiki. Nie symuluja Unity, EXILED, Harmony, Mirror ani samej fizyki.

Build pluginu sprawdza zgodnosc kompilacyjna z DLL w REFERENCES. W referencjach gry brakuje w oryginalnym repo Pooling.dll, wymaganego przez typ ZombieRole; dodana referencja pochodzi z archiwum wskazanego przez oficjalny workflow EXILED: https://exmod-team.github.io/SL-References/Dev.zip.
SHA-256 dodanego Pooling.dll: 018ac2b5a25ca2b0db71902a2aa115fd8993be66709118db92a80a168ccf5c03.
Jest to zaleznosc kompilacyjna, nie plik do instalowania obok pluginu.

## Test serwerowy przed stabilnym wydaniem

1. Na serwerze z drugim CustomRole przeladuj tylko SCP153. Druga rola i jej gracze musza pozostac bez zmian.
2. Nadaj, usun i ponownie nadaj SCP153; nowe nadanie ma miec 0 ugryzien i 400 HS. Powtorne give na aktywnej roli ma zostac odrzucone.
3. Anuluj zmiane roli na Zombie przez inny plugin. Nie moze powstac SCP153 ani zostac nadane jego HP/HS.
4. Anuluj atak przed i po handlerze SCP153, rowniez przez LabAPI. Nie moze wzrosnac licznik, HP ani HS i nie moze nastapic teleport.
5. Sprawdz trafienie pochloniete w calosci przez HS/AHP oraz trafienie smiertelne. Jedna proba natywnego ataku nie moze dac kilku stosow.
6. Zmien role ofiary podczas obslugi obrazen; jej nowe zycie nie moze otrzymac efektow ugryzienia.
7. Zakoncz runde lub wylacz plugin przed 5. sekunda. Stare zadanie spawnu nie moze sie wykonac.
8. Sprawdz podstawowe i koncowe HS: 15/30 na sekunde, po 5 s bez przyjetych obrazen, do wypracowanego maksimum. Aktywuj zdolnosc 049 i blokady HS/SCP-244.
9. Szybko przelaczaj obserwacje A/B, odrodz obserwatora, dolacz w trakcie rundy i przeladuj schemat. Zywy przeciwnik musi widziec model.
10. Usun schemat lub ustaw zla nazwe: powinien byc widoczny zwykly Zombie. Usun audio: rola nadal dziala.
11. Sprawdz wszystkie warianty korytarzy, blokady drzwi i przeszkody. Gdy brakuje miejsca, gracz pozostaje na miejscu. Po dekontaminacji nie trafia do LCZ; po warhead nie jest przenoszony pod ziemie ani do Pocket Dimension.
12. Sprawdz wejscie do Pocket Dimension i upuszczanie przedmiotu przy aktywnych pluginach blokujacych te operacje.
13. Testuj klikniecia i przytrzymanie ataku przed/po 7 stosach. Serwerowy cooldown jest zmieniony, ale klient przewiduje animacje wedlug wlasnego timera 1.3 s; zdalne animacje i UI nie sa dowodem czasu przyjetych trafien.
14. Wykonaj kilka rund dla 6, 8, 12 i 14 osob, zapisujac czas do 10 stosow i przezywalnosc. Nie traktuj samych testow jednostkowych jako potwierdzenia balansu.
15. Sprawdz ustawienia removal_kills_player i wspoldzielone efekty/nazwy z innym pluginem.

Nie wykonano testow klienta ani serwera w srodowisku tworzenia tej poprawki. Kompilacja i testy logiki maja osobny, ograniczony zakres.
