export interface OptionModel<T = string> {
  value: T
  label: string
}

export interface DateRange {
  start?: Date
  end?: Date
}

// Iteration States from Moda.Common.Domain.Enums.Work.IterationState
export enum IterationState {
  Unknown = 0,
  Completed = 1,
  Active = 2,
  Future = 3,
}

// Work Type Tiers from Moda.Common.Domain.Enums.Work.WorkTypeTier
export enum WorkTypeTier {
  Portfolio = 0,
  Requirement = 1,
  Task = 2,
  Other = 3,
}

// Work Status Categories from Moda.Common.Domain.Enums.Work.WorkStatusCategory
export enum WorkStatusCategory {
  Proposed = 0,
  Active = 1,
  Done = 2,
  Removed = 3,
}
