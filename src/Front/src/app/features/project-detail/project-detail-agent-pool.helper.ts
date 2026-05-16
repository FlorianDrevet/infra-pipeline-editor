export interface ProjectDetailAgentPoolDraft {
  readonly useCustomPool: boolean;
  readonly agentPoolName: string | null;
}

export function normalizeProjectDetailAgentPoolName(agentPoolName: string | null | undefined): string | null {
  const normalizedAgentPoolName = agentPoolName?.trim();
  return normalizedAgentPoolName || null;
}

export function resolveProjectDetailAgentPoolDraft(
  agentPoolName: string | null | undefined,
): ProjectDetailAgentPoolDraft {
  const normalizedAgentPoolName = normalizeProjectDetailAgentPoolName(agentPoolName);

  return {
    useCustomPool: normalizedAgentPoolName !== null,
    agentPoolName: normalizedAgentPoolName,
  };
}

export function resolveProjectDetailAgentPoolValue(
  draft: ProjectDetailAgentPoolDraft,
): string | null {
  if (!draft.useCustomPool) {
    return null;
  }

  return normalizeProjectDetailAgentPoolName(draft.agentPoolName);
}

export function toggleProjectDetailAgentPoolDraft(
  draft: ProjectDetailAgentPoolDraft,
  useCustomPool: boolean,
): ProjectDetailAgentPoolDraft {
  return {
    useCustomPool,
    agentPoolName: draft.agentPoolName,
  };
}

export function hasProjectDetailAgentPoolChanges(
  projectAgentPoolName: string | null | undefined,
  draft: ProjectDetailAgentPoolDraft,
): boolean {
  return normalizeProjectDetailAgentPoolName(projectAgentPoolName) !== resolveProjectDetailAgentPoolValue(draft);
}