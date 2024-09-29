import { DataItem } from 'vis-timeline/standalone'

export type ModaDataItem<T = any> = DataItem & {
  itemColor?: string | undefined
  objectData?: T
}

export interface ModaTimelineOptions {
  maxHeight?: number | undefined
  minHeight?: number | undefined
  showCurrentTime?: boolean | undefined
  start: Date
  end: Date
  min: Date
  max: Date
  groupOrder?: string | undefined
  template?: TimelineOptionsTemplateFunction | undefined
}

export interface RangeItemTemplateProps<T = any> {
  item: ModaDataItem<T>
  fontColor: string
  foregroundColor?: string | undefined
}
