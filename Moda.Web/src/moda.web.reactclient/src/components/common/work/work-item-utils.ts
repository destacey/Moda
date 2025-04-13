export const workItemKeyComparator = (key1: string, key2: string) => {
  if (!key1) return 1 // sort empty keys to the end
  if (!key2) return -1

  const [str1, num1] = key1.split('-')
  const [str2, num2] = key2.split('-')

  if (str1 < str2) return -1
  if (str1 > str2) return 1

  return parseInt(num1) - parseInt(num2)
}

export const workStatusCategoryComparator = (
  category1: string,
  category2: string,
) => {
  const categories = ['Proposed', 'Active', 'Done', 'Removed']
  return categories.indexOf(category1) - categories.indexOf(category2)
}
