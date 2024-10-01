import { DataItem, TimelineOptionsTemplateFunction } from 'vis-timeline/standalone'

export type ModaDataItem<T = unknown> = DataItem & {
  itemColor?: string | undefined
  objectData?: T
}

export type ModaDataGroup<T = any> = DataGroup & {
  objectData?: T
}

export type ModaTimelineOptions<T = unknown, TData extends ModaDataItem<unknown> = ModaDataItem<T>> = {
  maxHeight?: number | undefined
  minHeight?: number | undefined
  showCurrentTime?: boolean | undefined
  start: Date
  end: Date
  min: Date
  max: Date
  groupOrder?: string | undefined
  template?: TimelineOptionsTemplateFunction<TData>
}

export interface ItemTemplateProps<TData extends ModaDataItem<unknown> = ModaDataItem<unknown>> {
  item: TData
  fontColor: string
  foregroundColor?: string | undefined
}

export type ItemTemplate<T = unknown, TData extends ModaDataItem<unknown> = ModaDataItem<T>> = (props: ItemTemplateProps<TData>) => React.ReactNode;

export interface GroupTemplateProps<T = any> {
  item: ModaDataGroup<T>
  fontColor: string
}
