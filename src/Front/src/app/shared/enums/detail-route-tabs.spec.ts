import {
  getConfigDetailTabIndex,
  getConfigDetailTabQuery,
  getProjectDetailTabIndex,
  getProjectDetailTabQuery,
} from './detail-route-tabs';

describe('detail-route-tabs', () => {
  it('Given_ProjectTabQuery_When_ResolvingSelectedIndex_Then_ReturnsExpectedProjectTabIndices', () => {
    expect(getProjectDetailTabIndex(null)).toBe(0);
    expect(getProjectDetailTabIndex('environments')).toBe(1);
    expect(getProjectDetailTabIndex('naming')).toBe(2);
    expect(getProjectDetailTabIndex('tags')).toBe(3);
    expect(getProjectDetailTabIndex('variables')).toBe(4);
    expect(getProjectDetailTabIndex('unknown')).toBe(0);
  });

  it('Given_ProjectTabIndex_When_ResolvingQueryParam_Then_DefaultTabOmitsTheTabQuery', () => {
    expect(getProjectDetailTabQuery(0)).toBeNull();
    expect(getProjectDetailTabQuery(1)).toBe('environments');
    expect(getProjectDetailTabQuery(2)).toBe('naming');
    expect(getProjectDetailTabQuery(3)).toBe('tags');
    expect(getProjectDetailTabQuery(4)).toBe('variables');
    expect(getProjectDetailTabQuery(5)).toBeNull();
  });

  it('Given_ConfigTabQuery_When_ResolvingSelectedIndex_Then_ReturnsExpectedConfigTabIndices', () => {
    expect(getConfigDetailTabIndex(null)).toBe(0);
    expect(getConfigDetailTabIndex('tags')).toBe(1);
    expect(getConfigDetailTabIndex('naming')).toBe(2);
    expect(getConfigDetailTabIndex('cross-config-refs')).toBe(3);
    expect(getConfigDetailTabIndex('variables')).toBe(4);
    expect(getConfigDetailTabIndex('git')).toBe(5);
    expect(getConfigDetailTabIndex('unknown')).toBe(0);
  });

  it('Given_ConfigTabIndex_When_ResolvingQueryParam_Then_DefaultTabOmitsTheTabQuery', () => {
    expect(getConfigDetailTabQuery(0)).toBeNull();
    expect(getConfigDetailTabQuery(1)).toBe('tags');
    expect(getConfigDetailTabQuery(2)).toBe('naming');
    expect(getConfigDetailTabQuery(3)).toBe('cross-config-refs');
    expect(getConfigDetailTabQuery(4)).toBe('variables');
    expect(getConfigDetailTabQuery(5)).toBe('git');
  });
});