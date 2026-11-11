import { afterNextRender, Component, computed, effect, input, output, signal } from '@angular/core';
import * as L from 'leaflet';
import { WalkingRoute, walkingRouter } from './walking-router';

export interface MapPin {
  latitude: number;
  longitude: number;
  label?: string;
}

const NOVI_SAD: L.LatLngTuple = [45.2551, 19.8451];
const DEFAULT_ZOOM = 13;

// Leaflet's marker images are copied to /leaflet by angular.json.
L.Icon.Default.imagePath = 'leaflet/';

let nextMapId = 0;

@Component({
  selector: 'app-map',
  templateUrl: './map.html',
  styleUrl: './map.scss',
})
export class MapView {
  readonly pins = input<MapPin[]>([]);
  readonly showPath = input(false);
  readonly routeLength = output<number>();

  protected readonly mapId = `app-map-${nextMapId++}`;
  protected readonly route = signal<WalkingRoute | null>(null);
  protected readonly routeFailed = signal(false);
  protected readonly routeLengthText = computed(() => this.route()?.lengthKm.toFixed(2));

  private readonly map = signal<L.Map | null>(null);
  private readonly pinLayer = L.layerGroup();
  private readonly routeLayer = L.layerGroup();
  private routeRequest = 0;

  constructor() {
    // Leaflet needs the rendered <div> to build the map in, and that <div> carries
    // its id only after the first render.
    afterNextRender(() => {
      const map = L.map(this.mapId).setView(NOVI_SAD, DEFAULT_ZOOM);
      L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap',
      }).addTo(map);
      this.map.set(map);
    });

    // effect() is used because Leaflet draws the map outside Angular's templates,
    // so the signals have to drive it by hand.
    effect((onCleanup) => {
      const map = this.map();
      if (map) {
        this.pinLayer.addTo(map);
        this.routeLayer.addTo(map);
        onCleanup(() => map.remove());
      }
    });
    effect(() => this.drawPins(this.pins(), this.map()));
    effect(() => this.drawRoute(this.pins(), this.showPath()));
  }

  private drawPins(pins: MapPin[], map: L.Map | null) {
    this.pinLayer.clearLayers();
    for (const pin of pins) {
      const marker = L.marker([pin.latitude, pin.longitude]).addTo(this.pinLayer);
      if (pin.label) {
        marker.bindTooltip(pin.label, { permanent: true, direction: 'top', offset: [-16, -12] });
      }
    }

    if (pins.length === 0) {
      map?.setView(NOVI_SAD, DEFAULT_ZOOM);
    } else {
      const bounds = L.latLngBounds(pins.map((pin) => [pin.latitude, pin.longitude]));
      map?.fitBounds(bounds, { padding: [40, 40], maxZoom: 16 });
    }
  }

  private async drawRoute(pins: MapPin[], showPath: boolean) {
    // A newer call makes the answer to an older one irrelevant.
    const request = ++this.routeRequest;
    this.routeLayer.clearLayers();
    this.route.set(null);
    this.routeFailed.set(false);

    if (!showPath) {
      return;
    }
    if (pins.length < 2) {
      this.route.set({ line: [], lengthKm: 0 });
      this.routeLength.emit(0);
      return;
    }

    const route = await walkingRouter.findRoute(pins);
    if (request !== this.routeRequest) {
      return;
    }
    if (route === null) {
      this.routeFailed.set(true);
      return;
    }
    L.polyline(route.line).addTo(this.routeLayer);
    this.route.set(route);
    this.routeLength.emit(route.lengthKm);
  }
}
