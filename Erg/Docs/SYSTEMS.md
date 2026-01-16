# WSTĘP

Gra opiera się o system podstawowych atrybutów, podstawowych umiejętności, wartości pochodnych zdolności i zakresów zdolności zależnych od gatunku.

## PODSTAWOWE ATRYBUTY

Podstawowe atrybuty określają wrodzone podstawowe cechy fizyczne i psychiczne postaci. Mieszczą się zawsze w zakresie od 0 do 1 (liczba zmiennoprzecinkowa) - spektrum w ramach funkcjonującej jednostki gatunku. Przykładowo człowiek z siłą 0 to skrajny słabeusz a człowiek z siłą 1 siłacz nad siłacze.

W grze dostępne są następujące podstawowe atrybuty: Siła, Wytrzymałość, Zręczność, Percepcja, Inteligencja, Siła woli, Charyzma, Uroda.

Podnoszenie podstawowych atrybutów jest bardzo trudne i odbywa się powoli.

## PODSTAWOWE UMIEJĘTNOŚCI

Podstawowe umiejętności określają wyuczone podstawowe zdolności przydatne w świecie gry. Mieszczą się zawsze w zakresie od 0 do 1 (liczba zmiennoprzecinkowa) - spektrum w ramach możliwości danej postaci. Przykładowo troll z umiejętnością Pływanie == 0 nie posiada żadnego treningu w tym zakresie, a troll z umiejętnością Pływanie == 1 to mistrz pływacki swojego gatunku.

Podobnie jak atrybuty, umiejętności są względne do gatunku — określają biegłość w ramach tego, czego dany gatunek jest w stanie się nauczyć. Troll-mistrz pływania (1.0) może wciąż pływać gorzej niż przeciętny człowiek, ponieważ trolle jako gatunek nie są predysponowane do pływania. Faktyczna efektywność wynika z wartości pochodnych, które są ograniczone zakresami gatunkowymi.

Podnoszenie podstawowych umiejętności jest łatwiejsze niż podnoszenie atrybutów.

## WARTOŚCI POCHODNE ZDOLNOŚCI

Czasem atrybuty i umiejętności, a nawet inne czynniki łączą się w wartość pochodną zdolności, z różnymi wagami. Wartości pochodne zdolności mieszczą się zawsze w zakresie od 0 do 1 (liczba zmiennoprzecinkowa). Przykładowo wartością pochodną jest Szansa trafienia przeciwnika, który nie wykonuje aktywnej obrony przed atakiem, w skrócie ToHitDerived. Przykładowy wzór na ToHitDerived to 0.3 * Zręczność + 0.7 * Skill_Noże (jeśli postać posługuje się aktualnie nożem). Widać więc, że bardziej liczy się wytrenowanie w nożach niż wrodzona zręczność postaci.

## ZAKRESY GATUNKOWE ZDOLNOŚCI

Każdy gatunek posiada zdefiniowane zakresy (minimum i maksimum) dla każdej zdolności, które określają faktyczną efektywność działań. Wartość pochodna zdolności (0-1) jest interpolowana w ramach zakresu gatunkowego, dając rzeczywisty wynik.

Wzór: `Efektywność = Lerp(Min_gatunku, Max_gatunku, Wartość_pochodna)`

Przykładowo, dla szansy trafienia (ToHitDerived), zakres MinToHit i MaxToHit jest:

| Gatunek  | MinToHit | MaxToHit | Interpretacja                                       |
|----------|----------|----------|-----------------------------------------------------|
| Człowiek | 0.10     | 0.95     | Pełne spektrum — od nieudacznika do mistrza         |
| Elf      | 0.20     | 0.98     | Wrodzona gracja, wyższy sufit                       |
| Goblin   | 0.05     | 0.60     | Może się nauczyć, ale nigdy nie dorówna człowiekowi |
| Troll    | 0.05     | 0.40     | Nieporadny, siła nie zastąpi precyzji               |

Przykład obliczenia:
- Goblin z ToHitDerived = 0.8 → Lerp(0.05, 0.60, 0.8) = 0.49
- Człowiek z ToHitDerived = 0.5 → Lerp(0.10, 0.95, 0.5) = 0.525

Goblin wytrenowany niemal do perfekcji (0.8) wciąż trafia rzadziej niż przeciętny człowiek (0.5). Gatunek definiuje biologiczne granice, jednostka wypełnia przestrzeń między nimi swoim wysiłkiem.

## NIELINIOWE KRZYWE INTERPOLACJI

Domyślnie interpolacja jest liniowa, ale niektóre zdolności mogą używać nieliniowych krzywych, które lepiej oddają naturę ich rozwoju.

Wzór: `Efektywność = Lerp(Min_gatunku, Max_gatunku, Krzywa(Wartość_pochodna))`

| Krzywa   | Formuła           | Charakterystyka                                |
|----------|-------------------|------------------------------------------------|
| Linear   | `t`               | Domyślna, neutralna                            |
| EaseOut  | `1 - (1-t)²`      | Łatwo być OK, trudno być mistrzem              |
| EaseIn   | `t²`              | Trudny start, potem szybki wzrost              |
| SCurve   | `t² × (3 - 2t)`   | Trudny start, plateau, potem mistrzostwo       |

Przykłady zastosowań:
- **Walka wręcz → EaseOut:** Każdy szybko nauczy się podstaw, ale mistrzostwo wymaga lat praktyki.
- **Czytanie → EaseIn:** Nauka liter i słów trwa długo, ale gdy "zaskoczy", postęp jest szybki.
- **Magia → SCurve:** Trudno w ogóle zacząć, potem plateau nauki podstaw, wreszcie mistrzostwo.

Przykład dla walki (EaseOut):
- derived 0.5 → Krzywa(0.5) = 0.75 → szybko osiągasz 75% potencjału
- derived 0.9 → Krzywa(0.9) = 0.99 → ostatnie 10% wymaga ogromnego wysiłku

## MODYFIKATORY SYTUACYJNE I EKWIPUNKU

Sytuacje w grze oraz ekwipunek mogą tymczasowo lub trwale modyfikować zakresy gatunkowe zdolności.

Przykłady:
- **Oślepienie:** Zakres gatunkowy zdolności Czytanie (MinReading, MaxReading) spada do 0 dla obu wartości — ślepy nie czyta.
- **Magiczny Pierścień Czytelnictwa:** Podwaja MaxReading — uczony człowiek z pierścieniem czyta jeszcze lepiej.
- **Kombinacja:** Oślepiony z pierścieniem nadal nie czyta (0 × 2 = 0). Modyfikatory stosują się logicznie.

## LISTA ZAKRESÓW GATUNKOWYCH ZDOLNOŚCI

- MinToHit, MaxToHit — zakres prawdopodobieństwa trafienia w walce przeciwnika, przed sprawdzeniem uniku/obrony
- MinReading, MaxReading — zakres umiejętność czytania ze zrozumieniem tekstów

## PUNKTY PRZEZNACZENIA

Postać gracza jako bohater ma niewielką (1-3) liczbę Punktów Przeznaczenia, które ratowałyby gracza z opresji w beznadziejnych sytuacjach w spektakularny sposób. Punkty Przeznaczenia możnaby regenerować, choć byłoby to trudne (do opracowania).  Efekty Punktów Przeznaczenia byłyby kontekstowe, przykładowo:

- Walka: wróg, zamiast zadać śmiertelny cios, potyka się i łamie kark
- Pułapka: mechanizm, zamiast ranić śmiertelnie gracza, zacina się
- Trucizna: postać wymiotuje całość, zanim wchłonie ostatnią śmiertelną dawkę
- Upadek: postać łapie się krawędzi lub przeskakuje nad otchłanią
- Magia: zaklęcie, które raziłoby śmiertelnie, rykoszetem wraca do rzucającego
