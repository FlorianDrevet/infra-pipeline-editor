import { sortHierarchicalEntries } from './project-detail-tree-ordering.helper';

describe('project detail tree ordering helper', () => {
  it('sorts hierarchical entries deterministically', () => {
    const entries = [
      'Common/modules/KeyVault/types.bicep',
      'Common/modules/ContainerRegistry/types.bicep',
      'Common/types.bicep',
    ];

    const sortedEntries = sortHierarchicalEntries(entries);

    expect(sortedEntries).toEqual([
      'Common/modules/ContainerRegistry/types.bicep',
      'Common/modules/KeyVault/types.bicep',
      'Common/types.bicep',
    ]);
  });

  it('returns a new array without mutating the input entries', () => {
    const entries = ['b', 'a'];

    const sortedEntries = sortHierarchicalEntries(entries);

    expect(sortedEntries).toEqual(['a', 'b']);
    expect(entries).toEqual(['b', 'a']);
    expect(sortedEntries).not.toBe(entries);
  });

  it('sorts numeric path segments deterministically', () => {
    const entries = ['modules/file10.bicep', 'modules/file2.bicep', 'modules/file1.bicep'];

    const sortedEntries = sortHierarchicalEntries(entries);

    expect(sortedEntries).toEqual([
      'modules/file1.bicep',
      'modules/file2.bicep',
      'modules/file10.bicep',
    ]);
  });
});