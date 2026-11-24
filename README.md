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
- Gracz dostaje schabowego, chrupiącą i mielonego od startu. W środku stoją cztery manekiny z dużą pulą HP.
- Jetpack leży na południowym górnym tarasie, pieróg na wschodnim podeście. Podnoszenie: **F**.
- **T** włącza lub zatrzymuje dopływ przeciwników (istniejący wrogowie pozostają). Limit: 18 aktywnych, do trzech nowych co 5 sekund; mieszanka melee i strzelających.
- Slam zadaje **12 obrażeń za metr spadku od aktywacji**, maksymalnie **90** (od 7,5 m). Promień wyliczany jest według `min(maksymalny promień, promień bazowy + przyrost × wysokość spadku)`. Promień wynosi od **1 do 5 m**, z przyrostem `4 / 7,5 m` za metr spadku. Pełne 5 m osiąga po 7,5 m spadku, razem z limitem 90 obrażeń. Wysokość mierzona jest przy stopach, więc kucanie nie zwiększa obrażeń. Każdy wróg dostaje jedno trafienie; przeszkody blokują falę. Lądowaniu towarzyszą złoty pierścień i kwadratowe odłamki; zasięg pierścienia odpowiada promieniowi obrażeń. Fala osiąga pełny promień w 0,12 s i pozostaje widoczna do 0,65 s; skala gracza wynosi 1:1 przy wysokości 2 m. Slam z wysokości **co najmniej 3 m** dodatkowo podrzuca żywych przeciwników do góry. Siła rośnie od 6 do 9 m/s, osiągając maksimum przy 7,5 m spadku. Wróg podczas lotu nie atakuje i po lądowaniu wraca do nawigacji. Każde lądowanie daje lekki, wygasający screen shake pozycji kamery (do 3,5 cm przez 0,18 s), bez zmiany celowania. Parametry podrzutu i shake są w tym samym profilu. Parametry bazowe są w `Assets/Resources/Combat/Slam.asset`: promień początkowy, przyrost promienia na metr, maksymalny promień, obrażenia na metr i maksymalne obrażenia. Gracz ma własną kopię statystyk; system ulepszeń nie jest jeszcze zaimplementowany.
- Śmierć przeładowuje tę arenę. Presja jest domyślnie wyłączona, żeby można było najpierw sprawdzić trasy.

Geometria sceny jest edytowalna w Hierarchy. W wersji początkowej `ArenaBootstrap` tworzy gracza, aktorów i nawigację przy wejściu w Play, korzystając z istniejącej geometrii.

**Rety Kotlety → Prototyp → Przygotuj arene movementu** generuje pełną scenę z aktorami i zapisanym NavMeshem. Polecenie zastępuje zawartość MovementArena i jej wygenerowane assety. Źródłowy układ jest w `Assets/Resources/Levels/MovementArena.json`; zapisuj tam zmiany, które mają przetrwać ponowne generowanie.

Alternatywny generator bez edytora: `python3 Validation/build_arena_scene.py` — odtwarza geometrię sceny i materiały z tego samego JSON-a, zachowując GUID-y istniejących plików. Również zastępuje ręczne zmiany w tej scenie.

Kontrola statyczna: `python3 Validation/check_arena_layout.py`. Kontrola fizyki i połączeń NavMesh w Unity: **Rety Kotlety → Prototyp → Sprawdz arene movementu**, po pełnym wygenerowaniu sceny. Raport: `Validation/Reports/movement-arena-layout.txt`.

## Mielony - bron prototypowa

Na MovementArena mielony jest dostępny od startu pod **3** i kółkiem myszy. W dłoni i w locie ma placeholder zwykłej brązowej kuli. Używa tymczasowo istniejącej ikony mielonego. **LPM** rzuca pocisk łukiem; po trafieniu w podłoże powstają żółty rozprysk i plama tłuszczu.

Domyślnie plama ma **2,5 m promienia**, trwa **5 s** i zadaje **5 obrażeń co 0,25 s** przeciwnikom wewnątrz. Plama dodatkowo spowalnia ruch o **20%**. Spowolnienie jest odświeżane podczas stania w tłuszczu i wygasa do 0,3 s po wyjściu; nakładające się plamy nie mnożą spowolnienia. Po wyjściu z plamy wróg nie otrzymuje dalszych obrażeń od tej plamy. Ściany i inne podesty blokują jej obrażenia. Przy trafieniu w ścianę lub przeciwnika pocisk szuka podłoża poniżej; bez podłoża kończy się samym rozpryskiem. Osobne plamy zadają obrażenia niezależnie.

Pocisk ma wspólny z pozostałymi broniami obrys, jasną smugę, kwadratowe okruszki i powiększanie modelu z odległością. Upgrade **3.1 — Mielony: Bounce** daje dwa odbicia od podłoża. Pierwsze i drugie uderzenie odbijają pocisk, trzecie kończy jego lot; każde zostawia własną plamę. Trafienie w ścianę lub wroga nadal kończy pocisk i próbuje zostawić plamę na podłożu poniżej.

Parametry lotu, cooldown i działanie plamy: `Assets/Resources/Combat/Mielony.asset`. Pocisk używa kroku fizyki i sprawdzania całej przebytej drogi, a nie tylko kolizji w końcowym punkcie. Pauza zatrzymuje lot i DoT. Ulepszenia schabowego i chrupiącej nie wpływają na mielonego. Kontrole Play Mode: `-executeMethod RetyKotlety.Editor.MielonyChecks.Run` (ticki, wielokrotne collidery, wyjście i powrót do plamy, zasięg, czas życia i lot łukiem).

## Skrzynie i ulepszenia

Na MovementArena przy starcie stoją cztery skrzynki (x: `-8`, `-4`, `4`, `8`; y: `0,7`; z: `-43`). Podejdź, wyceluj i naciśnij **F**. Panel otwierania zatrzymuje rozgrywkę i przewija karty przez 3,2 s. Skrzynia losuje wyłącznie ulepszenia broni. Zdobyte ulepszenie patelni pozostaje w puli, ponieważ stackuje się; wspólne przedmioty i movement kupuje się w sklepie. Karty pokazują aktualną pulę; zatrzymanie wskazuje faktycznie przyznawaną nagrodę. Ulepszenia mechaniki broni są jednorazowe z wyjątkiem stackującej patelni. Po zakończeniu kliknij **Odbierz ulepszenie** albo naciśnij **Esc**. Skrzynia jest jednorazowa, a ulepszenie obowiązuje do restartu sceny. Przed użyciem pokazuje model zamknięty, a po odebraniu nagrody — otwarty, z mniejszym, pikselowym dymem z małych czarnych kwadratów wydobywających się ze środka. Po 4 sekundach rozgrywki skrzynia znika; resztki dymu łagodnie wygasają. Pauza zatrzymuje ten czas. Modele FBX i tekstury znajdują się w `Assets/Resources/Props/UpgradeChest/Closed` oraz `Open`; materiały są we wspólnym folderze `Materials`. Obie wersje są dopasowywane według podstawy, z zachowaniem kolizji zamkniętego korpusu. Otwarty model zachowuje oryginalne tekstury krawędzi, ścian i wieka. Tylko dno ma czarny materiał, a gęsta warstwa małych kwadratów dymu wydobywa się tuż pod górną krawędzią korpusu i zakrywa wnętrze. Poprzedni błędny model zachowano poza importem Unity w `Documentation/Archive/RejectedChestModel`.

Po ulepszeniu schabowy trafia początkowego przeciwnika i automatycznie leci do **maksymalnie trzech kolejnych, różnych wrogów**. Każdy następny cel jest najbliższym żywym, widocznym wrogiem w promieniu **14 m od pocisku**. Pocisk koryguje lot w stronę wybranego celu i zachowuje obrażenia. Nie wraca do trafionych wrogów; brak celu lub przeszkoda kończą łańcuch. Chrupiaca i pomidory nie otrzymują tego efektu. Dotychczasowe odbicia schabowego od otoczenia pozostają osobną mechaniką.

| ID | Ulepszenie | Działanie |
| --- | --- | --- |
| `1.1` | Schabowy: Rykoszet | Do trzech dodatkowych, różnych wrogów; zasięg 14 m |
| `2.1` | Chrupiaca: Piercing | Pociski przebijają wrogów, każdy pocisk trafia danego wroga tylko raz; ściany blokują |
| `3.1` | Mielony: Bounce | Dwa odbicia od ziemi i plama przy każdym kontakcie: maksymalnie trzy plamy |
| `2.2` | Chrupiaca: Krwawienie | 5 obrażeń co 0,25 s przez 3 s: 12 ticków, 60 obrażeń. Kolejne trafienia odświeżają czas bez mnożenia stosów i bez opóźniania następnego ticka |

Bez Dokładki chrupiaca strzela trzema pociskami w jednym poziomym wachlarzu względem kamery: lewy (-6°), środkowy (0°), prawy (+6°), bez losowego rozrzutu pionowego.

Piercing i krwawienie łączą się: każdy przebity przeciwnik otrzymuje bleed. Efekty chrupiącej obejmują wszystkie pociski salwy, ale bleed pozostaje pojedynczym statusem na przeciwniku. Pauza zatrzymuje krwawienie.

Definicje nagród: `Assets/Resources/Upgrades` (ID, opis i parametry efektów). `PlayerUpgrades` przechowuje stan pojedynczego podejścia bez modyfikowania wspólnego assetu. Kolejne nagrody można definiować jako osobne `UpgradeDefinition`; nowe zachowania wymagają dodania obsługi ich efektu.

Kontrola samego harmonogramu krwawienia: `Validation/BleedChecks.cs` (kod C# poza Assets). Sprawdza ticki, długie klatki, odświeżanie i brak stackowania.

Test integracyjny w Unity: `-executeMethod RetyKotlety.Editor.UpgradeChecks.Run`. Sprawdza skrzynię, pauzę, przyznanie nagrody, brak przypadkowego stackowania, limit czterech trafionych wrogów i blokowanie łańcucha przez osłonę. Wyniki: `Validation/Reports/upgrade-results.txt` i `upgrade-errors.txt`.

## Stackujace przedmioty

Nowe przedmioty są wspólne dla broni i można zbierać je wielokrotnie. Każda sztuka daje poniższy przyrost; stacki tego samego bonusu sumują się. Build pod Tab pokazuje jedną kartę danego przedmiotu z licznikiem `xN`, liczbę rodzajów oraz łączną liczbę sztuk. Wspólne przedmioty nie przełączają wybranej broni.

| ID | Przedmiot | Przyrost za sztukę |
| --- | --- | --- |
| `5.1` | Podwójna panierka | +8% wszystkich obrażeń gracza, także DoT, patelni i slamu |
| `5.2` | Duża porcja | +10% promienia plam tłuszczu i slamu; nie zmienia hitboxów zwykłych pocisków |
| `5.3` | Rozgrzany tłuszcz | +1 obrażenie każdego ticka krwawienia i plam tłuszczu |
| `5.4` | Danie dnia | +4% obrażeń pocisku za każdego poprzedniego różnego trafionego wroga; bonus maks. +100% |
| `5.5` | Na wynos | +3% obrażeń pocisku za każde pełne 5 m jego faktycznej drogi; bonus maks. +100% |
| `5.6` | Podrzut szefa kuchni | +10% obrażeń przeciw wrogom aktualnie podrzuconym slamem, także obrażeń DoT |
| `5.7` | Dokładka | +1 pocisk na atak każdej broni dystansowej; stackuje się bez obniżania obrażeń pocisku |

**Dokładka (`5.7`)** jest dostępna w sklepie po awansie i konsoli. `give 5.7` dodaje jedną sztukę, `give 5.7 3` dodaje trzy. Z jedną sztuką schabowy wystrzeliwuje 2 pociski, chrupiaca 4, mielony 2, a łosoś 2 ryby. Każdy kolejny stack dodaje po jednym pocisku do salwy; patelnia pozostaje bez zmian. Schabowy i łosoś wystrzeliwują pociski kolejno co 0,09 s w aktualnym kierunku celowania. Chrupiaca i mielony zachowują symetryczny poziomy wachlarz z odstępem 6° między sąsiadami do maksymalnej szerokości 72°.

Każdy dodatkowy pocisk zachowuje pełne obrażenia i własne efekty: rykoszet, piercing, bleed, plamy, bounce lub orbitę. Ryby wracają niezależnie, a ręka odblokowuje kolejny rzut dopiero po powrocie całej salwy. Panel buildu pokazuje liczbę pocisków na atak oraz liczbę ryb aktualnie w locie. Test w Unity: `-executeMethod RetyKotlety.Editor.MultishotChecks.Run` (ilość pocisków czterech broni, efekty na każdym pocisku i blokada rzutu do powrotu ostatniej ryby).

Przykład: pięć sztuk panierki daje +40%, a nie pięć mnożników. Trzy sztuki rozgrzanego tłuszczu zwiększają bazowy tick z 5 do 8; panierka mnoży tę wartość tylko raz. Promień slamu jest liczony z wysokości i limitów profilu, a następnie zwiększany przez Dużą porcję, więc ulepszony slam może przekroczyć bazowe 5 m.

Danie dnia pamięta różne cele przez całe życie pocisku. Powrót i kolejne okrążenia tego samego wroga łososiem nie nabijają nowych różnych celów, ale korzystają z już zdobytego bonusu. Na wynos dolicza lot do celu, orbitę oraz powrót łososia. Oba bonusy sumują się ze sobą w jednym mnożniku (każdy osobno ma limit +100%). Nie zwiększają ticków DoT ani obrażeń patelni i slamu. Mielony korzysta z panierki, obszaru, dodatkowych obrażeń DoT i bonusu przeciw podrzuconym; jego plamy nie dostają bonusu za przebytą drogę.

Pocisk zapamiętuje statystyki przy wyrzucie, a plamy dziedziczą je z pocisku; już lecący pocisk lub istniejąca plama nie zmieniają siły po odebraniu kolejnego przedmiotu. Slam i patelnia pobierają bieżące bonusy przy uderzeniu. Stan podrzucenia celu jest sprawdzany przy samym trafieniu/ticku. Bazowe assety nie są modyfikowane. Dotychczasowe ulepszenia `1.x`–`4.x` nadal nie stackują się.

Test obliczeń i historii trafień: `python3 Validation/check_incremental_bonuses.py`. Test stanu kolekcji i zasobów wewnątrz Unity: `-executeMethod RetyKotlety.Editor.IncrementalUpgradeChecks.Run`. Raporty trafiają do `Validation/Reports`. Test matematyczny nie zastępuje Play Mode: obszary, kolizje, UI i rzeczywiste loty trzeba sprawdzić w grze.

## Konsola debugowania

**§ / tylda (`~`, klawisz BackQuote)** otwiera konsolę i zatrzymuje grę. **Esc** lub ten sam klawisz zamyka konsolę. Nie nakłada się na inne otwarte menu. Komendę zatwierdza **Enter**, **Enter numeryczny** lub przycisk **Wyślij**.

- `give 1.1` — rykoszet schabowego.
- `give 2.1` — piercing chrupiącej.
- `give 2.2` — krwawienie chrupiącej.
- `give 3.1` — dwa odbicia mielonego od podłoża.
- `give 5.1 5` — pięć sztuk Podwójnej panierki.
- `give 5.7 3` — trzy sztuki Dokładki, czyli +3 pociski do każdej salwy.
- `list` — lista dostępnych ID i nazw; `[STACK]` oznacza powtarzalny przedmiot.
- `help` — pomoc.
- `clear` — wyczyszczenie konsoli.

`give` przyznaje upgrade na bieżące podejście. Ulepszenie konkretnej broni wyposaża tę broń, a przedmiot wspólny pozostawia bieżącą broń. `give 5.1 5` dodaje pięć sztuk panierki; opcjonalna liczba może wynosić 1–100. Powtórne `give` jednorazowego ulepszenia broni nie stackuje efektu, natomiast przedmioty `5.x` zwiększają swój licznik. Build pod Tab pokazuje nagrody otrzymane przez konsolę, sklep i skrzynie. ID `1.x` dotyczą schabowego, `2.x` chrupiącej, `3.x` mielonego, `4.x` łososia, a `5.x` wspólnych przedmiotów stackujących.

## Podglad buildu

**Tab** otwiera i zamyka panel aktualnego podejścia. Panel pokazuje posiadane bronie, broń w dłoni, zdobyte ulepszenia z opisami, jetpack, paliwo, pierogi oraz parametry slamu. Lista ulepszeń korzysta z faktycznie zdobytych definicji i przewija się przy większej liczbie nagród. **Esc** lub przycisk **Zamknij** również wraca do gry. Otwarty panel zatrzymuje rozgrywkę; nie nakłada się na pauzę ani otwieranie skrzynki.

Jetpack działa przez **przytrzymanie Spacji w powietrzu** po podniesieniu znajdźki. Na ziemi Spacja wykonuje zwykły skok; po skoku jest 0,18 s opóźnienia zapłonu, żeby jetpack nie przycinał początkowego impulsu. Puszczenie Spacji, lądowanie lub brak paliwa wyłącza ciąg. Tab pozostaje menu buildu. Build resetuje się razem z podejściem.

## Struktura projektu

| Katalog | Zawartość |
| --- | --- |
| `Assets/Scripts/Player` | Gracz, hak i kiełbasiana linka |
| `Assets/Scripts/Movement` | Motor ruchu, obliczenia i profil ustawień |
| `Assets/Scripts/Combat` | Pociski, rozrzut i prezentacja broni w dłoni |
| `Assets/Scripts/Progression` | Poziomy, XP, punkty do zebrania i zakupy |
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
| V, 1, 2, 3, kółko myszy | Wybór broni |
| F / E | Podniesienie przedmiotu / zjedzenie pieroga |
| Q / R | Wyrzucenie lub zwolnienie haka / skracanie linki |
| Tab | Otworz / zamknij podglad buildu (pauza) |
| Przytrzymana Spacja w powietrzu | Jetpack po podniesieniu |
| Esc | Pauza |

## Szkielet dystansowy i tekstury PSX

Przeciwnik dystansowy używa teraz `Assets/Resources/Enemies/Skeleton/Models/Skeleton.fbx` z materiałem `Skeleton.mat` i atlasem `Textures/Base.png`. Model jest dopasowywany do istniejącej kapsuły i podłoża. Strzelanie, zasięg i nawigacja pozostają zachowaniem `Enemy`. Starsze zapisane sceny podmieniają dotychczasowy model przy uruchomieniu, a generator od razu tworzy szkieleta.

Dostarczony FBX jest statycznym meshem, bez animacji. Chodzeniu towarzyszy lekki ruch modelu, a strzałowi krótki odrzut. Warianty tekstury zachowano w `Textures/Variant2` i `Textures/GameBoyColor`. Źródłowy plik Blender jest w `SourceAssets/Skeleton`, żeby projekt nie wymagał Blendera podczas importu.

Tekstury PSX są w `Assets/Resources/PSX/Textures`: `Albedo`, `Normals/GL` i `Normals/DX`. Kolory mają filtrowanie Point i brak kompresji; normal mapy są oznaczone jako normal mapy. Arena używa albedo Ground, Concrete i Metal. Shader `PSXWorld` mapuje je w przestrzeni świata i powtarza co około 2,86 m, bez rozciągania na dużych bryłach. Normal mapy są uporządkowane do dalszego wykorzystania, ale nie są obecnie podpięte do materiałów areny. Kolory tras, znaki, cele haka i skrzynki zachowują swoje oznaczenia.

## Skala HUD

**Opcje → Skala HUD** zmienia rozmiar interfejsu podczas gry od **50% do 120%** w krokach 5%. Domyślnie ustawione jest **80%**. Zmiana działa od razu, również przy otwartej pauzie, a wybór jest zapisywany przy wyjściu z opcji. Panel pozostaje zakotwiczony przy dolnej lewej krawędzi; celownik, modele broni, menu i licznik FPS nie są skalowane.

## Licznik FPS

Licznik w prawym górnym rogu jest domyślnie włączony. **Opcje → Pokaż licznik FPS** w menu głównym lub podczas pauzy pozwala go wyłączyć i ponownie włączyć. Wybór zapisuje się między uruchomieniami. Wynik jest uśredniany co 0,5 s i działa także podczas pauzy.

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

## Ulepszenia movementu

- **6.1 — Śliska sprawa:** slide na ziemi rozlewa tłuszcz (promień 0,7 m, czas 3 s, 2 obrażenia co 0,25 s, slow 20%). Plamy powstają co minimum 1,2 m i 0,15 s, korzystają z bonusów do obszaru i DoT.
- **6.2 — Szybka obsługa:** dash daje +35% szybkości ataku przez 3 s. Kolejny dash odświeża czas; bonus przyspiesza animacje ataku, cooldown mielonego oraz serie schabowego i łososia. Przyspiesza też o 35% lot łososia, powrót i orbitowanie, również dla ryb już w powietrzu. Zasięg, obrażenia i liczba okrążeń pozostają takie same; po wygaśnięciu buffa wraca zwykłe tempo.

Oba ulepszenia są jednorazowe, dostępne w sklepie po awansie, buildzie pod Tab i przez `give 6.1` / `give 6.2`.

## Patelnia: Od dechy do dechy (`0.1`)

Stackujący upgrade w skrzyniach i buildzie pod Tab. `give 0.1` daje jedną sztukę, `give 0.1 3` daje trzy i wybiera patelnię. Uderzony przeciwnik wylatuje poziomo w kierunku celowania, z niewielkim podrzutem. Każdego napotkanego wroga trafia raz na lot, zadając 60 obrażeń kolizji (+30 za każdą kolejną sztukę) i lekko odpychając. Prędkość lotu wynosi 16 m/s (+3 m/s za kolejną sztukę). Kolizje korzystają z bonusów obrażeń gracza i bonusu na podrzuconych wrogach.

Efekt działa też po śmiertelnym uderzeniu patelnią: ciało znika po zakończeniu lotu. Ściany zatrzymują lot, a żywy wróg wraca do nawigacji po lądowaniu. Delikatny odrzut trafionych celów nie tworzy kolejnych pocisków ani lawiny obrażeń.

Test kolizji w Unity: `-executeMethod RetyKotlety.Editor.PanBowlingChecks.Run` (jedno trafienie na cel, wiele colliderów, zatrzymanie na ścianie, brak obrażeń przez ścianę, lot po śmiertelnym uderzeniu).

## Levelowanie, punkty i sklep

Poza hordą na Arenie progresji zabójstwo zwykłego wroga daje 10 XP i wyrzuca złotą kostkę wartą 5 punktów. Wróg dystansowy daje 15 XP i 8 punktów. XP trafia do gracza od razu. Między awansami kostki przyciągają się z odległości 5 m, jeżeli nie oddziela ich ściana. Przy każdym awansie wszystkie pozostałe kostki na planszy trafiają do portfela przed otwarciem sklepu, włącznie z nagrodą za ostatnie zabójstwo. Manekiny treningowe nie przyznają nagród. Jedna śmierć przyznaje nagrodę tylko raz, również dla ciał wystrzelonych patelnią.

Run zaczyna się na poziomie 1. Pierwszy awans wymaga 30 XP, każdy kolejny próg rośnie o 20 XP (30, 50, 70...). Nadwyżka XP przechodzi na następny poziom. Po awansie automatycznie otwiera się sklep i zatrzymuje walkę; jeśli inne menu jest otwarte, sklep poczeka na jego zamknięcie. Od poziomu 2 można ponownie otwierać sklep pod **B**. **B**, **Esc** albo przycisk powrotu zamykają sklep. Można kupić wiele przedmiotów podczas jednej wizyty, a niewydane punkty zachowują się w obrębie runu.

- **Sklep:** wspólne przedmioty `5.1`–`5.7` i jednorazowe ulepszenia movementu `6.1`–`6.2`. Statystyki podstawowe też kupuje się za punkty. Każdy stack daje ten sam przyrost.

| ID | Statystyka | Na sztukę | Pierwszy zakup |
|---|---|---|---:|
| 7.1 | Solidny posiłek | +20 maksymalnego i obecnego HP | 10 |
| 7.2 | Mocny wyrzut | +15% prędkości pocisków | 10 |
| 7.3 | Sprawne dłonie | +10% szybkości ataku | 15 |
| 7.4 | Domowy rosół | +0,5 HP/s regeneracji | 10 |

- **Skrzynie:** wyłącznie ulepszenia broni `0.1`, `1.1`, `2.1`, `2.2`, `3.1`, `4.1`. Patelnia `0.1` nadal stackuje się jako ulepszenie konkretnej broni. Stare skrzynie z przypisanym przedmiotem wspólnym przechodzą na losowanie ulepszenia broni.
- **Ceny:** bazowo 10 punktów; Dokładka kosztuje bazowo 30, ulepszenia movementu 15. Każda już posiadana sztuka danego przedmiotu podnosi jego cenę o połowę ceny bazowej, zaokrągloną w dół (minimum 5 punktów). Cena bazowa jest konfigurowalna w `UpgradeDefinition.shopBaseCost`.

HUD pokazuje poziom, postęp XP i portfel. Panel buildu pod Tab nadal zbiera wszystkie nabyte ulepszenia. Po śmierci/restartowaniu sceny poziom, punkty i upgrade'y resetują się.

Weryfikacja obliczeń XP, przenoszenia nadwyżki, cen i portfela: `python3 Validation/check_incremental_bonuses.py`. Test integracyjny w Play Mode: `-executeMethod RetyKotlety.Editor.ProgressionIntegrationChecks.Run` (śmierć, pickup, zakup, blokady menu i pula skrzynek).

## Przykładowy run: ProgressionArena

Uruchom **Arena progresji** z menu głównego albo otwórz `Assets/Scenes/ProgressionArena.unity` i naciśnij Play. Scena ma własną edytowalną geometrię, punkty referencyjne fal i żółte pole dostaw; dotychczasowa `MovementArena` pozostaje osobnym sandboxem.

- Start: wszystkie cztery bronie dystansowe i patelnia, bez darmowych upgrade'ów, XP i punktów. Pierwsza fala rusza po 8 s.
- Dziewięć fal liczy kolejno **18, 24, 32, 40, 48, 56, 64, 72 i 84** przeciwników. Grupy wchodzą podczas walki, maksymalnie 48 żywych naraz. Po oczyszczeniu fali jest 6 s przerwy i 15 HP leczenia przed następną.
- Od drugiej fali pomarańczowy **Tłuczek** sygnalizuje uderzenie wokół siebie (1,1 s ostrzeżenia, 4,5 m, 26 obrażeń). Od czwartej fioletowy **Pieprznik** ostrzeliwuje zapamiętane miejsce pod graczem (1,5 s, 3,4 m, 20 obrażeń). Można wybiec z kręgu, schować się za osłoną lub podrzucić elitę podczas przygotowania ataku.
- Zwykły wróg hordy daje 3 XP i 1 punkt, strzelec 4 XP i 2 punkty, elita 14 XP i 7 punktów. Dziesięć pierwszych zabójstw wystarcza na awans i, po zebraniu kostek, na pierwszy podstawowy zakup. Kolejne sztuki Dokładki kosztują 30, 45 i 60 punktów.
- Po oczyszczeniu fal **3, 6 i 9** na żółte pole przy starcie trafia po jednej skrzyni z ulepszeniem broni. Odbierz ją pod **F**. Nieodebrane skrzynie pozostają na polu.
- Dwie rampy prowadzą na platformy na wysokości 5 m. Centralna wieża 9 m służy do hooka i wysokiego slama. Niskie tunele oraz otwarta pętla po obwodzie pozwalają testować slide, tłuszcz i dash.
- Po dziewiątej fali można spokojnie zebrać resztę punktów i odwiedzić sklep. Podejdź do niebieskiej strefy na końcu areny i naciśnij **F**, aby zakończyć run.

Geometria: `Assets/Resources/Levels/ProgressionArena.json`. Generator tylko tej sceny: `python3 Validation/build_progression_arena.py` (nadpisuje wygenerowaną geometrię ProgressionArena, nie MovementArena). Logika fal i ustawienia: `ProgressionArenaWaves`. Test uruchomienia w Unity: `-executeMethod RetyKotlety.Editor.ProgressionArenaChecks.Run`.
