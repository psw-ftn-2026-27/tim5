# Mapa (`app-map`)

Deljena prikazna komponenta platformskog tima. Prikazuje OpenStreetMap mapu sa pinovima i,
po želji, putanju koja ih povezuje. Ne zavisi ni od jednog modula i ne poziva server
aplikacije. Stranica uvozi `MapView` (i tip `MapPin`) iz `shared/map/map` i dodaje
`MapView` u `imports` svoje komponente.

## Ulazi

| Ulaz       | Tip        | Podrazumevano | Značenje                                |
| ---------- | ---------- | ------------- | --------------------------------------- |
| `pins`     | `MapPin[]` | `[]`          | Pinovi, redom kojim ih putanja obilazi. |
| `showPath` | `boolean`  | `false`       | Da li se pinovi povezuju putanjom.      |

`MapPin` se izvozi iz `map.ts`:

```ts
export interface MapPin {
  latitude: number; // geografska širina, decimalni stepeni
  longitude: number; // geografska dužina, decimalni stepeni
  label?: string; // kratka oznaka koja stoji uz pin
}
```

## Izlazi

| Izlaz         | Tip      | Kada se javlja                                           |
| ------------- | -------- | -------------------------------------------------------- |
| `routeLength` | `number` | Dužina putanje u kilometrima, svaki put kad se izračuna. |

## Ponašanje

- Pogled se podešava tako da se svi pinovi vide. Bez pinova mapa je centrirana na Novi Sad.
- Putanja je **pešačka ruta po ulicama** od prvog do poslednjeg pina, redom kojim su
  predati, bez povratka na prvi. Računa je spoljni servis (javni OSRM server sa pešačkim
  profilom, `routing.openstreetmap.de`), pa je za nju potreban internet.
- Dok je putanja prikazana, na mapi piše „Pešačka ruta po ulicama” i njena dužina.
- Kada se pinovi promene, ruta se ponovo računa i `routeLength` se ponovo javlja.
- Sa manje od dva pina servis se ne poziva, a `routeLength` je `0`.
- Kada rutu nije moguće izračunati, pinovi ostaju, linije nema, na mapi piše „Ruta trenutno
  nije dostupna.”, a `routeLength` se **ne** javlja. Stranica zato sama briše staru dužinu
  kad promeni pinove ili isključi putanju.

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

Živ primer je demo stranica `/platform/map-demo` (`core/map-demo/`).
