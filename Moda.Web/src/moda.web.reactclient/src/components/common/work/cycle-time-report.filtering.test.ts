import {
  applyBalancedPercentileFilter,
  applyForecastingPercentileFilter,
  filterCycleTimeWorkItems,
} from './cycle-time-report.filtering'
import { WorkItemListDto } from '@/src/services/moda-api'

const createMockWorkItem = (
  key: string,
  cycleTime: number | null | undefined,
): Partial<WorkItemListDto> => ({
  key,
  cycleTime,
  workspace: { key: 'ws-1' } as any,
})

describe('cycle-time-report.filtering', () => {
  const sortedItems = [
    createMockWorkItem('WI-1', 1),
    createMockWorkItem('WI-2', 2),
    createMockWorkItem('WI-3', 3),
    createMockWorkItem('WI-4', 4),
    createMockWorkItem('WI-5', 100),
  ] as WorkItemListDto[]

  describe('applyBalancedPercentileFilter', () => {
    it('should trim both ends based on percentile', () => {
      const result = applyBalancedPercentileFilter(sortedItems, 0.6)
      expect(result.map((x) => x.key)).toEqual(['WI-2', 'WI-3', 'WI-4'])
    })
  })

  describe('applyForecastingPercentileFilter', () => {
    it('should keep the fastest work items based on percentile', () => {
      const result = applyForecastingPercentileFilter(sortedItems, 0.6)
      expect(result.map((x) => x.key)).toEqual(['WI-1', 'WI-2', 'WI-3'])
    })
  })

  describe('filterCycleTimeWorkItems', () => {
    it('should return all cycle-time items when percentile is 100', () => {
      const data = [
        createMockWorkItem('WI-1', 5),
        createMockWorkItem('WI-2', null),
        createMockWorkItem('WI-3', 2),
      ] as WorkItemListDto[]

      const result = filterCycleTimeWorkItems(data, 100, 'Balanced')
      expect(result.map((x) => x.key)).toEqual(['WI-1', 'WI-3'])
    })

    it('should return empty when percentile is 0', () => {
      const result = filterCycleTimeWorkItems(sortedItems, 0, 'Balanced')
      expect(result).toEqual([])
    })

    it('should apply Balanced formula', () => {
      const result = filterCycleTimeWorkItems(sortedItems, 60, 'Balanced')
      expect(result.map((x) => x.key)).toEqual(['WI-2', 'WI-3', 'WI-4'])
    })

    it('should apply Forecasting formula', () => {
      const result = filterCycleTimeWorkItems(sortedItems, 60, 'Forecasting')
      expect(result.map((x) => x.key)).toEqual(['WI-1', 'WI-2', 'WI-3'])
    })
  })
})
