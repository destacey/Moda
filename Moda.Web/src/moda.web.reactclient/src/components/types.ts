export interface OptionModel<T = string> {
  value: T
  label: string
}

export interface DateRange {
  start?: Date
  end?: Date
}

// Work Type Tiers from Moda.Common.Domain.Enums.Work.WorkTypeTier
export enum WorkTypeTier {
  Portfolio = 0,
  Requirement = 1,
  Task = 2,
  Other = 3,
}
