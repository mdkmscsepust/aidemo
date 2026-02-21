export interface LocationInput {
  text?: string;
  lat?: number;
  lng?: number;
}

export interface RouteRequest {
  from: LocationInput;
  to: LocationInput;
  transportMode: string;
  alternatives: number;
  traffic: boolean;
  avoidTolls: boolean;
  avoidHighways: boolean;
}

export interface Bbox {
  top: number;
  left: number;
  bottom: number;
  right: number;
}

export interface RouteItem {
  id: string;
  rank: number;
  etaSeconds: number;
  baseDurationSeconds: number;
  distanceMeters: number;
  trafficPenaltySeconds: number;
  polyline: string;
  bbox: Bbox;
  warnings: string[];
}

export interface RouteResponse {
  routes: RouteItem[];
}

export interface TrafficIncident {
  id: string;
  type: string;
  severity: string;
  description: string;
  lat: number;
  lng: number;
  startTime?: string;
  endTime?: string;
}

export interface TrafficResponse {
  incidents: TrafficIncident[];
}

export interface RouteState {
  loading: boolean;
  from?: LocationInput;
  to?: LocationInput;
  routes: RouteItem[];
  selectedRouteId?: string;
  incidents: TrafficIncident[];
  error?: string;
}
