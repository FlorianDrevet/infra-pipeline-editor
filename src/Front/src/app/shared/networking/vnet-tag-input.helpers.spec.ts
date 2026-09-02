import { createVnetCidrTagValidator, createVnetIpv4TagValidator, mapStringsToTagInputItems, mapTagInputItemsToStrings } from './vnet-tag-input.helpers';

describe('vnet tag input helpers', () => {
  it('maps string arrays to tag items and back without losing order', () => {
    const source = ['10.0.0.0/16', '10.0.0.4'];

    const tagItems = mapStringsToTagInputItems(source);

    expect(tagItems).toEqual([
      { value: '10.0.0.0/16' },
      { value: '10.0.0.4' },
    ]);
    expect(mapTagInputItemsToStrings(tagItems)).toEqual(source);
  });

  it('rejects malformed CIDR values with localized feedback', () => {
    const validator = createVnetCidrTagValidator((key) => key);

    expect(validator('10.0.0.0/16', [])).toBeTrue();
    expect(validator('10.0.0.0', [])).toBe('COMMON.VNET_HELP_DIALOG.VALIDATION.INVALID_CIDR');
  });

  it('rejects malformed IPv4 values with localized feedback', () => {
    const validator = createVnetIpv4TagValidator((key) => key);

    expect(validator('10.0.0.4', [])).toBeTrue();
    expect(validator('10.0.0.256', [])).toBe('COMMON.VNET_HELP_DIALOG.VALIDATION.INVALID_IPV4');
  });
});