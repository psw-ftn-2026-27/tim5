import { afterNextRender, Component, computed, effect, input, output, signal } from '@angular/core';
import * as L from 'leaflet';
import { WalkingRoute, walkingRouter } from './walking-router';

export interface MapPin {
  latitude: number;
  longitude: number;
  label?: string;
}

export interface MapCircle {
  latitude: number;
  longitude: number;
  radiusInMeters: number;
}

const NOVI_SAD: L.LatLngTuple = [45.2551, 19.8451];
const DEFAULT_ZOOM = 13;

// Leaflet's marker images are copied to /leaflet by angular.json.
L.Icon.Default.imagePath = 'leaflet/';

// The picked pin is a round dot instead of Leaflet's default marker, so it never
// looks like one of the pins. Leaflet builds it outside the component's template,
// where map.scss does not reach, so its style is written inline with the tokens.
const PICKED_ICON = L.divIcon({
  className: '',
  html:
    '<div style="width: 100%; height: 100%; box-sizing: border-box; border-radius: 50%;' +
    ' background: var(--color-danger); border: 3px solid var(--color-surface);' +
    ' box-shadow: 0 0 0 1px var(--color-text);"></div>',
  iconSize: [22, 22],
});

let nextMapId = 0;

@Component({
  selector: 'app-map',
  templateUrl: './map.html',
  styleUrl: './map.scss',
})
export class MapView {
  readonly pins = input<MapPin[]>([]);
  readonly showPath = input(false);
  readonly picking = input(false);
  readonly initialLocation = input<MapPin | null>(null);
  readonly circle = input<MapCircle | null>(null);
  readonly routeLength = output<number>();
  readonly locationPicked = output<MapPin>();

  protected readonly mapId = `app-map-${nextMapId++}`;
  protected readonly route = signal<WalkingRoute | null>(null);
  protected readonly routeFailed = signal(false);
  protected readonly routeLengthText = computed(() => this.route()?.lengthKm.toFixed(2));

  private readonly map = signal<L.Map | null>(null);
  private readonly pinLayer = L.layerGroup();
  private readonly routeLayer = L.layerGroup();
  private readonly pickedLayer = L.layerGroup();
  private readonly circleLayer = L.layerGroup();
  private routeRequest = 0;
  // Plain fields, not signals: the first click must not itself re-run the view effect.
  private clicked = false;
  private fittedPins: MapPin[] | null = null;

  constructor() {
    // Leaflet needs the rendered <div> to build the map in, and that <div> carries
    // its id only after the first render.
    afterNextRender(() => {
      const map = L.map(this.mapId).setView(NOVI_SAD, DEFAULT_ZOOM);
      L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap',
      }).addTo(map);
      map.on('click', (event: L.LeafletMouseEvent) => this.pick(event.latlng));
      this.map.set(map);
    });

    // effect() is used because Leaflet draws the map outside Angular's templates,
    // so the signals have to drive it by hand.
    effect((onCleanup) => {
      const map = this.map();
      if (map) {
        this.pinLayer.addTo(map);
        this.routeLayer.addTo(map);
        this.pickedLayer.addTo(map);
        this.circleLayer.addTo(map);
        onCleanup(() => map.remove());
      }
    });
    effect(() => this.drawPins(this.pins(), this.picking()));
    effect(() =>
      this.fitView(
        this.pins(),
        this.map(),
        this.picking() ? this.initialLocation() : null,
        this.circle(),
      ),
    );
    effect(() => this.drawPicked(this.picking() ? this.initialLocation() : null));
    effect(() => this.drawRoute(this.pins(), this.showPath()));
    effect(() => this.drawCircle(this.circle()));
  }

  private drawPins(pins: MapPin[], picking: boolean) {
    this.pinLayer.clearLayers();
    for (const pin of pins) {
      // In picking mode a click on a pin must reach the map, so the pin ignores it.
      const marker = L.marker([pin.latitude, pin.longitude], { interactive: !picking }).addTo(
        this.pinLayer,
      );
      if (pin.label) {
        marker.bindTooltip(pin.label, { permanent: true, direction: 'top', offset: [-16, -12] });
      }
    }
  }

  // The view fits the pins, the circle and, in picking mode, the initial location. After
  // the user's first click the view follows only the pins: a new initial location (usually
  // the page saving that click) must not move what the user is looking at.
  private fitView(
    pins: MapPin[],
    map: L.Map | null,
    initialLocation: MapPin | null,
    circle: MapCircle | null,
  ) {
    if (this.clicked) {
      if (pins === this.fittedPins) {
        return;
      }
      initialLocation = null;
    }
    this.fittedPins = pins;

    const points = initialLocation ? [...pins, initialLocation] : pins;
    if (points.length === 0 && circle === null) {
      map?.setView(NOVI_SAD, DEFAULT_ZOOM);
    } else {
      const bounds = L.latLngBounds(points.map((point) => [point.latitude, point.longitude]));
      if (circle) {
        // toBounds takes the side of the square around the centre, so twice the radius.
        bounds.extend(
          L.latLng(circle.latitude, circle.longitude).toBounds(circle.radiusInMeters * 2),
        );
      }
      map?.fitBounds(bounds, { padding: [40, 40], maxZoom: 16 });
    }
  }

  private pick(clicked: L.LatLng) {
    if (!this.picking()) {
      return;
    }
    this.clicked = true;
    this.drawPicked({ latitude: clicked.lat, longitude: clicked.lng });

    // A click on a repeated copy of the world gives a longitude outside -180..180.
    // wrap() is called only then, because it slightly changes an in-range longitude.
    const outside = clicked.lng < -180 || clicked.lng > 180;
    const wrapped = outside ? clicked.wrap() : clicked;
    this.locationPicked.emit({
      latitude: Math.min(90, Math.max(-90, wrapped.lat)),
      longitude: wrapped.lng,
    });
  }

  private drawPicked(location: MapPin | null) {
    this.pickedLayer.clearLayers();
    if (location) {
      L.marker([location.latitude, location.longitude], {
        icon: PICKED_ICON,
        interactive: false,
      }).addTo(this.pickedLayer);
    }
  }

  private drawCircle(circle: MapCircle | null) {
    this.circleLayer.clearLayers();
    if (circle) {
      // In picking mode a click inside the circle must reach the map, so the circle ignores it.
      L.circle([circle.latitude, circle.longitude], {
        radius: circle.radiusInMeters,
        interactive: false,
      }).addTo(this.circleLayer);
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
