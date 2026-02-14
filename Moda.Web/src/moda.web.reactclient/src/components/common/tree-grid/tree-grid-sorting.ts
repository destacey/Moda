import type { SortingFn } from '@tanstack/react-table'
import dayjs from 'dayjs'

const compareNumbers = (a: number, b: number): number => {
  return a === b ? 0 : a > b ? 1 : -1
}

type DateSortOptions = {
  emptyValue?: number
}

/**
 * Custom sorting factory that sorts by a derived date value.
 * Handles null/undefined values using a configurable emptyValue (default: -Infinity).
 */
export function dateSortBy(
  getDate: (row: any) => string | number | Date | null | undefined,
  options?: DateSortOptions,
): SortingFn<any>

export function dateSortBy(
  getDate: (row: any) => string | number | Date | null | undefined,
  options: DateSortOptions = {},
): SortingFn<any> {
  const emptyValue = options.emptyValue ?? -Infinity

  return (a, b) => {
    const av = getDate(a)
    const bv = getDate(b)

    const aNum = av ? dayjs(av).valueOf() : emptyValue
    const bNum = bv ? dayjs(bv).valueOf() : emptyValue

    return compareNumbers(aNum, bNum)
  }
}
