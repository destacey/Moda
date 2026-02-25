import { AnalyticsDataset, Visibility } from '@/src/services/moda-api'

export type Direction = 'Asc' | 'Desc'

export interface BuilderColumn {
  field: string
  alias?: string
}

export interface BuilderFilter {
  field: string
  operator: string
  valuesCsv?: string
}

export interface BuilderMeasure {
  type: string
  field?: string
  alias?: string
  percentile?: number
}

export interface BuilderSort {
  field: string
  direction: Direction
}

export interface BuilderDefinition {
  version: 1
  dataset: AnalyticsDataset
  columns: BuilderColumn[]
  filters: BuilderFilter[]
  groupBy: string[]
  measures: BuilderMeasure[]
  sort: BuilderSort[]
}

export interface FormValues {
  id?: string
  name: string
  description?: string
  dataset: AnalyticsDataset
  visibility: Visibility
  managerIds: string[]
  isActive: boolean
  includeInactive: boolean
  definition: BuilderDefinition
}
