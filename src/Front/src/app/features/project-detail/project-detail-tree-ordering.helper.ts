export function sortHierarchicalEntries(entries: readonly string[]): string[] {
  return [...entries].sort((left, right) => left.localeCompare(right));
}