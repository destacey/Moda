import { dateSortBy } from './project-tasks-table.sorting'

describe('project-tasks-table.sorting', () => {
  describe('dateSortBy', () => {
    type TestRow = { original: { value?: string | number | Date | null } }

    const row = (value?: string | number | Date | null): TestRow => ({
      original: { value },
    })

    it('returns 0 for equal dates', () => {
      const sort = dateSortBy((r: any) => r.original.value)

      expect(sort(row('2024-01-01'), row('2024-01-01'), 'x')).toBe(0)
    })

    it('sorts earlier dates before later dates', () => {
      const sort = dateSortBy((r: any) => r.original.value)

      expect(sort(row('2024-01-01'), row('2024-01-02'), 'x')).toBeLessThan(0)
      expect(sort(row('2024-01-02'), row('2024-01-01'), 'x')).toBeGreaterThan(0)
    })

    it('supports Date and numeric timestamps', () => {
      const sort = dateSortBy((r: any) => r.original.value)

      const jan1 = new Date('2024-01-01T00:00:00Z')
      const jan2 = new Date('2024-01-02T00:00:00Z')
      const jan1Ms = jan1.getTime()

      expect(sort(row(jan1), row(jan2), 'x')).toBeLessThan(0)
      expect(sort(row(jan1Ms), row(jan2), 'x')).toBeLessThan(0)
      expect(sort(row(jan1Ms), row(jan1), 'x')).toBe(0)
    })

    it('treats null/undefined as emptyValue (default: -Infinity), sorting empties first', () => {
      const sort = dateSortBy((r: any) => r.original.value)

      expect(sort(row(null), row('2024-01-01'), 'x')).toBeLessThan(0)
      expect(sort(row(undefined), row('2024-01-01'), 'x')).toBeLessThan(0)
      expect(sort(row(null), row(undefined), 'x')).toBe(0)
    })

    it('respects emptyValue override', () => {
      const sort = dateSortBy((r: any) => r.original.value, {
        emptyValue: Infinity,
      })

      expect(sort(row(null), row('2024-01-01'), 'x')).toBeGreaterThan(0)
      expect(sort(row(undefined), row('2024-01-01'), 'x')).toBeGreaterThan(0)
      expect(sort(row(null), row(undefined), 'x')).toBe(0)
    })
  })
})
