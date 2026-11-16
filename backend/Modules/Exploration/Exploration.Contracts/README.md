# Pozicija turiste (simulator)

Modul Exploration čuva **trenutnu poziciju** svakog turiste (uloga `explorer`). Dok ne postoji
mobilna aplikacija, turista je postavlja ručno, klikom na mapu na stranici
`/exploration/simulator`. Funkcionalnosti koje zavise od toga gde je turista (E21–E23,
G21–G23) čitaju sačuvanu poziciju i ne znaju odakle je stigla, pa se neće menjati kada
simulator zameni mobilna aplikacija.

## Šta se čuva

- Jedna pozicija po korisniku. Novo označavanje zamenjuje prethodno.
- Geografska širina i dužina u decimalnim stepenima (širina −90 do 90, dužina −180 do 180).
- Vreme označavanja (UTC, serverski sat).
- Korisnik koji nikad nije označio poziciju **nema poziciju**. To nisu koordinate 0, 0.

Pozicija može biti zastarela, na primer od juče. Da li je to za tvoju funkcionalnost problem,
odlučuješ ti, na osnovu vremena označavanja.

## Iz drugog modula (npr. Games): kontrakt `IExplorationApi`

```cs
public interface IExplorationApi
{
    Task<UserPositionDto?> GetCurrentPositionAsync(Guid userId);
}

public sealed record UserPositionDto(double Latitude, double Longitude, DateTime MarkedAt);
```

- Vraća `null` kada korisnik nema poziciju. Uvek proveri `null` pre upotrebe.
- `IExplorationApi` ubrizgaj u svoju `*Queries` ili `*Service` klasu, kao bilo koji drugi
  kontrakt. Implementaciju registruje modul Exploration.
- Tvoj `Application` projekat treba referencu na `Exploration.Contracts`. Izmena `.csproj`
  ide preko platformskog tima, po protokolu `docs/protocols/zahtevi-platformskom-timu.md`.
- Ne čuvaj kopiju pozicije u svom modulu. Pitaj kontrakt kada ti treba.

## Unutar modula Exploration (E21–E23)

Poziciju čitaj kroz `ITouristPositionReadRepository.GetByUserAsync(userId)`
(`Exploration.Application/TouristPositions/`), koji vraća `TouristPositionDto?`. Kontrakt
`IExplorationApi` je namenjen drugim modulima, a ne Exploration-u.

## Na klijentu: upućivanje korisnika na simulator

- Iz Exploration-a: link `routerLink="/exploration/simulator"`.
- Iz drugog modula (npr. Games): isto, link na adresu `/exploration/simulator`. Ne uvozi
  stranicu ni komponentu simulatora, jer modul do tuđeg ekrana dolazi samo linkom.
- Krajnje tačke (samo uloga `explorer`): `GET /api/exploration/positions/mine` (404 kada
  pozicija ne postoji) i `PUT /api/exploration/positions/mine` sa telom
  `{ "latitude": …, "longitude": … }`. Drugi moduli ih ne pozivaju, nego čitaju kroz kontrakt.

## Poznato ograničenje

Mapa na simulatoru se još ne centrira na sačuvanu poziciju ako je ona van vidnog polja
Novog Sada. Ispravka je planirana kao zasebna izmena deljene komponente `shared/map/`.
