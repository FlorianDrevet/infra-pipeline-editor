import { sortResourceTypesAlphabetically } from './resource-type-sort.helpers';

describe('resource type sort helpers', () => {
  it('sorts resource types with a locale-aware alphabetical comparator', () => {
    const sorted = sortResourceTypesAlphabetically(['Storage10', 'storage2', 'ApplicationInsights', 'AppConfiguration']);

    expect(sorted).toEqual(['AppConfiguration', 'ApplicationInsights', 'storage2', 'Storage10']);
  });
});