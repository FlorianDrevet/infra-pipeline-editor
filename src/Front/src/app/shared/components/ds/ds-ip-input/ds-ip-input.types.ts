/**
 * Entry mode for {@link DsIpInputComponent}.
 *
 * - `ipv4` renders four octet segments (e.g. `10.0.0.4`).
 * - `cidr` adds a trailing prefix segment after a slash (e.g. `10.0.0.0/16`).
 */
export type DsIpInputMode = 'ipv4' | 'cidr';

/**
 * Static descriptor of a single segment of a segmented IP/CIDR field.
 *
 * A segment owns its numeric bounds and the separator that is rendered
 * immediately before it as a persistent visual mask.
 */
export interface DsIpSegmentDef {
  /** Maximum numeric value the segment accepts (255 for octets, 32 for the prefix). */
  readonly max: number;

  /** Maximum number of digits the segment accepts (3 for octets, 2 for the prefix). */
  readonly maxLength: number;

  /** Separator rendered before the segment, or `null` for the first segment. */
  readonly separatorBefore: string | null;

  /** Semantic role of the segment, used for accessible labelling. */
  readonly kind: 'octet' | 'prefix';
}
