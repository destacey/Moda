import { AnalyticsDataset, Visibility } from '@/src/services/moda-api'
import {
  BuilderColumn,
  BuilderDefinition,
  BuilderMeasure,
  BuilderSort,
  FormValues,
} from './types'

export const fieldOptions = [
  { label: 'Id', value: 'id' },
  { label: 'Key', value: 'key' },
  { label: 'Title', value: 'title' },
  { label: 'Workspace Key', value: 'workspace.key' },
  { label: 'Workspace Name', value: 'workspace.name' },
  { label: 'Type Name', value: 'type.name' },
  { label: 'Status Name', value: 'status.name' },
  { label: 'Status Category', value: 'statusCategory' },
  { label: 'Created Date', value: 'createdDate' },
  { label: 'Changed Date', value: 'changedDate' },
  { label: 'Done Date', value: 'doneDate' },
  { label: 'Priority', value: 'priority' },
  { label: 'Stack Rank', value: 'stackRank' },
  { label: 'Story Points', value: 'storyPoints' },
]

export const groupByOptions = [
  ...fieldOptions,
  { label: 'Created Date (Day)', value: 'createdDate:Day' },
  { label: 'Created Date (Week)', value: 'createdDate:Week' },
  { label: 'Created Date (Month)', value: 'createdDate:Month' },
  { label: 'Changed Date (Day)', value: 'changedDate:Day' },
  { label: 'Changed Date (Week)', value: 'changedDate:Week' },
  { label: 'Changed Date (Month)', value: 'changedDate:Month' },
  { label: 'Done Date (Day)', value: 'doneDate:Day' },
  { label: 'Done Date (Week)', value: 'doneDate:Week' },
  { label: 'Done Date (Month)', value: 'doneDate:Month' },
]

export const filterOperatorOptions = [
  { label: 'Equals', value: 'Equals' },
  { label: 'Not Equals', value: 'NotEquals' },
  { label: 'In', value: 'In' },
  { label: 'Not In', value: 'NotIn' },
  { label: 'Contains', value: 'Contains' },
  { label: 'Starts With', value: 'StartsWith' },
  { label: 'Ends With', value: 'EndsWith' },
  { label: 'Greater Than', value: 'GreaterThan' },
  { label: 'Greater Or Equal', value: 'GreaterThanOrEqual' },
  { label: 'Less Than', value: 'LessThan' },
  { label: 'Less Or Equal', value: 'LessThanOrEqual' },
  { label: 'Between', value: 'Between' },
  { label: 'Is Null', value: 'IsNull' },
  { label: 'Is Not Null', value: 'IsNotNull' },
]

export const measureTypeOptions = [
  { label: 'Count', value: 'Count' },
  { label: 'Sum', value: 'Sum' },
  { label: 'Average', value: 'Avg' },
  { label: 'Median', value: 'Median' },
  { label: 'Percentile', value: 'Percentile' },
]

export const numericFieldOptions = [
  { label: 'Priority', value: 'priority' },
  { label: 'Stack Rank', value: 'stackRank' },
  { label: 'Story Points', value: 'storyPoints' },
]

export const makeDefaultDefinition = (): BuilderDefinition => ({
  version: 1,
  dataset: AnalyticsDataset.WorkItems,
  columns: [
    { field: 'key', alias: 'Key' },
    { field: 'title', alias: 'Title' },
    { field: 'statusCategory', alias: 'Status' },
  ],
  filters: [],
  groupBy: [],
  measures: [],
  sort: [{ field: 'key', direction: 'Asc' }],
})

export const makeDefaultFormValues = (): FormValues => ({
  name: '',
  description: '',
  dataset: AnalyticsDataset.WorkItems,
  visibility: Visibility.Private,
  managerIds: [],
  isActive: true,
  includeInactive: false,
  definition: makeDefaultDefinition(),
})

export const parseDefinition = (
  definitionJson: string,
  dataset: AnalyticsDataset,
): BuilderDefinition => {
  try {
    const parsed = JSON.parse(definitionJson) as {
      version?: number
      dataset?: AnalyticsDataset
      columns?: BuilderColumn[]
      filters?: Array<{
        field: string
        operator: string
        values?: unknown[]
      }>
      groupBy?: string[]
      measures?: BuilderMeasure[]
      sort?: BuilderSort[]
    }
    return {
      version: 1,
      dataset,
      columns: parsed.columns ?? [],
      filters:
        parsed.filters?.map((f) => ({
          field: f.field,
          operator: f.operator,
          valuesCsv: (f.values ?? []).map(String).join(', '),
        })) ?? [],
      groupBy: parsed.groupBy ?? [],
      measures: parsed.measures ?? [],
      sort: parsed.sort ?? [],
    }
  } catch {
    return makeDefaultDefinition()
  }
}

const parseCsvValues = (valuesCsv?: string): unknown[] => {
  if (!valuesCsv?.trim()) return []
  return valuesCsv
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean)
    .map((item) => {
      const lower = item.toLowerCase()
      if (lower === 'true') return true
      if (lower === 'false') return false
      if (!Number.isNaN(Number(item)) && item !== '') return Number(item)
      return item
    })
}

export const buildDefinitionJson = (
  definition: BuilderDefinition,
  dataset: AnalyticsDataset,
): string => {
  const payload = {
    version: 1,
    dataset,
    columns: definition.columns ?? [],
    filters: (definition.filters ?? []).map((f) => ({
      field: f.field,
      operator: f.operator,
      values: parseCsvValues(f.valuesCsv),
    })),
    groupBy: definition.groupBy ?? [],
    measures: definition.measures ?? [],
    sort: definition.sort ?? [],
  }

  return JSON.stringify(payload, null, 2)
}
