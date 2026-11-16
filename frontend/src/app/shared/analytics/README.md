# Beleženje događaja (`Analytics`)

Deljeni servis platformskog tima. Modul preko njega beleži važne radnje korisnika u Google
Analytics-u, bez ikakvog znanja o GA-u. Modul nikad ne zove GA direktno, već samo
`Analytics` iz `shared/analytics/analytics`.

## Primer: događaj `tour_published` na stranici `MyTours`

**1. Dodaj događaj u katalog.** Katalog je tip `AnalyticsEvent` u `analytics.ts`. Svaki
događaj je jedan interfejs sa imenom i parametrima koje to ime nosi, i jedan član unije.

```ts
interface TourPublished {
  name: 'tour_published';
  params: { tour_id: string };
}

export type AnalyticsEvent = TourPublished;
```

Sledeći događaj je još jedan interfejs, dopisan u uniju iza `|`:
`export type AnalyticsEvent = TourPublished | CartItemAdded;`.

Ime ili parametar koji nisu u katalogu ne prolaze kompajliranje.

**2. Zabeleži događaj iz stranice.** Stranica ubrizgava `Analytics` i zove `record(...)`
odmah posle uspeha komande, pored ponovnog učitavanja resursa. Poziv ne ide u servis grupe,
jer on nosi samo komande.

```ts
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly analytics = inject(Analytics);

  protected async publish(event: Event): Promise<void> {
    // ...
    try {
      await this.tourAuthoring.publish(tourId, { isFree: choice === 'Free', amount: amount });
      this.tours.reload();
      this.analytics.record({ name: 'tour_published', params: { tour_id: tourId } });
      this.closeForm();
    } catch (failure) {
      // na grešku se događaj ne beleži
    }
  }
}
```

Na klik, na otvaranje forme, na grešku ni na odustajanje se ne beleži ništa.

## Pravila

- Ime događaja je u obliku `<predmet>_<radnja u prošlom vremenu>`, malim slovima sa
  podvlakom: `tour_published`. Imena parametara su takođe malim slovima sa podvlakom:
  `tour_id`.
- Ulogu trenutnog korisnika (`administrator`, `explorer` ili `guest`) dodaje sam servis.
  Modul je ne navodi u parametrima.
- Nijedan parametar ne sme da bude lični podatak: ime, mejl, korisničko ime ni
  identifikator korisnika.
- Bez saglasnosti korisnika ili bez podešenog GA identifikatora (`gaMeasurementId`) ništa
  se ne šalje. Poziv `record(...)` tada ne radi ništa, a stranica radi normalno.
- Promene rute beleži GA4 _enhanced measurement_ (uključen podrazumevano u novom GA4
  svojstvu), pa stranice same ne beleže preglede stranica.
