# Globalni stilovi

Zajedničke CSS klase koje koriste stranice svih modula. Fajlovi u ovom folderu pripadaju
platformskom timu. Modul ih koristi, a ne menja: predlog za novu klasu ili izmenu ide
platformskom timu.

| Fajl                | Sadrži                                                              |
| ------------------- | ------------------------------------------------------------------- |
| `_tokens.scss`      | Boje, razmaci, zaobljenja i veličine slova kao CSS promenljive.     |
| `_base.scss`        | Osnovni izgled elemenata: telo stranice, naslovi, linkovi, polja.   |
| `_layout.scss`      | Raspored: `.container`, `.stack`, `.grid`, `.actions`, `.toolbar`, `.links`. |
| `_components.scss`  | Komponente: dugme, kartica, polje forme, tabela, poruke, oznaka.   |

## Gde ide koji stil

1. Menja se vrednost boje, razmaka ili slova: token u `_tokens.scss`.
2. Klasu koristi više od jednog modula: globalna klasa u `_layout.scss` ili `_components.scss`,
   preko platformskog tima.
3. Sve ostalo: `.scss` fajl same komponente. Tu se vrednosti pišu preko tokena
   (`var(--space-4)`, `var(--color-border)`), nikad kao gotove boje ili pikseli.

Pre nego što napišeš lokalni stil, proveri da ga neka klasa iz ovog fajla već ne pokriva.
Lokalni `.actions { display: flex; ... }` ili ručno stilizovano polje znače da je globalna
klasa preskočena.

## Klase

### Raspored stranice

| Klasa        | Kada                                                                 |
| ------------ | -------------------------------------------------------------------- |
| `.stack`     | Na korenu stranice i forme: razmak između elemenata jedan ispod drugog. |
| `.grid`      | Lista kartica (ture, blogovi, klubovi).                              |
| `.actions`   | Red dugmadi: dugmad forme, akcije u redu tabele, paginacija.         |
| `.toolbar`   | Red polja i dugmadi iznad liste: pretraga, filteri, dugme „Create“.  |
| `.links`     | Red linkova za navigaciju unutar modula (početna stranica modula).   |

Sve tri klase za redove se prelamaju u novi red kada nema mesta.

```html
<div class="stack">
  <h1>Quizzes</h1>

  <div class="toolbar">
    <div class="field">…filter…</div>
    <a class="button button--primary" routerLink="/games/quizzes/create">Create quiz</a>
  </div>

  …lista…
</div>
```

### Kartice

| Klasa            | Kada                                                                       |
| ---------------- | -------------------------------------------------------------------------- |
| `.card`          | Uokvirena celina: kartica u listi, forma, potvrda.                        |
| `.card--narrow`  | Uz `.card`, za formu: ograničava širinu na 32rem i centrira je.           |

Stranica čija forma treba da bude šira proširuje `.card--narrow` lokalno, u svom `.scss`
fajlu, sa komentarom zašto (primer: `create-quiz.scss`, 40rem).

### Dugmad

| Klasa                       | Kada                                                                   |
| --------------------------- | ---------------------------------------------------------------------- |
| `.button`                   | Svako dugme i svaki link koji treba da izgleda kao dugme.             |
| `.button .button--primary`  | Glavna akcija stranice ili forme: Create, Save, Publish, Search.      |
| `.button .button--danger`   | Akcija koja trajno briše ili poništava: Delete, potvrda storniranja.   |

- Na jednoj formi ili u jednom redu tabele je najviše jedno primarno dugme.
- `.button--danger` nije za svako „Remove“. Uklanjanje koje se lako vraća (tura iz korpe)
  je obično `.button`.
- **Cancel izgleda kao dugme, nikad kao goli link.** Kada Cancel samo vodi nazad na listu,
  to je link: `<a class="button" routerLink="…">Cancel</a>`. Kada zatvara formu na istoj
  stranici, to je `<button class="button" type="button">`.
- Onemogućeno dugme (`[disabled]`) se samo prikazuje bleđe. Nije potrebna posebna klasa.

### Forme

| Klasa     | Kada                                                                          |
| --------- | ----------------------------------------------------------------------------- |
| `.field`  | Omotač jednog polja: `<label>`, polje i poruke o greškama ispod njega.         |

Polja (`input`, `select`, `textarea`) su stilizovana globalno i izvan `.field`, pa im ne treba
lokalni stil. Checkbox takođe ide u `.field`, sa labelom oko njega:

```html
<div class="field">
  <label for="name">Name</label>
  <input id="name" type="text" [formField]="form.name" />
  <p class="error">…</p>
</div>

<div class="field">
  <label>
    <input type="checkbox" [formField]="form.isPublic" />
    Visible without purchase
  </label>
</div>
```

### Tabele

| Klasa             | Kada                                                                   |
| ----------------- | ---------------------------------------------------------------------- |
| `.table`          | Svaka tabela.                                                          |
| `.table-wrapper`  | Omotač tabele koja na užem ekranu ne staje (mnogo kolona ili dugmadi). |

```html
<div class="table-wrapper">
  <table class="table">…</table>
</div>
```

### Poruke i tekst

| Klasa       | Kada                                                                    |
| ----------- | ----------------------------------------------------------------------- |
| `.error`    | Poruka o grešci: neuspelo učitavanje, greška polja, odbijena akcija.    |
| `.success`  | Poruka o uspehu posle akcije: uplata izvršena, plaćanje uspelo.         |
| `.meta`     | Sporedan tekst: učitavanje, prazno stanje, datum, broj rezultata.       |
| `.badge`    | Status u tabeli ili na kartici: Draft, Published, Pending, Reversed.    |

```html
@if (tours.isLoading()) {
  <p class="meta">Loading tours...</p>
}

<td><span class="badge">{{ tour.status }}</span></td>

<td colspan="5" class="meta">No quizzes to show.</td>
```

## Akcent modula

Exploration (petrol) i Games (ljubičasta) imaju svoj akcent, a Social i Payment koriste
osnovni (narandžasti). Akcent se ne postavlja klasom, nego na `:host` svake **stranice** modula,
u njenom `.scss` fajlu:

```scss
// Pages of the Exploration module use the module's own accent.
:host {
  --color-accent: var(--color-accent-exploration);
  --color-accent-text: var(--color-accent-exploration-text);
}
```

CSS promenljive se nasleđuju, pa boju dobija sve unutar stranice: `.button--primary`, linkovi,
okvir pri fokusu i komponente unutar stranice (kartica, mapa). Komponenta koja nije stranica,
na primer `tour-card`, ne dobija ovaj blok. Nova stranica u Exploration ili Games mora da ga ima.

## Konvencije

- Ceo tekst korisničkog interfejsa je na engleskom.
- Učitavanje i prazno stanje se pišu sa `.meta`.
- Glavna akcija na stranici je `.button--primary`, i kada je link (`<a class="button button--primary">`).
- Ne dodaju se biblioteke komponenti, utility klase (Tailwind) ni `::ng-deep`.
