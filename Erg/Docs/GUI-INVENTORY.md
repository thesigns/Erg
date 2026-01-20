Opis okna Inventory.

Na górze znajduje się nagłówek ekranu "INVENTORY". Nod nim nagłówek "kolumn" itemów - Item, Mass i Value. Poniżej jest lista itemów podzielona na kategorie. Każda kategoria ma wewnętrzny nagłówek np. "Books". Wewątrz kategorii itemy są posortowane alfabetycznie, podobnie kategorie są w koejności alfabetycznej. Po prawej stronie pasek przewijania (ASCII), na całą wysokość listy itemów. Po lewej strzałeczka >> wskazująca która linia listy jest zaznaczona. Na samym dole 2 linie podpowiedzi klawiaturowych. Podpowiedzi są kontekstowe, jeśli itemu nie da się przeczytać to podpowiedź [rR] read się nie wyświetla. Akcje bez shiftu (mała litera) to użycie 1 itemu, akcja z Shift (duża litera) to użycie określonej liczby (powinno się pojawić małe okienko z pytaniem o liczbę i możliwością wpisania liczby z klawiatury) np "R" pozwala przeczytać od razu kilka książek zestackowany. Wysokość tabeli itemów ma 20 linii. Tabela jest posortowana wg typów item i zawiera nagłówki typów. Całość można przewijać w pionie jeśli się nie mieści w 20 liniach strzałkami, home/end (góra/dół) i pgup/pgdn (20 linii w górę/20 linii w dół. Wewnętrzne nagłówki listy itemów są "przeskakiwane" tzn nie da się ich zaznaczyć. Kolory ładne, czytelna 

Poniżej znajduje się projekt okna Inventory o wysokości 25 linii:

```
################################### INVENTORY ##################################
    Item                                                 Mass      Value     
 -- Books ------------------------------------------------------------------- :      
 >> « Book of Swimming (x2)                              600       900        #  
    « Book of Literacy                                   300       450        :
 -- Coins ------------------------------------------------------------------- :
    ¤ Copper Coin (x45)                                  45        45         :
    ¤ Gold Coin                                          6         400        :
 -- Corpses ----------------------------------------------------------------- :
    ϗ Zombie Corpse                                      12000     50         :
                                                                              :
                                                                              :
                                                                              :
                                                                              :
                                                                              :
                                                                              :
                                                                              :
                                                                              :
                                                                              :
                                                                              :
                                                                              :
                                                                              :

Actions: [dD] Drop [rR] Read [w] Wield
[Esc] Quit [Arrows/Home/End/PgUp/PgDn] Navigate
```

Dostępne akcje:
[dD] - drop, wszystkie item
[rR] - read, książki, w przyszłości też scroller
[w] - wield, chwyć do ręki (na razie nie ma slotów ekwpunku, więc nie ma znaczenia), wszystkie itemy
[cC] - consume, zwłoki i w przyszłości napoje i jedzenie

Jednostka masy w świecie gry to Coin (c) - dokładnie tyle ile waży jeden miedziak (odpowiednik 5 gram). Podobnie z Value, podawana jest w miedziakach (Value Copper Coin = 1c, Silver Coin = 20c, Gold Coin = 400c).