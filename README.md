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
- Ruch ma stały krok 60 Hz i wspólny profil `Assets/Resources/Movement/ArenaMovement.asset`; aktualne parametry opisuje sekcja ruchu arenowego poniżej.
- Wall jump ma kontrolowany impuls od ściany i trzy użycia w powietrzu. Fizyczne sprawdzenie ściany zapobiega odbijaniu się od kontaktów zapamiętanych dawno temu.
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

## Ruch arenowy

Sterowanie: **Shift — dash**, **Ctrl/C — ślizg**, **Ctrl/C w powietrzu — slam**, **Spacja — skok**, także przy ścianie. Dash ma trzy odnawiające się ładunki, widoczne pod paskiem życia. Zwykły dash kosztuje jeden ładunek; dash połączony ze skokiem z ziemi zużywa kolejny i zachowuje prędkość. Podczas ślizgu regeneracja ładunków jest wstrzymana.

Bieg osiąga 16,5 m/s w około 0,1 s. Grawitacja 40 m/s² i impuls skoku 12,5 m/s dają krótki łuk skoku. Kontrola w powietrzu przekierowuje pęd, a brak wciśniętego kierunku nie hamuje automatycznie postaci. Skok przy lądowaniu i slide-jump zachowują nabraną prędkość. Ponowne wciskanie ślizgu nie mnoży jej bez końca.

Ślizg zaczyna się od 24 m/s i zachowuje wyższy pęd, stopniowo wracając do tej prędkości. Dash osiąga 42 m/s przez 0,15 s. Cały ruch ma limit pędu poziomego 72 m/s; kolizja ze ścianą usuwa zablokowaną część prędkości. Wall jump ma do trzech użyć przed lądowaniem, z impulsem 8 m/s od ściany i ograniczonym pędem wzdłuż niej.

Wciśnięcie skoku do 0,12 s przed lądowaniem zostaje zapamiętane. Po opuszczeniu krawędzi dostępne jest dodatkowe 0,1 s na skok. Slam przyspiesza w dół do 80 m/s; skok przy jego lądowaniu daje ograniczone odbicie do góry. Kamera interpoluje ruch między krokami fizyki i delikatnie poszerza FOV przy większej prędkości.

Parametry, klawisze, limit wall jumpów i reakcję kamery edytuje się w **`Assets/Resources/Movement/ArenaMovement.asset`** w Inspectorze. Profil ładuje się również w już zapisanych scenach — regenerowanie poziomów nie jest potrzebne. Po zmianie skryptów uruchom ponownie Play, aby odświeżyć cały HUD i stan ruchu.

Sprawdzenia nowego motoru i fizycznego CharacterControllera: `Validation/arena-movement-results.txt`. Raport zawiera 35 zakończonych powodzeniem sprawdzeń. Podczas testu Unity 6000.3 zgłosiło osobny błąd indeksowania wyszukiwarki edytora; pełny stos zapisano w `Validation/arena-movement-editor-issues.txt`. Testy można uruchomić w osobnej instancji Unity przez `-executeMethod RetyKotlety.Editor.ArenaMovementChecks.Run`.

## Sterowanie

| Klawisz | Działanie |
| --- | --- |
| WASD / strzałki | Ruch |
| Mysz | Rozglądanie |
| Spacja | Skok / wall jump |
| Lewy Shift | Dash |
| Lewy Ctrl / C | Ślizg na ziemi, slam w powietrzu |
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

Wcześniejsze sprawdzenia map, animacji i walki są zapisane w katalogu `Validation`. Aktualny motor ruchu ma osobny raport `arena-movement-results.txt`. Proporcje rąk porównano z obrazem `Validation/godot-weapons-reference.png`.

## Pozostałe różnice

Interfejs, efekty cząsteczkowe i oświetlenie są implementacjami Unity. Shader glitch, postprocessing Forward+ i niektóre materiały Godota nie są odwzorowane 1:1. Chrupiaca i przeciwnik strzelający są obsługiwani przez kod, lecz źródłowy poziom 001 nie umieszcza ich w początkowym rozmieszczeniu.

Importer: [oficjalny Unity glTFast](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.20/manual/index.html). Licencja oryginalnych zasobów: `LICENSE.txt`.






## Repozytorium Git

Duże modele GLB są przechowywane przez Git LFS. Klonowanie przez GitHub Desktop pobiera je automatycznie. Przy użyciu zwykłego Gita należy mieć zainstalowany Git LFS i po sklonowaniu wykonać git lfs pull. Otwórz sklonowany folder projektu bezpośrednio w Unity Hub; foldery Library, Temp i Logs są generowane lokalnie i nie należą do repozytorium.
