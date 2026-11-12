# Rety Kotlety — Unity

Otwórz **ten katalog** w Unity Hub, używając **Unity 6000.6.8f1**. Otwórz `Assets/Scenes/MainMenu.unity` i uruchom Play. Projekt używa renderera Built-in i glTFast 6.20.0.

## Plan rozwoju: bullet heaven FPS roguelite

**Kierunek: dobry movement napędza siłę buildu.** Gracz sprawnie porusza się po arenie, dzięki temu skuteczniej zabija, zbiera ulepszenia i łączy ruch z coraz większą destrukcją.

Punktem odniesienia jest [Megabonk](https://store.steampowered.com/app/3405340/Megabonk/): narastające hordy, składanie mocnych kombinacji i szybkie decyzje o rozwoju postaci. Wyróżnikiem Rety Kotlety ma być prowadzenie walki przez przestrzeń w perspektywie pierwszej osoby.

**Status:** poniżej zapisano docelowy kierunek i proponowany zakres prac. Opis planowanych systemów nie oznacza, że są już zaimplementowane. Parametry prototypu są punktem wyjścia do testów.

### Obecna baza i brakujące systemy

Mamy dash, slide, wall jump, slam, hak, jetpack, bronie, przeciwników, fale i HUD. Potrzebujemy przede wszystkim struktury pojedynczego podejścia, systemu ulepszeń oraz przeciwników i map zaprojektowanych pod movement.

Obecny system zamykania aren i czyszczenia kolejnych fal wymaga dostosowania do nowej pętli. Samo zwiększenie liczby przeciwników nie stworzy bullet heaven.

### 1. Podstawowa pętla podejścia

Pierwszy prototyp: **10-minutowy run na jednej ręcznie zbudowanej mapie**.

`Walka i przemieszczanie → zbieranie XP → wybór ulepszenia → trudniejsze hordy → elita → boss → wynik i odblokowania`

Gracz powinien mieć powody do zmiany miejsca: skrzynie, krótkie wydarzenia, zasoby i korzystne pozycje. Bieganie po obwodzie mapy powinno być mniej opłacalne niż aktywne korzystanie z terenu.

### 2. Movement jako część walki

Na początek trzy interakcje działające także bez ulepszeń:

| Ruch | Podstawowe zastosowanie | Przykładowe ulepszenie |
| --- | --- | --- |
| Slide | Przejście pod ostrzałem i wejście między przeciwników | Zostawia pas gorącego tłuszczu |
| Slam | Obrażenia obszarowe i rozepchnięcie hordy | Zabici przeciwnicy wyrzucają odbijające się kotlety |
| Hak | Szybkie wejście lub wyjście z zagrożenia | Przyciągnięty wróg po zderzeniu wybucha |

Dash i wall jump początkowo pozostają niezawodnymi narzędziami przemieszczania. Ich efekty bojowe mogą pojawiać się w buildach.

Unikamy stałej zasady „im szybciej biegniesz, tym większe obrażenia”, która mogłaby premiować bezmyślne krążenie. Zamiast tego testujemy krótkie, czytelne okazje:

- Po slajdzie następny strzał przebija przeciwników.
- Po odbiciu od ściany gracz dostaje dodatkową salwę.
- Slam odnawia część cooldownu haka.

Efekty potrzebują ograniczeń zapobiegających nieskończonym pętlom. Podstawowy movement ma być przyjemny od pierwszej sekundy; ulepszenia zmieniają jego zastosowanie, zamiast dopiero odblokowywać sprawne sterowanie.

### 3. Broń ręczna i automatyczne efekty buildu

Punkt wyjścia: **ręcznie sterowana broń główna plus automatyczne efekty buildu**.

Gracz celuje schabowym lub chrupiącą albo walczy patelnią. Dodatkowe bronie i pasywki obsługują część hordy, np. orbitujące pierogi, odbijające się pociski, wybuchy po zabójstwie i efekty ruchowe.

Pozostawiamy satysfakcję ze strzelania oraz przestrzeń na movement. Przy dużej liczbie wrogów wymaganie precyzyjnego trafiania każdego z osobna mogłoby przeciążyć gracza.

### 4. Rozwój postaci podczas podejścia

Do zbudowania:

- XP wypadające z przeciwników, poziomy i wybór jednego z trzech ulepszeń.
- Wspólne statystyki: obrażenia, szybkostrzelność, liczba pocisków, obszar, czas działania, cooldown i zasięg zbierania.
- Limity broni i pasywek, żeby wybory tworzyły różne buildy.
- Ulepszenia zmieniające zachowanie, obok prostych bonusów procentowych.

Przykładowa synergia:

`Chrupiaca strzela szerzej → pociski odbijają się → slide daje dodatkową salwę → zabójstwa salwą odnawiają dash`

Definicje broni, ulepszeń i przeciwników przechowujemy w `ScriptableObject`. Obrażenia i zabójstwa powinny przekazywać informacje o źródle trafienia. Efekty „po zabójstwie” potrzebują jasno określonych zasad uruchamiania kolejnych efektów, żeby kombinacje pozostawały kontrolowalne.

### 5. Przeciwnicy i sterowanie presją

Na początek cztery role:

| Przeciwnik | Do czego zmusza |
| --- | --- |
| Zwykły ścigający | Szukanie drogi przez hordę |
| Strzelający z zapowiedzią ataku | Dash, slide i zmianę kierunku |
| Ciężki blokujący przejścia | Hak, obejście lub zmianę wysokości |
| Latający | Kontrolowanie przestrzeni ponad ziemią |

W FPS-ie szczególnie potrzebne są sygnały ataków spoza ekranu: kierunkowy dźwięk, wskaźnik zagrożenia i czas na reakcję. Gracz nie widzi całej hordy jak przy kamerze z góry.

Zamiast samych stałych fal tworzymy sterownik starcia dobierający przeciwników według czasu, budżetu trudności i sytuacji na mapie. Ma dawkować presję i zapewniać krótkie momenty oddechu.

### 6. Mapa pod walkę ruchem

Pierwsza arena powinna zapewniać:

- Połączone trasy na kilku wysokościach.
- Ściany, od których warto się odbijać.
- Czytelne miejsca zaczepienia haka.
- Przejścia do slajdu i miejsca na slam w grupę przeciwników.
- Kilka tras ucieczki z każdej większej strefy.

Sprawdzamy, czy przeciwnicy wywierają presję także wtedy, gdy gracz wejdzie na dach. Stanie poza ich zasięgiem nie powinno stać się najlepszą strategią.

### Kolejność realizacji i kryteria ukończenia

| Etap | Zakres | Kiedy uznajemy go za gotowy |
| --- | --- | --- |
| 1. Pętla podejścia | Start, timer, hordy, XP, wybór ulepszeń, śmierć i wynik | Da się rozegrać pełne 10 minut |
| 2. Movement w walce | Slide, slam i hak z efektami bojowymi | Gracz świadomie używa ich do zabijania |
| 3. Buildy | 3 bronie, około 12 ulepszeń, kilka wyraźnych synergii | Kolejne podejścia wymagają różnych decyzji |
| 4. Arena i przeciwnicy | Jedna dopracowana mapa, 4 role wrogów, elita i boss | Nie ma jednej bezpiecznej strategii |
| 5. Wydajność i czytelność | Pooling, limity efektów, optymalizacja AI, sygnały zagrożeń | Docelowa horda działa płynnie na wybranym sprzęcie |
| 6. Roguelite | Zapis, odblokowania i kolejne warianty zawartości | Jest powód do następnego podejścia |

Wydajność mierzymy od pierwszego etapu, mimo że osobny etap poświęcamy jej dopracowaniu. Obecne tworzenie i niszczenie pocisków oraz operacje fizyczne trzeba sprawdzić przy docelowej liczbie wrogów. Pooling będzie jednym z pierwszych usprawnień. Docelowy sprzęt, liczbę jednoczesnych przeciwników i budżet klatki ustalamy przed oceną wydajności.

### Zakres pierwszego grywalnego prototypu

- [ ] Jedna arena i 10-minutowe podejście.
- [ ] Obecne trzy bronie: patelnia, schabowy i chrupiaca.
- [ ] XP, poziomy i wybór jednego z trzech ulepszeń.
- [ ] Około 12 ulepszeń i kilka wyraźnych synergii.
- [ ] Trzy sposoby zabijania movementem: slide, slam i hak.
- [ ] Narastająca presja przeciwników oraz czytelny koniec podejścia i wynik.
- [ ] Testy czytelności walki i wydajności przy założonej liczbie wrogów.

Najpierw sprawdzamy, czy połączenie ruchu, hord i budowania postaci jest przyjemne. Dopiero potem rozszerzamy liczbę map, przeciwników i pozostałej zawartości. Elita, boss oraz trwałe odblokowania wchodzą zgodnie z etapami powyżej; nie muszą blokować najwcześniejszego testu podstawowej pętli.

## Pierwsza arena prototypowa: MovementArena

Otwórz `Assets/Scenes/MovementArena.unity` i uruchom Play albo wybierz **Arena movementu** w menu głównym. To sandbox układu mapy i presji przeciwników; pełny run, XP, ulepszenia oraz pozostałe bojowe efekty movementu pozostają kolejnymi zadaniami z planu. Slam ma już obrażenia obszarowe i efekt lądowania.

- Arena ma 112 × 112 m, otwarty środek, podesty na 4 m i tarasy na 8 m.
- Rampy łączą ziemię z wyższymi trasami; pomosty zapewniają miejsca do zeskoków i slamu.
- Pomarańczowe tunele mają prześwit 1,35 m do testowania slajdu; pary ścian służą do wall jumpów.
- Żółte cele wysoko nad areną służą do haka; niebieskozielone powierzchnie oznaczają trasy.
- Gracz dostaje schabowego i chrupiącą od startu. W środku stoją cztery manekiny z dużą pulą HP.
- Jetpack leży na południowym górnym tarasie, pieróg na wschodnim podeście. Podnoszenie: **F**.
- **T** włącza lub zatrzymuje dopływ przeciwników (istniejący wrogowie pozostają). Limit: 18 aktywnych, do trzech nowych co 5 sekund; mieszanka melee i strzelających.
- Slam zadaje **12 obrażeń za metr spadku od aktywacji**, maksymalnie **90** (od 7,5 m). Promień wyliczany jest według `min(maksymalny promień, promień bazowy + przyrost × wysokość spadku)`. Promień wynosi od **1 do 5 m**, z przyrostem `4 / 7,5 m` za metr spadku. Pełne 5 m osiąga po 7,5 m spadku, razem z limitem 90 obrażeń. Wysokość mierzona jest przy stopach, więc kucanie nie zwiększa obrażeń. Każdy wróg dostaje jedno trafienie; przeszkody blokują falę. Lądowaniu towarzyszą złoty pierścień i kwadratowe odłamki; zasięg pierścienia odpowiada promieniowi obrażeń. Fala osiąga pełny promień w 0,12 s i pozostaje widoczna do 0,65 s; skala gracza wynosi 1:1 przy wysokości 2 m. Parametry bazowe są w `Assets/Resources/Combat/Slam.asset`: promień początkowy, przyrost promienia na metr, maksymalny promień, obrażenia na metr i maksymalne obrażenia. Gracz ma własną kopię statystyk; system ulepszeń nie jest jeszcze zaimplementowany.
- Śmierć przeładowuje tę arenę. Presja jest domyślnie wyłączona, żeby można było najpierw sprawdzić trasy.

Geometria sceny jest edytowalna w Hierarchy. W wersji początkowej `ArenaBootstrap` tworzy gracza, aktorów i nawigację przy wejściu w Play, korzystając z istniejącej geometrii.

**Rety Kotlety → Prototyp → Przygotuj arene movementu** generuje pełną scenę z aktorami i zapisanym NavMeshem. Polecenie zastępuje zawartość MovementArena i jej wygenerowane assety. Źródłowy układ jest w `Assets/Resources/Levels/MovementArena.json`; zapisuj tam zmiany, które mają przetrwać ponowne generowanie.

Alternatywny generator bez edytora: `python3 Validation/build_arena_scene.py` — odtwarza geometrię sceny i materiały z tego samego JSON-a, zachowując GUID-y istniejących plików. Również zastępuje ręczne zmiany w tej scenie.

Kontrola statyczna: `python3 Validation/check_arena_layout.py`. Kontrola fizyki i połączeń NavMesh w Unity: **Rety Kotlety → Prototyp → Sprawdz arene movementu**, po pełnym wygenerowaniu sceny. Raport: `Validation/Reports/movement-arena-layout.txt`.

## Skrzynie i ulepszenia

Na MovementArena przy starcie stoją trzy skrzynki (x: `-4`, `4`, `8`; y: `0,7`; z: `-43`). Podejdź, wyceluj i naciśnij **F**. Panel otwierania zatrzymuje rozgrywkę i przewija karty przez 3,2 s. Każda skrzynia losuje jedno z jeszcze niezdobytych ulepszeń. Karty pokazują aktualną pulę; zatrzymanie wskazuje faktycznie przyznawaną nagrodę. Trzy skrzynie pozwalają zdobyć wszystkie trzy efekty bez powtórek. Po zakończeniu kliknij **Odbierz ulepszenie** albo naciśnij **Esc**. Skrzynia jest jednorazowa, a ulepszenie obowiązuje do restartu sceny. Przed użyciem pokazuje model zamknięty, a po odebraniu nagrody — otwarty, z mniejszym, pikselowym dymem z małych czarnych kwadratów wydobywających się ze środka. Po 4 sekundach rozgrywki skrzynia znika; resztki dymu łagodnie wygasają. Pauza zatrzymuje ten czas. Modele FBX i tekstury znajdują się w `Assets/Resources/Props/UpgradeChest/Closed` oraz `Open`; materiały są we wspólnym folderze `Materials`. Obie wersje są dopasowywane według podstawy, z zachowaniem kolizji zamkniętego korpusu. Otwarty model zachowuje oryginalne tekstury krawędzi, ścian i wieka. Tylko dno ma czarny materiał, a gęsta warstwa małych kwadratów dymu wydobywa się tuż pod górną krawędzią korpusu i zakrywa wnętrze. Poprzedni błędny model zachowano poza importem Unity w `Documentation/Archive/RejectedChestModel`.

Po ulepszeniu schabowy trafia początkowego przeciwnika i automatycznie leci do **maksymalnie trzech kolejnych, różnych wrogów**. Każdy następny cel jest najbliższym żywym, widocznym wrogiem w promieniu **14 m od pocisku**. Pocisk koryguje lot w stronę wybranego celu i zachowuje obrażenia. Nie wraca do trafionych wrogów; brak celu lub przeszkoda kończą łańcuch. Chrupiaca i pomidory nie otrzymują tego efektu. Dotychczasowe odbicia schabowego od otoczenia pozostają osobną mechaniką.

| ID | Ulepszenie | Działanie |
| --- | --- | --- |
| `1.1` | Schabowy: Rykoszet | Do trzech dodatkowych, różnych wrogów; zasięg 14 m |
| `2.1` | Chrupiaca: Piercing | Pociski przebijają wrogów, każdy pocisk trafia danego wroga tylko raz; ściany blokują |
| `2.2` | Chrupiaca: Krwawienie | 5 obrażeń co 0,25 s przez 3 s: 12 ticków, 60 obrażeń. Kolejne trafienia odświeżają czas bez mnożenia stosów i bez opóźniania następnego ticka |

Chrupiaca strzela trzema pociskami w jednym poziomym wachlarzu względem kamery: lewy (-6°), środkowy (0°), prawy (+6°), bez losowego rozrzutu pionowego.

Piercing i krwawienie łączą się: każdy przebity przeciwnik otrzymuje bleed. Efekty chrupiącej obejmują wszystkie trzy pociski salwy, ale bleed pozostaje pojedynczym statusem na przeciwniku. Pauza zatrzymuje krwawienie.

Definicje nagród: `Assets/Resources/Upgrades` (ID, opis i parametry efektów). `PlayerUpgrades` przechowuje stan pojedynczego podejścia bez modyfikowania wspólnego assetu. Kolejne nagrody można definiować jako osobne `UpgradeDefinition`; nowe zachowania wymagają dodania obsługi ich efektu.

Kontrola samego harmonogramu krwawienia: `Validation/BleedChecks.cs` (kod C# poza Assets). Sprawdza ticki, długie klatki, odświeżanie i brak stackowania.

Test integracyjny w Unity: `-executeMethod RetyKotlety.Editor.UpgradeChecks.Run`. Sprawdza skrzynię, pauzę, przyznanie nagrody, brak przypadkowego stackowania, limit czterech trafionych wrogów i blokowanie łańcucha przez osłonę. Wyniki: `Validation/Reports/upgrade-results.txt` i `upgrade-errors.txt`.

## Konsola debugowania

**§ / tylda (`~`, klawisz BackQuote)** otwiera konsolę i zatrzymuje grę. **Esc** lub ten sam klawisz zamyka konsolę. Nie nakłada się na inne otwarte menu. Komendę zatwierdza **Enter**, **Enter numeryczny** lub przycisk **Wyślij**.

- `give 1.1` — rykoszet schabowego.
- `give 2.1` — piercing chrupiącej.
- `give 2.2` — krwawienie chrupiącej.
- `list` — lista dostępnych ID i nazw.
- `help` — pomoc.
- `clear` — wyczyszczenie konsoli.

`give` przyznaje upgrade na bieżące podejście i wyposaża powiązaną broń. Powtórne `give` tego samego ID nie stackuje efektu. Build pod Tab pokazuje nagrody otrzymane przez konsolę i skrzynie. ID `1.x` dotyczą schabowego, `2.x` chrupiącej.

## Podglad buildu

**Tab** otwiera i zamyka panel aktualnego podejścia. Panel pokazuje posiadane bronie, broń w dłoni, zdobyte ulepszenia z opisami, jetpack, paliwo, pierogi oraz parametry slamu. Lista ulepszeń korzysta z faktycznie zdobytych definicji i przewija się przy większej liczbie nagród. **Esc** lub przycisk **Zamknij** również wraca do gry. Otwarty panel zatrzymuje rozgrywkę; nie nakłada się na pauzę ani otwieranie skrzynki.

Jetpack został przeniesiony z Tab na **G**. Build resetuje się razem z podejściem.

## Struktura projektu

| Katalog | Zawartość |
| --- | --- |
| `Assets/Scripts/Player` | Gracz, hak i kiełbasiana linka |
| `Assets/Scripts/Movement` | Motor ruchu, obliczenia i profil ustawień |
| `Assets/Scripts/Combat` | Pociski, rozrzut i prezentacja broni w dłoni |
| `Assets/Scripts/Upgrades` | Definicje ulepszeń, stan gracza, skrzynia i panel otwierania |
| `Assets/Scripts/Enemies` | Zachowanie przeciwników |
| `Assets/Scripts/World` | Sesja gry, areny, fale, znajdźki i wyjścia |
| `Assets/Scripts/UI` | HUD i liczby obrażeń |
| `Assets/Scripts/Effects` | Efekty trafień i jetpacka |
| `Assets/Scripts/Migration` | Dane migracji i budowanie świata |
| `Assets/Editor/Migration` | Import modeli i generowanie scen |
| `Assets/Editor/Validation` | Kontrole ruchu, walki i poziomów w edytorze |
| `Assets/Resources` | Modele, dane, dźwięki, UI, profil ruchu i shadery ładowane w grze |
| `Assets/Scenes` | MainMenu, Level001, TestLevel i MovementArena |
| `Assets/Generated` | Wygenerowane siatki kolizji i nawigacja |
| `Assets/_Recovery` | Zachowana kopia odzyskiwania sceny |
| `Validation/Reports` | Zapisane raporty i wyniki kontroli |
| `Validation/Screenshots` | Zrzuty z gry oraz referencje |
| `Documentation` | Historia migracji i szczegóły przeniesienia z Godota |

Przy przenoszeniu assetów zachowuj ich pliki `.meta` lub używaj okna Project w Unity. Nazwy i ścieżki wewnątrz `Resources` są używane przez kod.

Polecenia **Rety Kotlety → Przygotuj edytowalne sceny** i **Przygotuj poziom testowy** odtwarzają sceny i wygenerowane assety, zastępując ręczne zmiany w tych plikach. Zwykłe uruchomienie gry nie wymaga generowania scen.

## Sterowanie

| Klawisz | Działanie |
| --- | --- |
| WASD / strzałki, mysz | Ruch i rozglądanie |
| Spacja | Skok / wall jump |
| Lewy Shift | Dash |
| Lewy Ctrl / C | Ślizg / slam |
| LPM / PPM | Atak / blok patelnią |
| V, 1, 2, kółko myszy | Wybór broni |
| F / E | Podniesienie przedmiotu / zjedzenie pieroga |
| Q / R | Wyrzucenie lub zwolnienie haka / skracanie linki |
| Tab | Otworz / zamknij podglad buildu (pauza) |
| G | Jetpack po podniesieniu |
| Esc | Pauza |

## Sprawdzanie projektu

`python3 Validation/verify_project.py` sprawdza pliki `.meta`, unikalność GUID i powiązania skryptów w scenach. Nie uruchamia gry.

`python3 Validation/verify_project.py --compile` dodatkowo kompiluje kod gry i edytora przy użyciu lokalnego Unity i konfiguracji z `Library/Bee`. Najpierw otwórz projekt w Unity, aby utworzyć tę konfigurację. Na macOS instalacja jest wykrywana z Unity Hub; inną lokalizację można podać przez `--editor-root`.

Kontrole Play Mode można uruchomić przez `-executeMethod` z jedną z metod:

- `RetyKotlety.Editor.ArenaMovementChecks.Run`
- `RetyKotlety.Editor.CombatHudChecks.Run`
- `RetyKotlety.Editor.TestLevelChecks.Run`
- `RetyKotlety.Editor.CorrectionChecks.Run`

Używaj osobnej instancji edytora przy zamkniętym projekcie i bez `-quit`: kontrole same kończą proces. Test poziomu odtwarza TestLevel, a kontrola walki może regenerować ikonę patelni. Wyniki trafiają do `Validation/Reports`, obrazy do `Validation/Screenshots`.

`Validation/Verify-Migration.ps1` zawiera dodatkowe kontrole historycznego eksportu z Godota. Historia i wcześniejsze ograniczenia: [Documentation/Migration.md](Documentation/Migration.md). Zapisane raporty są wynikami wcześniejszych uruchomień, a nie potwierdzeniem bieżącego stanu gry.

## Repozytorium

Modele GLB są obsługiwane przez Git LFS. Po klonowaniu pobierz je przez `git lfs pull`. `Library`, `Temp`, `Logs`, `UserSettings` i metadane macOS są lokalne i ignorowane przez Git.
