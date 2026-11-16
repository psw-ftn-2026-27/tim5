import { Component, computed, signal } from '@angular/core';
import { MapPin, MapView } from '../../shared/map/map';

interface PinSet {
  name: string;
  pins: MapPin[];
}

const TRG_SLOBODE: MapPin = { latitude: 45.2551, longitude: 19.8451, label: 'Trg slobode' };
const DUNAVSKI_PARK: MapPin = { latitude: 45.2556, longitude: 19.851, label: 'Dunavski park' };
const TVRDJAVA: MapPin = {
  latitude: 45.2522,
  longitude: 19.8628,
  label: 'Petrovaradinska tvrđava',
};

const PIN_SETS: PinSet[] = [
  { name: 'Tri pina u Novom Sadu', pins: [TRG_SLOBODE, DUNAVSKI_PARK, TVRDJAVA] },
  {
    name: 'Pet pinova u Novom Sadu',
    pins: [
      { latitude: 45.2654, longitude: 19.8299, label: 'Železnička stanica' },
      TRG_SLOBODE,
      DUNAVSKI_PARK,
      { latitude: 45.2426, longitude: 19.8566, label: 'Štrand' },
      { latitude: 45.2469, longitude: 19.8452, label: 'SPENS' },
    ],
  },
];

// The location the "Izmena postojeće lokacije" button pretends a key point already has.
const EXISTING_LOCATION: MapPin = { latitude: 45.2517, longitude: 19.8369 };

@Component({
  selector: 'app-map-demo',
  imports: [MapView],
  templateUrl: './map-demo.html',
  styleUrl: './map-demo.scss',
})
export class MapDemo {
  protected readonly pinSets = PIN_SETS;
  protected readonly selectedSet = signal<PinSet | null>(PIN_SETS[0]);
  protected readonly showPath = signal(false);
  protected readonly routeLength = signal<number | null>(null);

  protected readonly pickingPins = [TRG_SLOBODE, DUNAVSKI_PARK, TVRDJAVA];
  protected readonly editing = signal(false);
  protected readonly initialLocation = computed(() => (this.editing() ? EXISTING_LOCATION : null));
  protected readonly location = signal<MapPin | null>(null);
  protected readonly lastReceived = signal<MapPin | null>(null);

  protected selectSet(set: PinSet | null) {
    if (set === this.selectedSet()) {
      return;
    }
    this.selectedSet.set(set);
    this.routeLength.set(null);
  }

  protected setShowPath(show: boolean) {
    this.showPath.set(show);
    this.routeLength.set(null);
  }

  protected setEditing(editing: boolean) {
    if (editing === this.editing()) {
      return;
    }
    this.editing.set(editing);
    this.location.set(editing ? EXISTING_LOCATION : null);
    this.lastReceived.set(null);
  }

  protected receiveLocation(location: MapPin) {
    this.location.set(location);
    this.lastReceived.set(location);
  }
}
