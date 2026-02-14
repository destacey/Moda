import { WorkItemListDto } from '@/src/services/moda-api'

export type CycleTimeOutlierMethod = 'Balanced' | 'Forecasting'

export const applyBalancedPercentileFilter = (
  sortedItems: WorkItemListDto[],
  percentile: number,
): WorkItemListDto[] => {
  const n = sortedItems.length
  const trimPercent = (1 - percentile) / 2
  const lowerIndex = Math.floor(n * trimPercent)
  const upperIndex = Math.ceil(n * (1 - trimPercent))
  return sortedItems.slice(lowerIndex, upperIndex)
}

export const applyForecastingPercentileFilter = (
  sortedItems: WorkItemListDto[],
  percentile: number,
): WorkItemListDto[] => {
  const n = sortedItems.length
  const cutoffIndex = Math.ceil(n * percentile)
  return sortedItems.slice(0, cutoffIndex)
}

export const filterCycleTimeWorkItems = (
  workItemsData: WorkItemListDto[] | undefined,
  percentile: number,
  method: CycleTimeOutlierMethod,
): WorkItemListDto[] => {
  const cycleTimeItems = workItemsData?.filter((item) => item.cycleTime != null) ?? []
  if (cycleTimeItems.length === 0) {
    return []
  }

  const normalizedPercentile = Math.max(0, Math.min(100, percentile)) / 100
  if (normalizedPercentile === 1) {
    return cycleTimeItems
  }
  if (normalizedPercentile === 0) {
    return []
  }

  const sorted = [...cycleTimeItems].sort(
    (a, b) => (a.cycleTime || 0) - (b.cycleTime || 0),
  )

  if (method === 'Balanced') {
    return applyBalancedPercentileFilter(sorted, normalizedPercentile)
  }

  return applyForecastingPercentileFilter(sorted, normalizedPercentile)
}
