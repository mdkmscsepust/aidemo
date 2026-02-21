const ENCODING_TABLE = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_';

interface Decoder {
  data: string;
  index: number;
}

function decodeChar(char: string): number {
  const index = ENCODING_TABLE.indexOf(char);
  if (index < 0) {
    throw new Error('Invalid polyline character.');
  }
  return index;
}

function decodeUnsignedVarint(decoder: Decoder): number {
  let result = 0;
  let shift = 0;
  while (true) {
    const value = decodeChar(decoder.data[decoder.index++]);
    result |= (value & 0x1f) << shift;
    if ((value & 0x20) === 0) {
      break;
    }
    shift += 5;
  }
  return result;
}

function decodeSignedVarint(decoder: Decoder): number {
  let value = decodeUnsignedVarint(decoder);
  const negative = value & 1;
  value >>= 1;
  return negative ? -value : value;
}

export function decodeFlexiblePolyline(polyline: string): [number, number][] {
  if (!polyline) {
    return [];
  }

  const decoder: Decoder = { data: polyline, index: 0 };
  const header = decodeUnsignedVarint(decoder);
  const precision = (header >> 3) & 0x0f;
  const thirdDim = (header >> 7) & 0x07;
  const thirdDimPrecision = (header >> 10) & 0x0f;

  const factor = Math.pow(10, precision);
  const thirdFactor = Math.pow(10, thirdDimPrecision);

  let lat = 0;
  let lng = 0;
  let z = 0;

  const coordinates: [number, number][] = [];
  while (decoder.index < decoder.data.length) {
    lat += decodeSignedVarint(decoder);
    lng += decodeSignedVarint(decoder);

    if (thirdDim !== 0) {
      z += decodeSignedVarint(decoder);
      void (z / thirdFactor);
    }

    coordinates.push([lng / factor, lat / factor]);
  }

  return coordinates;
}
