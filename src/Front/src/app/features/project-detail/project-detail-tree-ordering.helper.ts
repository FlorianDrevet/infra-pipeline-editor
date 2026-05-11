const hierarchicalEntryCollator = new Intl.Collator('en', {
  numeric: true,
  sensitivity: 'variant',
});

export function sortHierarchicalEntries(entries: readonly string[]): string[] {
  return [...entries].sort((left, right) => hierarchicalEntryCollator.compare(left, right));
}