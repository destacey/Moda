/**
 * Sorts an array of objects by the 'name' property and returns a comma-separated string of the sorted names.
 *
 * @param {Array<{ name: string }>} items - The array of objects to be sorted.
 * @returns {string} A comma-separated string of sorted names.
 *
 * @example
 * const items = [{ name: 'Charlie' }, { name: 'Alice' }, { name: 'Bob' }];
 * const result = getSortedNames(items);
 * console.log(result); // Output: 'Alice, Bob, Charlie'
 */
export function getSortedNames(items: { name: string }[]): string {
  return items
    .slice()
    .sort((a, b) => a.name.localeCompare(b.name))
    .map((m) => m.name)
    .join(', ')
}
