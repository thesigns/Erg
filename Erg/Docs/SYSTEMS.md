# WSTĘP

Gra opiera się na systemie atrybutów podstawowych, umiejętności podstawowych, umiejętności pochodnych oraz zakresów zdolności zależnych od gatunku.

## ATRYBUTY PODSTAWOWE

Atrybuty podstawowe opisują wrodzone, fundamentalne cechy fizyczne i psychiczne postaci. Każdy atrybut mieści się w zakresie od 0 do 1 (liczba zmiennoprzecinkowa), reprezentując spektrum możliwości w ramach danego gatunku.

Przykładowo: człowiek z wartością Strength = 0 jest skrajnie słabą jednostką, natomiast człowiek z wartością Strength = 1 to osobnik o maksymalnej, niemal granicznej sile fizycznej.

W grze występują następujące atrybuty podstawowe: Strength, Endurance, Agility, Perception, Intelligence, Willpower, Charisma.

Zmiany bazowych atrybutów zachodzą bardzo powoli i są wynikiem długotrwałych działań postaci. Przykładowo regularne dźwiganie dużych ciężarów stopniowo zwiększa atrybut Strength. Początkowe wartości atrybutów zależą od wybranej profesji, która odzwierciedla dotychczasową drogę życiową postaci.

Każdy atrybut posiada:

* **wartość bazową** – trwałą, rozwijaną w czasie,
* **wartość aktualną** – modyfikowaną tymczasowo, np. przez przedmioty, efekty magiczne lub stany.

## UMIEJĘTNOŚCI PODSTAWOWE

Umiejętności podstawowe określają wyuczone, podstawowe zdolności przydatne w świecie gry, które można bezpośrednio trenować. Zawsze mieszczą się w zakresie od 0 do 1 (liczba zmiennoprzecinkowa), reprezentując spektrum biegłości w ramach możliwości danej postaci.

Przykładowo: troll z umiejętnością Pływanie = 0 nie posiada żadnego treningu w tym zakresie, natomiast troll z umiejętnością Pływanie = 1 jest mistrzem pływania w ramach możliwości swojego gatunku.

Podobnie jak atrybuty, umiejętności są względne względem gatunku i określają poziom opanowania danej zdolności w granicach tego, czego dany gatunek jest w stanie się nauczyć. Troll‑mistrz pływania (1.0) może wciąż pływać gorzej niż przeciętny człowiek, ponieważ trolle jako gatunek nie są predysponowane do pływania. Faktyczna efektywność wynika z zakresów zdolności gatunkowych.

Trenowanie umiejętności podstawowych odbywa się szybciej niż trenowanie atrybutów podstawowych i może następować poprzez czytanie podręczników, trening u nauczycieli NPC oraz praktykę (np. pływanie zwiększa umiejętność Pływanie). Nieużywane umiejętności tracą wartość, jednak proces ten zachodzi bardzo powoli.

## WSPÓŁCZYNNIKI TRENINGU

Każdemu atrybutowi oraz każdej umiejętności podstawowej przypisany jest współczynnik treningu, określany funkcją zależną od jego wartości bazowej. Domyślnie współczynnik ten definiowany jest jako:

`TrainingFactor = 1 - WartośćBazowa`

Przykładowo:

`StrengthTrainingFactor = 1 - BaseStrength`

Wzrost oraz spadek bazowych atrybutów i umiejętności podstawowych jest mnożony przez odpowiadający im współczynnik treningu oraz globalną wartość gry **TrainingSpeed**. Przy zastosowaniu domyślnej funkcji szybkość rozwoju maleje liniowo wraz ze wzrostem wartości bazowej, aż do osiągnięcia zera przy wartości maksymalnej.

## UMIEJĘTNOŚCI POCHODNE

Umiejętności pochodne powstają poprzez połączenie atrybutów, umiejętności podstawowych oraz innych czynników, z różnymi wagami wpływu. Podobnie jak pozostałe, zawsze mieszczą się w zakresie od 0 do 1 (liczba zmiennoprzecinkowa).

Przykładem umiejętności pochodnej jest umiejętność ataku maczugą, określana jako **SkillDerivedMace**. Przykładowy wzór obliczania tej umiejętności może wyglądać następująco:

`SkillDerivedMace = 0.3 * Strength + 0.1 * Agility + 0.6 * SkillMace`

W przedstawionym przypadku największy wpływ na umiejętność ataku maczugą ma ogólne wytrenowanie w posługiwaniu się maczugami, następnie siła, a w najmniejszym stopniu zręczność.

## ZDOLNOŚCI I ICH ZAKRESY GATUNKOWE

Każdy gatunek posiada ten sam zestaw zdolności (**Abilities**), zdefiniowanych przez zakresy minimalne i maksymalne dla każdej zdolności. Zakresy te określają faktyczną efektywność działań, zależną od gatunku. Atrybuty, umiejętności podstawowe lub umiejętności pochodne są używane do interpolacji wartości zdolności, dając rzeczywisty wynik.

Wzór:

`Zdolność = Lerp(MinGatunku, MaxGatunku, WartośćWejściowa)`

Przykładowo, dla zdolności trafienia maczugą (**AbilityMaceHit**) zakres **AbilityMaceHitMin** oraz **AbilityMaceHitMax** zależy od gatunku:

| Gatunek  | Min  | Max  | Interpretacja                                   |
|----------|------|------|-------------------------------------------------|
| Człowiek | 0.10 | 0.90 | Pełne spektrum — od nieudacznika do mistrza.    |
| Troll    | 0.35 | 0.98 | Wrodzona zdolność posługiwania się maczugami.   |
| Elf      | 0.03 | 0.50 | Wrodzony brak predyspozycji do używania maczug. |
| Szczur   | 0.00 | 0.00 | Nigdy nie nauczy się używać maczugi.            |

W efekcie elf wytrenowany w posługiwaniu się maczugą niemal do perfekcji wciąż trafia nią rzadziej niż przeciętny troll.

## MODYFIKATORY SYTUACYJNE I EKWIPUNKU

Sytuacje w grze oraz ekwipunek mogą tymczasowo lub trwale modyfikować zakresy gatunkowe zdolności.

Przykłady:

* **Oślepienie:** Zakres gatunkowy zdolności Czytanie (AbilityReadingMin, AbilityReadingMax) zostaje zredukowany do 0 dla obu wartości — postać niewidoma nie jest w stanie czytać.
* **Magiczny Pierścień Czytelnictwa:** Podwaja wartość AbilityReadingMin — przedmiot wspiera przede wszystkim słabszych czytelników.
* **Kombinacja:** Oślepiona postać wyposażona w pierścień nadal nie czyta (0 × 2 = 0). Modyfikatory są stosowane w sposób logiczny i hierarchiczny.

## LISTA ZDOLNOŚCI

- AbilityHitUnarmed — zdolność trafienia przeciwnika bez użycia broni
- AbilityHitKnife — zdolność trafienia przeciwnika nożem lub podobną bronią krótką
- AbilityHitMace — zdolność trafienia przeciwnika maczugą
- AbilityDefenceUnarmed — zdolność bronienia się bez użycia broni
- AbilityDefenceKnife — zdolność bronienia się nożem lub podobną bronią krótką
- AbilityDefenceMace — zdolność bronienia się maczugą
- AbilityDefenceShield — zdolność obrony tarczą
- AbilityDodge — zdolność robienia uników
- AbilitySwimming — zdolność pływania
- AbilityReading — zdolność czytania
- AbilityCarrying — zdolność dźwigania przedmiotów (w gramach)

## PUNKTY PRZEZNACZENIA

Postać gracza jako bohater ma niewielką (1-3) liczbę Punktów Przeznaczenia, które ratowałyby gracza z opresji w beznadziejnych sytuacjach w spektakularny sposób. Punkty Przeznaczenia możnaby regenerować, choć byłoby to trudne (do opracowania).  Efekty Punktów Przeznaczenia byłyby kontekstowe, przykładowo:

- Walka: wróg, zamiast zadać śmiertelny cios, potyka się i łamie kark
- Pułapka: mechanizm, zamiast ranić śmiertelnie gracza, zacina się
- Trucizna: postać wymiotuje całość, zanim wchłonie ostatnią śmiertelną dawkę
- Upadek: postać łapie się krawędzi lub przeskakuje nad otchłanią
- Magia: zaklęcie, które raziłoby śmiertelnie, rykoszetem wraca do rzucającego

---

## Uwagi od Claude Code

### Ogólna ocena

System jest dobrze przemyślany i elegancki. Normalizacja wszystkich wartości do zakresu 0-1 upraszcza obliczenia i porównania między różnymi mechanikami. Separacja między wrodzonymi zdolnościami gatunkowymi a indywidualnym treningiem tworzy interesującą przestrzeń projektową.

### Zalety systemu

1. **Spójność matematyczna** — jednolity zakres 0-1 dla atrybutów, umiejętności i zdolności pochodnych ułatwia bilansowanie i przewidywanie interakcji między mechanikami.

2. **Gatunek ma znaczenie** — zakresy gatunkowe (Min/Max) elegancko rozwiązują problem różnic między rasami bez tworzenia sztywnych blokad. Szczur nigdy nie użyje maczugi, ale elf z determinacją może się nauczyć — choć nigdy nie dorówna trollowi.

3. **Naturalne spowolnienie progresu** — współczynnik treningu `1 - WartośćBazowa` sprawia, że początkowy rozwój jest szybki, a osiągnięcie mistrzostwa wymaga znacznego nakładu czasu. To intuicyjne i zgodne z rzeczywistością.

4. **Punkty Przeznaczenia** — mechanika ratunkowa dodaje dramatyzmu i zmniejsza frustrację z przypadkowych śmierci, zachowując jednocześnie poczucie zagrożenia.

### Pytania i sugestie do rozważenia

1. **Degradacja umiejętności** — wspomniano, że nieużywane umiejętności tracą wartość. Warto rozważyć, czy degradacja powinna być asymetryczna (wolniejsza niż trening) i czy powinna istnieć minimalna wartość "pamięci mięśniowej".

2. **Interakcje modyfikatorów** — przy wielu modyfikatorach sytuacyjnych i ekwipunku pojawia się pytanie o kolejność ich stosowania. Czy modyfikatory addytywne (+0.1) są stosowane przed multiplikatywnymi (×2)? Czy istnieje hierarchia źródeł modyfikatorów?

3. **Wartości graniczne** — co dzieje się, gdy modyfikatory wypychają wartość poza zakres 0-1? Czy są clampowane, czy mogą przekraczać naturalne limity (np. magicznie wzmocniony troll z AbilityMaceHit > 1.0)?

4. **Atrybuty a umiejętności pochodne** — w przykładzie `SkillDerivedMace` wagi sumują się do 1.0 (0.3+0.1+0.6). Czy to wymagane? Jeśli suma < 1.0, maksymalna wartość pochodna byłaby nieosiągalna; jeśli > 1.0, teoretycznie możliwe wartości > 1.0.

5. **Regeneracja Punktów Przeznaczenia** — wspomniano, że byłoby to trudne. Rozważenia warte są: osiągnięcia fabularne, odpoczynek w świątyniach, poświęcenie cennych przedmiotów, lub całkowity brak regeneracji (punkty jako skończony zasób na całą grę).

6. **Brakujące zdolności** — lista zdolności skupia się na walce i paru innych. Warto rozważyć dodanie: AbilityPersuasion (przekonywanie), AbilityLockpicking (otwieranie zamków), AbilityStealth (skradanie), AbilityCrafting (rzemiosło) — zależnie od planowanego zakresu gry.

### Uwaga implementacyjna

Przy implementacji w C# warto rozważyć użycie struktury `readonly record struct` dla zakresów gatunkowych oraz atrybutów z wartościami bazowymi i aktualnymi — zapewni to niezmienność i czytelność kodu.
