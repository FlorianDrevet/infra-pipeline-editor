import { shouldDeferMonoRepoBatchReveal } from './project-generation-visibility.helper';

describe('project generation visibility helper', () => {
  it('defers reveal while a Generate All batch is active and at least one generation is still loading', () => {
    const shouldDeferReveal = shouldDeferMonoRepoBatchReveal({
      isGenerateAllBatchActive: true,
      isAnyGenerationLoading: true,
    });

    expect(shouldDeferReveal).toBeTrue();
  });

  it('does not defer reveal for standalone generations outside of Generate All', () => {
    const shouldDeferReveal = shouldDeferMonoRepoBatchReveal({
      isGenerateAllBatchActive: false,
      isAnyGenerationLoading: true,
    });

    expect(shouldDeferReveal).toBeFalse();
  });

  it('reveals outcomes once the Generate All batch has fully settled', () => {
    const shouldDeferReveal = shouldDeferMonoRepoBatchReveal({
      isGenerateAllBatchActive: true,
      isAnyGenerationLoading: false,
    });

    expect(shouldDeferReveal).toBeFalse();
  });
});