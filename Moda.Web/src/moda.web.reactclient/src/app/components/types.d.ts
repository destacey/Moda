export interface OptionModel<T = string> {
  value: T
  label: string
}

export interface DateRange {
  start?: Date
  end?: Date
}
