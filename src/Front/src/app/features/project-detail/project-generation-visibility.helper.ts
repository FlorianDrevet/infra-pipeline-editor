interface MonoRepoBatchRevealState {
  readonly isGenerateAllBatchActive: boolean;
  readonly isAnyGenerationLoading: boolean;
}

export function shouldDeferMonoRepoBatchReveal(state: MonoRepoBatchRevealState): boolean {
  return state.isGenerateAllBatchActive && state.isAnyGenerationLoading;
}