import { DsTagInputItem, DsTagInputValidator } from '../components/ds/ds-tag-input/ds-tag-input.types';

const InvalidCidrTranslationKey = 'COMMON.VNET_HELP_DIALOG.VALIDATION.INVALID_CIDR';
const InvalidIpv4TranslationKey = 'COMMON.VNET_HELP_DIALOG.VALIDATION.INVALID_IPV4';
const Ipv4SegmentPattern = /^\d{1,3}$/;

type TranslationResolver = (key: string) => string;

export function mapStringsToTagInputItems(values: ReadonlyArray<string> | null | undefined): DsTagInputItem[] {
  return (values ?? [])
    .map((value) => value.trim())
    .filter((value) => value.length > 0)
    .map((value) => ({ value }));
}

export function mapTagInputItemsToStrings(
  values: ReadonlyArray<DsTagInputItem> | DsTagInputItem | string | null | undefined,
): string[] {
  if (typeof values === 'string') {
    return values
      .split(/[\r\n,;]+/)
      .map((value) => value.trim())
      .filter((value) => value.length > 0);
  }

  if (isTagInputItem(values)) {
    const value = values.value.trim();
    return value.length > 0 ? [value] : [];
  }

  return (values ?? [])
    .map((value) => value.value.trim())
    .filter((value) => value.length > 0);
}

export function createVnetCidrTagValidator(resolveTranslation: TranslationResolver): DsTagInputValidator {
  return (value) => isValidCidrBlock(value) ? true : resolveTranslation(InvalidCidrTranslationKey);
}

export function createVnetIpv4TagValidator(resolveTranslation: TranslationResolver): DsTagInputValidator {
  return (value) => isValidIpv4Address(value) ? true : resolveTranslation(InvalidIpv4TranslationKey);
}

function isValidCidrBlock(value: string): boolean {
  const [address, prefix] = value.trim().split('/');
  if (!address || !prefix || !isValidIpv4Address(address) || !/^\d{1,2}$/.test(prefix)) {
    return false;
  }

  const numericPrefix = Number(prefix);
  return numericPrefix >= 0 && numericPrefix <= 32;
}

function isValidIpv4Address(value: string): boolean {
  const segments = value.trim().split('.');
  if (segments.length !== 4) {
    return false;
  }

  return segments.every((segment) => {
    if (!Ipv4SegmentPattern.test(segment)) {
      return false;
    }

    const numericValue = Number(segment);
    return numericValue >= 0 && numericValue <= 255;
  });
}

function isTagInputItem(value: unknown): value is DsTagInputItem {
  return typeof value === 'object'
    && value !== null
    && !Array.isArray(value)
    && 'value' in value
    && typeof value.value === 'string';
}