# Mapa (`app-map`)

Deljena prikazna komponenta platformskog tima. Prikazuje OpenStreetMap mapu sa pinovima i,
po želji, putanju koja ih povezuje. U režimu biranja korisnik klikom bira jednu lokaciju.
Ne zavisi ni od jednog modula i ne poziva server aplikacije. Stranica uvozi `MapView` (i tip `MapPin`) iz `shared/map/map` i dodaje
`MapView` u `imports` svoje komponente.

## Ulazi

| Ulaz              | Tip              | Podrazumevano | Značenje                                                      |
| ----------------- | ---------------- | ------------- | ------------------------------------------------------------- |
| `pins`            | `MapPin[]`       | `[]`          | Pinovi, redom kojim ih putanja obilazi.                       |
| `showPath`        | `boolean`        | `false`       | Da li se pinovi povezuju putanjom.                            |
| `picking`         | `boolean`        | `false`       | Da li je uključen režim biranja.                              |
| `initialLocation` | `MapPin \| null` | `null`        | Početna lokacija pina za biranje (izmena postojeće lokacije). |

`MapPin` se izvozi iz `map.ts`:

```ts
export interface MapPin {
  latitude: number; // geografska širina, decimalni stepeni
  longitude: number; // geografska dužina, decimalni stepeni
  label?: string; // kratka oznaka koja stoji uz pin
}
```

## Izlazi

| Izlaz            | Tip      | Kada se javlja                                                |
| ---------------- | -------- | ------------------------------------------------------------- |
| `routeLength`    | `number` | Dužina putanje u kilometrima, svaki put kad se izračuna.      |
| `locationPicked` | `MapPin` | Koordinate (bez `label`) posle svakog klika u režimu biranja. |

## Ponašanje

- Pogled se podešava tako da se svi pinovi vide. Bez pinova mapa je centrirana na Novi Sad.
- U režimu biranja pogled obuhvata i `initialLocation`. Kada je ona jedina tačka, mapa je
  centrirana na nju, sa istim najvećim uvećanjem kao kod pinova. Van režima biranja
  `initialLocation` ne utiče na pogled.
- Dok korisnik ne klikne na mapu, pogled se ponovo podešava svaki put kad stigne nova
  `initialLocation` (na primer kad se sačuvana lokacija učita sa servera). Posle prvog klika
  promena `initialLocation` ne pomera pogled, pa korisnik vidi mapu kako ju je ostavio.
- Putanja je **pešačka ruta po ulicama** od prvog do poslednjeg pina, redom kojim su
  predati, bez povratka na prvi. Računa je spoljni servis (javni OSRM server sa pešačkim
  profilom, `routing.openstreetmap.de`), pa je za nju potreban internet.
- Dok je putanja prikazana, na mapi piše „Pešačka ruta po ulicama” i njena dužina.
- Kada se pinovi promene, ruta se ponovo računa i `routeLength` se ponovo javlja.
- Sa manje od dva pina servis se ne poziva, a `routeLength` je `0`.
- Kada rutu nije moguće izračunati, pinovi ostaju, linije nema, na mapi piše „Ruta trenutno
  nije dostupna.”, a `routeLength` se **ne** javlja. Stranica zato sama briše staru dužinu
  kad promeni pinove ili isključi putanju.

## Režim biranja

- Na mapi postoji najviše jedan pin koji se bira. Izgleda kao okrugla tačka, drugačije od
  pinova iz `pins`, koji ostaju prikazani radi orijentacije.
- Svaki klik postavlja pin na mesto klika i javlja njegove koordinate kroz `locationPicked`;
  prethodni pin nestaje. Klik na postojeći pin bira mesto klika, a ne taj pin.
- Sa `initialLocation` pin je na toj lokaciji od početka, a `locationPicked` se ne javlja dok
  korisnik ne klikne.
- Javljena širina je između −90 i 90, a dužina između −180 i 180, i kada korisnik klikne na
  ponovljenu kopiju sveta.
- Van režima biranja klik ne postavlja pin i ništa ne javlja.
- Lokacija se menja samo klikom, ne prevlačenjem. Poruku „lokacija nije izabrana”, validaciju
  i čuvanje radi forma koja koristi komponentu.

## Primer bez putanje

```html
<app-map [pins]="pins()" />
```

## Primer sa putanjom

```ts
protected readonly pins = signal<MapPin[]>([
  { latitude: 45.2551, longitude: 19.8451, label: 'Trg slobode' },
  { latitude: 45.2522, longitude: 19.8628, label: 'Petrovaradinska tvrđava' },
]);
protected readonly routeLength = signal<number | null>(null);
```

```html
<app-map [pins]="pins()" [showPath]="true" (routeLength)="routeLength.set($event)" />
```

## Primer biranja u formi

Stranica forme čuva izabranu lokaciju u signalu. Kod izmene postojećeg zapisa predaje
njegovu lokaciju kao `initialLocation`, a kod novog zapisa `null`.

```ts
protected readonly existingLocation: MapPin | null = null; // ili lokacija zapisa koji se menja
protected readonly location = signal<MapPin | null>(this.existingLocation);
```

```html
<app-map
  [pins]="tourPins()"
  [picking]="true"
  [initialLocation]="existingLocation"
  (locationPicked)="location.set($event)"
/>

@if (location(); as picked) {
<p>Izabrano: {{ picked.latitude }}, {{ picked.longitude }}</p>
} @else {
<p>Lokacija nije izabrana.</p>
}
```

Pri slanju forme stranica čita `location()` i šalje `latitude` i `longitude` serveru.

Živ primer je demo stranica `/platform/map-demo` (`core/map-demo/`).
