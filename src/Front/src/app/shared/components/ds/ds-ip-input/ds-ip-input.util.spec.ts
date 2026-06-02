import { DsIpSegmentDef } from './ds-ip-input.types';
import {
  getIpSegmentDefs,
  joinSegments,
  sanitizeSegment,
  shouldAdvanceSegment,
  splitToSegments,
} from './ds-ip-input.util';

const octetDef: DsIpSegmentDef = { max: 255, maxLength: 3, separatorBefore: '.', kind: 'octet' };
const prefixDef: DsIpSegmentDef = { max: 32, maxLength: 2, separatorBefore: '/', kind: 'prefix' };

describe('ds-ip-input.util', () => {
  describe('getIpSegmentDefs', () => {
    it('returns four octet segments for ipv4', () => {
      const defs = getIpSegmentDefs('ipv4');
      expect(defs.length).toBe(4);
      expect(defs[0].separatorBefore).toBeNull();
      expect(defs[1].separatorBefore).toBe('.');
      expect(defs.every((d) => d.kind === 'octet' && d.max === 255 && d.maxLength === 3)).toBeTrue();
    });

    it('appends a slash-prefixed prefix segment for cidr', () => {
      const defs = getIpSegmentDefs('cidr');
      expect(defs.length).toBe(5);
      expect(defs[4]).toEqual({ max: 32, maxLength: 2, separatorBefore: '/', kind: 'prefix' });
    });
  });

  describe('sanitizeSegment', () => {
    it('keeps digits only', () => {
      expect(sanitizeSegment('1a2b', octetDef)).toBe('12');
    });

    it('trims to the segment length', () => {
      expect(sanitizeSegment('1999', octetDef)).toBe('199');
      expect(sanitizeSegment('123', prefixDef)).toBe('12');
    });

    it('clamps to the segment maximum', () => {
      expect(sanitizeSegment('300', octetDef)).toBe('255');
      expect(sanitizeSegment('99', prefixDef)).toBe('32');
    });

    it('preserves empty input', () => {
      expect(sanitizeSegment('', octetDef)).toBe('');
      expect(sanitizeSegment('abc', octetDef)).toBe('');
    });
  });

  describe('shouldAdvanceSegment', () => {
    it('does not advance on empty value', () => {
      expect(shouldAdvanceSegment('', octetDef)).toBeFalse();
    });

    it('advances when the segment is full', () => {
      expect(shouldAdvanceSegment('192', octetDef)).toBeTrue();
      expect(shouldAdvanceSegment('16', prefixDef)).toBeTrue();
    });

    it('advances when no further digit can keep the value valid', () => {
      expect(shouldAdvanceSegment('26', octetDef)).toBeTrue();
      expect(shouldAdvanceSegment('4', prefixDef)).toBeTrue();
    });

    it('waits for more digits when a longer value is still reachable', () => {
      expect(shouldAdvanceSegment('25', octetDef)).toBeFalse();
      expect(shouldAdvanceSegment('5', octetDef)).toBeFalse();
      expect(shouldAdvanceSegment('3', prefixDef)).toBeFalse();
    });
  });

  describe('joinSegments', () => {
    it('returns empty string when all segments are empty', () => {
      expect(joinSegments(['', '', '', ''], 'ipv4')).toBe('');
      expect(joinSegments(['', '', '', '', ''], 'cidr')).toBe('');
    });

    it('joins octets with dots for ipv4', () => {
      expect(joinSegments(['10', '0', '0', '4'], 'ipv4')).toBe('10.0.0.4');
    });

    it('joins octets and prefix for cidr', () => {
      expect(joinSegments(['10', '0', '0', '0', '16'], 'cidr')).toBe('10.0.0.0/16');
    });

    it('emits a non-empty, invalid string for partial input so validators can reject it', () => {
      expect(joinSegments(['10', '0', '', ''], 'ipv4')).toBe('10.0..');
      expect(joinSegments(['10', '0', '0', '0', ''], 'cidr')).toBe('10.0.0.0/');
    });
  });

  describe('splitToSegments', () => {
    it('returns empty segments for empty input', () => {
      expect(splitToSegments('', 'ipv4')).toEqual(['', '', '', '']);
      expect(splitToSegments(null, 'cidr')).toEqual(['', '', '', '', '']);
    });

    it('splits a complete ipv4 address', () => {
      expect(splitToSegments('10.0.0.4', 'ipv4')).toEqual(['10', '0', '0', '4']);
    });

    it('splits a complete cidr block', () => {
      expect(splitToSegments('10.0.0.0/16', 'cidr')).toEqual(['10', '0', '0', '0', '16']);
    });

    it('sanitizes and clamps each parsed segment', () => {
      expect(splitToSegments('300.0.0.999/99', 'cidr')).toEqual(['255', '0', '0', '255', '32']);
    });

    it('tolerates a partial cidr without a slash', () => {
      expect(splitToSegments('10.0', 'cidr')).toEqual(['10', '0', '', '', '']);
    });

    it('round-trips with joinSegments', () => {
      expect(joinSegments(splitToSegments('172.16.0.0/12', 'cidr'), 'cidr')).toBe('172.16.0.0/12');
    });
  });
});
