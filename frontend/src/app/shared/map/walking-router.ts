import * as L from 'leaflet';
import type { MapPin } from './map';

export interface WalkingRoute {
  line: L.LatLngTuple[];
  lengthKm: number;
}

// The public OSRM server run by FOSSGIS, with the walking (foot) profile.
const ROUTING_SERVER = 'https://routing.openstreetmap.de/routed-foot/route/v1';

// The only place that talks to the routing service. The map component calls
// walkingRouter.findRoute, and tests replace that method with a fake.
export const walkingRouter = {
  async findRoute(pins: MapPin[]): Promise<WalkingRoute | null> {
    try {
      const routing = await loadRoutingMachine();
      const router = routing.osrmv1({ serviceUrl: ROUTING_SERVER, profile: 'foot' });
      const waypoints = pins.map((pin) => routing.waypoint(L.latLng(pin.latitude, pin.longitude)));
      return await new Promise((resolve) => {
        router.route(waypoints, (error?: L.Routing.IError, routes?: L.Routing.IRoute[]) => {
          const route = routes?.[0];
          if (error || !route?.coordinates || !route.summary) {
            resolve(null);
            return;
          }
          resolve({
            line: route.coordinates.map((point): L.LatLngTuple => [point.lat, point.lng]),
            lengthKm: route.summary.totalDistance / 1000,
          });
        });
      });
    } catch {
      return null;
    }
  },
};

// Leaflet Routing Machine is an older plugin: it reads Leaflet from the global
// variable `L` and adds `L.Routing` to it. So the global is set first, and the
// plugin is loaded only when a route is needed for the first time.
async function loadRoutingMachine(): Promise<typeof L.Routing> {
  const global = window as unknown as { L?: typeof L };
  global.L ??= { ...L };
  await import('leaflet-routing-machine');
  return global.L.Routing;
}
