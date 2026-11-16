import { ComponentFixture, TestBed } from '@angular/core/testing';
import * as L from 'leaflet';
import { MapPin, MapView } from './map';
import { WalkingRoute, walkingRouter } from './walking-router';

const PINS: MapPin[] = [
  { latitude: 45.2551, longitude: 19.8451, label: 'Trg slobode' },
  { latitude: 45.2556, longitude: 19.851, label: 'Dunavski park' },
  { latitude: 45.2522, longitude: 19.8628 },
];

const ROUTE: WalkingRoute = {
  line: [
    [45.2551, 19.8451],
    [45.2556, 19.851],
    [45.2522, 19.8628],
  ],
  lengthKm: 2.5,
};

describe('MapView', () => {
  let fixture: ComponentFixture<MapView>;
  let lengths: number[];
  let picked: MapPin[];

  beforeEach(() => {
    fixture = TestBed.createComponent(MapView);
    lengths = [];
    picked = [];
    fixture.componentInstance.routeLength.subscribe((length) => lengths.push(length));
    fixture.componentInstance.locationPicked.subscribe((location) => picked.push(location));
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  async function show(pins: MapPin[], showPath: boolean) {
    fixture.componentRef.setInput('pins', pins);
    fixture.componentRef.setInput('showPath', showPath);
    await fixture.whenStable();
  }

  function markers(): L.Marker[] {
    return fixture.componentInstance['pinLayer'].getLayers() as L.Marker[];
  }

  function lines(): L.Polyline[] {
    return fixture.componentInstance['routeLayer'].getLayers() as L.Polyline[];
  }

  function pickedMarkers(): L.Marker[] {
    return fixture.componentInstance['pickedLayer'].getLayers() as L.Marker[];
  }

  function pickedAt(): number[][] {
    return pickedMarkers().map((marker) => [marker.getLatLng().lat, marker.getLatLng().lng]);
  }

  async function pickOn(pins: MapPin[], initialLocation: MapPin | null) {
    fixture.componentRef.setInput('pins', pins);
    fixture.componentRef.setInput('picking', true);
    fixture.componentRef.setInput('initialLocation', initialLocation);
    await fixture.whenStable();
  }

  function click(latitude: number, longitude: number) {
    const map = fixture.componentInstance['map']() as L.Map;
    map.fire('click', { latlng: L.latLng(latitude, longitude) });
  }

  function text(): string {
    return (fixture.nativeElement as HTMLElement).textContent ?? '';
  }

  it('shows every pin at its coordinates', async () => {
    await show(PINS, false);

    const shown = markers().map((marker) => [marker.getLatLng().lat, marker.getLatLng().lng]);
    expect(shown).toEqual(PINS.map((pin) => [pin.latitude, pin.longitude]));
  });

  it('does not ask for a route or draw a line when the path is off', async () => {
    const findRoute = vi.spyOn(walkingRouter, 'findRoute');

    await show(PINS, false);

    expect(findRoute).not.toHaveBeenCalled();
    expect(lines()).toHaveLength(0);
  });

  it('asks for the route through the pins in the given order, without returning to the first', async () => {
    const findRoute = vi.spyOn(walkingRouter, 'findRoute').mockResolvedValue(ROUTE);

    await show(PINS, true);

    expect(findRoute).toHaveBeenCalledExactlyOnceWith(PINS);
  });

  it('shows the walking route label with its length and reports the same length', async () => {
    vi.spyOn(walkingRouter, 'findRoute').mockResolvedValue(ROUTE);

    await show(PINS, true);

    expect(lines()).toHaveLength(1);
    expect(text()).toContain('Pešačka ruta po ulicama: 2.50 km');
    expect(lengths).toEqual([2.5]);
  });

  it('asks for the route again and reports the new length when the pins change', async () => {
    const findRoute = vi
      .spyOn(walkingRouter, 'findRoute')
      .mockResolvedValueOnce(ROUTE)
      .mockResolvedValueOnce({ line: ROUTE.line.slice(0, 2), lengthKm: 1.2 });
    await show(PINS, true);

    await show(PINS.slice(0, 2), true);

    expect(findRoute).toHaveBeenLastCalledWith(PINS.slice(0, 2));
    expect(lengths).toEqual([2.5, 1.2]);
  });

  it('does not ask for a route and reports length 0 for fewer than two pins', async () => {
    const findRoute = vi.spyOn(walkingRouter, 'findRoute');

    await show(PINS.slice(0, 1), true);

    expect(findRoute).not.toHaveBeenCalled();
    expect(lengths).toEqual([0]);
  });

  it('keeps the pins, draws no line, says the route is unavailable and reports no length on failure', async () => {
    vi.spyOn(walkingRouter, 'findRoute').mockResolvedValue(null);

    await show(PINS, true);

    expect(markers()).toHaveLength(PINS.length);
    expect(lines()).toHaveLength(0);
    expect(text()).toContain('Ruta trenutno nije dostupna.');
    expect(lengths).toEqual([]);
  });

  it('places the picked pin where the map is clicked and reports those coordinates', async () => {
    await pickOn([], null);

    click(45.25, 19.84);

    expect(pickedAt()).toEqual([[45.25, 19.84]]);
    expect(picked).toEqual([{ latitude: 45.25, longitude: 19.84 }]);
  });

  it('keeps only one picked pin, at the new place, after a second click', async () => {
    await pickOn([], null);

    click(45.25, 19.84);
    click(45.26, 19.85);

    expect(pickedAt()).toEqual([[45.26, 19.85]]);
    expect(picked).toEqual([
      { latitude: 45.25, longitude: 19.84 },
      { latitude: 45.26, longitude: 19.85 },
    ]);
  });

  it('shows the picked pin at the initial location and reports nothing before a click', async () => {
    await pickOn(PINS, { latitude: 45.2522, longitude: 19.8628 });

    expect(pickedAt()).toEqual([[45.2522, 19.8628]]);
    expect(picked).toEqual([]);
  });

  it('reports the clicked coordinates and leaves the pins unchanged on a click at a pin', async () => {
    await pickOn(PINS, null);

    click(PINS[0].latitude, PINS[0].longitude);

    expect(picked).toEqual([{ latitude: PINS[0].latitude, longitude: PINS[0].longitude }]);
    const shown = markers().map((marker) => [marker.getLatLng().lat, marker.getLatLng().lng]);
    expect(shown).toEqual(PINS.map((pin) => [pin.latitude, pin.longitude]));
  });

  it('places no pin and reports nothing on a click outside picking mode', async () => {
    await show(PINS, false);

    click(45.25, 19.84);

    expect(pickedMarkers()).toHaveLength(0);
    expect(picked).toEqual([]);
  });

  it('reports a longitude within -180..180 on a click at a repeated copy of the world', async () => {
    await pickOn([], null);

    click(45.25, 19.84 + 360);

    expect(picked).toHaveLength(1);
    expect(picked[0].latitude).toBe(45.25);
    expect(picked[0].longitude).toBeCloseTo(19.84);
  });
});
