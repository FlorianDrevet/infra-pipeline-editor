const resourceTypeCollator = new Intl.Collator(undefined, {
  numeric: true,
  sensitivity: 'base',
});

export function sortResourceTypesAlphabetically(resourceTypes: Iterable<string>): string[] {
  return [...resourceTypes].sort((left, right) => resourceTypeCollator.compare(left, right));
}