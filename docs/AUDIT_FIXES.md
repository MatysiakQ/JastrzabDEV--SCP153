# Powiazanie z audytem 30.10.2026

| ID | Zmiana |
| --- | --- |
| A01 | Wlasna instancja CustomRole; sprzatanie na migawce i wyrejestrowanie tylko jej |
| A02 | Natychmiastowa inicjalizacja po zatwierdzonym spawnie; brak opoznionego nadawania HP; ominiecie opoznionego ClearInventory z bazowego AddRole |
| A03 | Nagrody w Hurt po faktycznych obrazeniach, z powiazaniem konkretnego handlera |
| A04 | Efekty ofiary synchronicznie po trafieniu; kontrola zycia i rundy |
| A05 | Audio inicjalizowane synchronicznie, sprzatane przez komponent |
| A06 | Nowy BiteProgress przy kazdym nadaniu; powtorne give odrzucane |
| A07 | Uchwyt zadania spawnu, kontrola generacji rundy i anulowanie przy zakonczeniu |
| A08 | Wylaczona regeneracja bazowego Zombie tylko dla tej roli; IsBlocked i timestamp potwierdzonych obrazen |
| A09 | Filtr Scp049DamageHandler/Scp0492 i aktywnego wykonania natywnego ataku |
| A10 | Biezace uzgadnianie widocznosci oraz obu stron relacji observerow Mirror; brak opoznionych zadan |
| A11 | Widoczny model Zombie przy awarii lub usunieciu schematu |
| A12 | Kilka punktow, ignorowanie triggerow, podloze, wolna kapsula, brak niesprawdzonego fallbacku |
| A13 | Brak przemieszczenia ofiar po detonacji warhead |
| A14 | Sprawdzenie kolizji ID i rejestracji wlasnej instancji przed uruchomieniem logiki |
| A15 | Walidacja zakresow, wartosci skonczonych i zaleznosci parametrow |
| A16 | Mnozniki i konfigurowalne progi; ewolucje nie pogarszaja parametrow bazowych |
| A17 | Jeden cooldown serwerowy w natywnej umiejetnosci. Lokalnej predykcji animacji klienta nie mozna zmienic sama DLL serwerowa; wymaga testu w grze |
| A18 | Jawne sprzatanie wlasnych efektow i pol; bez wymuszania skali i stanow domyslnych przez bazowy RemoveRole |
| A19 | Wersja 2.2.0 w pluginie i assembly, dokladne referencje, SDK build i CI |

A12 nie stanowi gwarancji dla kazdej modyfikacji mapy; fizyka i dostepnosc wyjsc wymagaja testu. A10 wymaga sprawdzenia na klientach i przy uzywanym interest management. Zweryfikowano kompilacje i logike balansowania; nie oglaszamy pluginu przetestowanym na docelowym serwerze.
