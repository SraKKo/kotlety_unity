# Rety Kotlety — Unity 6

Otwórz folder `Unity` w Unity Hub, następnie `Assets/Scenes/MainMenu.unity` i **Play**. Docelowy edytor: **6000.3.13f1**, renderer Built-in. Pakiet glTFast 6.20.0 importuje modele. Oryginał Godota znajduje się w katalogu nadrzędnym.

Sceny są już przygotowane z pełną hierarchią obiektów. Narzędzie **Rety Kotlety → Przygotuj edytowalne sceny** odtwarza wszystkie sceny i pliki w `Assets/Generated`; zastępuje własne zmiany w tych plikach. Zwykłe uruchomienie gry nie wymaga ponownego generowania.

## Poprawki skali, animacji i ruchu

- Usunięto automatyczne normalizowanie rozmiaru ręki, patelni i przeciwników. Używane są kompletne sceny Godota z ich wewnętrznymi transformacjami.
- Przywrócono kapsułę gracza (wysokość 2 m, promień 0,5 m, środek w punkcie gracza), oryginalny punkt kamery i pozycje rąk.
- Osobna kamera broni ma FOV 110°, a kamera świata około 86,36°, jak w źródłowym projekcie. Broń nie znika wewnątrz ścian; modele broni są na warstwie 11.
- Ręka korzysta z gotowej biblioteki `lapka.tscn`: `melee_idle2`, `melee_attack2`, `melee_block`, `throw_schabowy`, `throw_chrupiaca`. Nie zastępuje ich proceduralne obracanie całego modelu.
- Trafienie patelnią występuje w czasie 0,8194 s klipu; wyrzut schabowego w 0,28 s, chrupiącej w 0,4 s. Prędkość odtwarzania 1,465 pochodzi ze źródłowej sceny. Animacje rzutu wracają do pozycji wyjściowej przez odtwarzanie wstecz.
- Lewa ręka korzysta z oryginalnej animacji haka. Przy zatrzymaniu animacje zachowują końcową pozę (ClampForever), zamiast wracać do pozy bazowej. Zaczepienie następuje po fazie wyrzutu, a zwolnienie haka cofa animację.
- Linka składa się z siatek **`kielba.tscn`** z oryginalną orientacją i skalą. Segmenty wyrastają od oryginalnego `pento_point`; nie jest to LineRenderer.
- Znajdźki i pociski zachowują wewnętrzną skalę swoich scen. Surowy model schabowego nie zastępuje już jego mniejszej wersji do podnoszenia/rzucania.
- Ruch ma stały krok 60 Hz, przyspieszenie na ziemi 20 m/s², w powietrzu 5 m/s², tarcie 15/1 m/s² i limit zwykłego biegu 20 m/s.
- Wall jump ma impuls **6 m/s od ściany**, zamiast 49,875 m/s, oraz limit pędu wzdłuż ściany 8 m/s. Kontakt wygasa po 0,08 s, cooldown wynosi 0,3 s, a pierwsze 0,15 s po odbiciu chroni impuls przed natychmiastowym anulowaniem przez sterowanie. Sama obecność ściany we wcześniejszej klatce nie wystarcza do kolejnego odbicia.
- Ślizg zmienia kapsułę z zachowaniem punktu podparcia i sprawdza prześwit przed wyprostowaniem. Kamera wraca do poziomego horyzontu.

## Poziom testowy z Godota

W menu głównym wybierz **Poziom testowy** albo otwórz `Assets/Scenes/TestLevel.unity` i naciśnij Play. To przeniesiona scena `scenes/node_3d.tscn`: tor ze skrzyń i ramp, dwie obrotowe platformy, fizyczna piłka, manekin, schabowy i chrupiąca, jetpack, pieróg oraz dwóch przeciwników wręcz i jeden strzelający. Zachowano oryginalne rozmieszczenie i skale. Po śmierci wczytuje się ponownie ta sama mapa.

Eksporter tej mapy: `../MigrationTools/export_test_level.gd`. W Unity polecenie **Rety Kotlety → Przygotuj poziom testowy** odtwarza wyłącznie nową scenę i jej pliki w `Assets/Generated/TestLevel`, zachowując pozostałe sceny. Wyniki testu: `Validation/test-level-results.txt`; podgląd: `Validation/test-level.png`.

## Chrupiąca, trafienia i HUD

Chrupiąca wyrzuca **trzy pociski jednocześnie**, każdy po 20 obrażeń, z niewielkim rozrzutem. Trafienie trzema daje 60 obrażeń. Moment wyrzutu nadal jest zsynchronizowany z oryginalną animacją.

Po trafieniu pojawia się czerwony, obrysowany licznik obrażeń w pikselowym foncie. Pokazuje rzeczywistą utratę zdrowia i sumuje trafienia z jednej potrójnej salwy; unosi się i znika. Obrażenia patelni, pocisków i odbitych strzałów przechodzą przez ten sam mechanizm.

Efekty mają odrębne wyglądy: odłamki teksturowanej patelni, okruchy pocisków i iskry odbić, oryginalne obrazki BUM przy śmierci oraz czerwono-żółty wylot jetpacka. Jednorazowe efekty wygasają i usuwają swoje obiekty.

HUD ponownie używa układu przestrzennego Godota (15° obrotu panelu, 0,01 jednostki/piksel i 4,14258 jednostki od kamery broni), oryginalnego paska życia, grafik schabowego/chrupiącej, grafiki jetpacka z wypełnieniem paliwa i oryginalnego celownika. Ikona patelni została wyrenderowana z jej oryginalnego modelu. HUD działa w obu mapach, bez ponownego generowania scen.

Testy strzelania, sumowania obrażeń, obrazków broni, paliwa i efektów: `Validation/combat-hud-results.txt` (20 sprawdzeń). Podglądy z Unity: `Validation/hud-chrupiaca.png` i `Validation/hud-schabowy.png`.

## Sterowanie

| Klawisz | Działanie |
| --- | --- |
| WASD / strzałki | Ruch |
| Mysz | Rozglądanie |
| Spacja | Skok / wall jump |
| Lewy Shift | Ślizg |
| F | Podniesienie przedmiotu |
| E | Zjedzenie pieroga (+20 HP) |
| Q | Wyrzucenie / zwolnienie haka |
| R | Skracanie linki |
| Tab | Jetpack po podniesieniu |
| LPM | Atak / strzał |
| PPM | Blok patelnią |
| V | Patelnia |
| 1 / 2 / kółko | Zmiana dostępnej broni |
| Esc | Pauza |

## Sprawdzanie

- `Validation/report.json`: geometria i animacje eksportów GLB, parametry źródłowe oraz kompilacja C#.
- `Validation/movement-results.txt`: testy matematyki ruchu, limitu wall jumpa, świeżości kontaktu i różnych kroków symulacji.
- `Validation/playmode-results.txt` i `playmode-errors.txt`: wyniki uruchomienia rzeczywistej sceny w Unity.
- `Validation/unity-wall-jump.txt`: zmierzony zasięg odbicia bez sterowania w fizyce CharacterController.
- `Validation/corrected-view.png` oraz `corrected-hook.png`: obrazy wyrenderowane przez kamery Unity.

Statyczna kontrola: `./Validation/Verify-Migration.ps1`. Test w Unity można uruchomić w trybie batch przez `-executeMethod RetyKotlety.Editor.CorrectionChecks.Run` (bez `-quit`, test zamyka edytor po zakończeniu). Test otwiera Level001 i nie zapisuje zmian powstałych w Play Mode.

Eksport źródłowy znajduje się w `../MigrationTools/export_unity.gd` oraz `export_presentation.gd`; drugi plik przenosi kompletne sceny rąk, broni, znajdziek i przeciwników. Podczas zwykłej pracy w Unity Godot nie jest potrzebny.

Ostatnia kontrola: 31 sprawdzeń w Play Mode i 10 testów matematyki ruchu przeszło bez błędów. Odbicie od ściany bez dalszego sterowania miało w CharacterController około 8,0 m zasięgu. Proporcje rąk porównano również z obrazem wyrenderowanym bezpośrednio w Godot (`Validation/godot-weapons-reference.png`).

## Pozostałe różnice

Interfejs, efekty cząsteczkowe i oświetlenie są implementacjami Unity. Shader glitch, postprocessing Forward+ i niektóre materiały Godota nie są odwzorowane 1:1. Chrupiaca i przeciwnik strzelający są obsługiwani przez kod, lecz źródłowy poziom 001 nie umieszcza ich w początkowym rozmieszczeniu.

Importer: [oficjalny Unity glTFast](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.20/manual/index.html). Licencja oryginalnych zasobów: `LICENSE.txt`.






## Repozytorium Git

Duże modele GLB są przechowywane przez Git LFS. Klonowanie przez GitHub Desktop pobiera je automatycznie. Przy użyciu zwykłego Gita należy mieć zainstalowany Git LFS i po sklonowaniu wykonać git lfs pull. Otwórz sklonowany folder projektu bezpośrednio w Unity Hub; foldery Library, Temp i Logs są generowane lokalnie i nie należą do repozytorium.

