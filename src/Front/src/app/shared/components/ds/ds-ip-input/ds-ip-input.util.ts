import { DsIpInputMode, DsIpSegmentDef } from './ds-ip-input.types';

const OCTET_COUNT = 4;
const OCTET_MAX = 255;
const OCTET_MAX_LENGTH = 3;
const PREFIX_MAX = 32;
const PREFIX_MAX_LENGTH = 2;

/**
 * Builds the ordered segment descriptors for the requested {@link DsIpInputMode}.
 * Always returns four octet segments, plus a prefix segment for `cidr`.
 */
export function getIpSegmentDefs(mode: DsIpInputMode): DsIpSegmentDef[] {
  const octets: DsIpSegmentDef[] = Array.from({ length: OCTET_COUNT }, (_, index) => ({
    max: OCTET_MAX,
    maxLength: OCTET_MAX_LENGTH,
    separatorBefore: index === 0 ? null : '.',
    kind: 'octet',
  }));

  if (mode === 'cidr') {
    return [
      ...octets,
      { max: PREFIX_MAX, maxLength: PREFIX_MAX_LENGTH, separatorBefore: '/', kind: 'prefix' },
    ];
  }

  return octets;
}

/**
 * Normalizes a raw segment string: keeps digits only, trims to the segment
 * length, and clamps the numeric value to the segment maximum. Empty stays empty.
 */
export function sanitizeSegment(raw: string, def: DsIpSegmentDef): string {
  const digits = raw.replace(/\D/g, '').slice(0, def.maxLength);
  if (digits === '') {
    return '';
  }

  return String(Math.min(Number(digits), def.max));
}

/**
 * Decides whether focus should jump to the next segment after the current edit.
 * Advances once the segment is full or no further digit could keep it valid
 * (e.g. an octet at `26` cannot become a valid three-digit value).
 */
export function shouldAdvanceSegment(value: string, def: DsIpSegmentDef): boolean {
  if (value === '') {
    return false;
  }

  if (value.length >= def.maxLength) {
    return true;
  }

  return Number(value) * 10 > def.max;
}

/**
 * Joins segment values into the canonical string contract consumed by the
 * Reactive Form: `a.b.c.d` for `ipv4`, `a.b.c.d/p` for `cidr`. Returns an empty
 * string when every segment is empty so `required` validation behaves correctly.
 */
export function joinSegments(values: readonly string[], mode: DsIpInputMode): string {
  if (values.every((value) => value === '')) {
    return '';
  }

  const address = values.slice(0, OCTET_COUNT).join('.');
  return mode === 'cidr' ? `${address}/${values[OCTET_COUNT] ?? ''}` : address;
}

/**
 * Splits an incoming IP/CIDR string into normalized per-segment values, used by
 * {@link writeValue} and paste handling. Tolerates partial and malformed input.
 */
export function splitToSegments(value: string | null | undefined, mode: DsIpInputMode): string[] {
  const defs = getIpSegmentDefs(mode);
  const result = defs.map(() => '');
  const trimmed = (value ?? '').trim();
  if (trimmed === '') {
    return result;
  }

  let address = trimmed;
  let prefix = '';
  if (mode === 'cidr') {
    const slashIndex = trimmed.indexOf('/');
    if (slashIndex >= 0) {
      address = trimmed.slice(0, slashIndex);
      prefix = trimmed.slice(slashIndex + 1);
    }
  }

  const octets = address.split('.');
  for (let index = 0; index < OCTET_COUNT; index++) {
    result[index] = sanitizeSegment(octets[index] ?? '', defs[index]);
  }

  if (mode === 'cidr') {
    result[OCTET_COUNT] = sanitizeSegment(prefix, defs[OCTET_COUNT]);
  }

  return result;
}
