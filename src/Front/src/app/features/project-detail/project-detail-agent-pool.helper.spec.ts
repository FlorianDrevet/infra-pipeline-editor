import {
  hasProjectDetailAgentPoolChanges,
  normalizeProjectDetailAgentPoolName,
  resolveProjectDetailAgentPoolDraft,
  resolveProjectDetailAgentPoolValue,
} from './project-detail-agent-pool.helper';

describe('project detail agent pool helper', () => {
  it('normalizes null, empty, and whitespace agent pool names to null', () => {
    expect(normalizeProjectDetailAgentPoolName(null)).toBeNull();
    expect(normalizeProjectDetailAgentPoolName('')).toBeNull();
    expect(normalizeProjectDetailAgentPoolName('   ')).toBeNull();
  });

  it('creates a custom-pool draft only when the normalized project value is present', () => {
    expect(resolveProjectDetailAgentPoolDraft('  build-pool  ')).toEqual({
      useCustomPool: true,
      agentPoolName: 'build-pool',
    });

    expect(resolveProjectDetailAgentPoolDraft('   ')).toEqual({
      useCustomPool: false,
      agentPoolName: null,
    });
  });

  it('resolves the effective agent pool value from the toggle state and trims whitespace', () => {
    expect(resolveProjectDetailAgentPoolValue({ useCustomPool: true, agentPoolName: '  build-pool  ' })).toBe('build-pool');
    expect(resolveProjectDetailAgentPoolValue({ useCustomPool: true, agentPoolName: '   ' })).toBeNull();
    expect(resolveProjectDetailAgentPoolValue({ useCustomPool: false, agentPoolName: 'build-pool' })).toBeNull();
  });

  it('reports dirty only when the effective agent pool value differs from the project value', () => {
    expect(hasProjectDetailAgentPoolChanges(null, { useCustomPool: true, agentPoolName: '   ' })).toBeFalse();
    expect(hasProjectDetailAgentPoolChanges('build-pool', { useCustomPool: true, agentPoolName: '  build-pool  ' })).toBeFalse();
    expect(hasProjectDetailAgentPoolChanges('build-pool', { useCustomPool: false, agentPoolName: 'build-pool' })).toBeTrue();
    expect(hasProjectDetailAgentPoolChanges(null, { useCustomPool: true, agentPoolName: 'build-pool' })).toBeTrue();
  });
});