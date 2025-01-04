import { FC } from 'react'
import {
  DataItemEnhanced,
  TimelineOptionsTemplateFunction,
  DataGroup,
} from 'vis-timeline/standalone'

export type ModaDataItem<T = unknown, G = unknown> = DataItemEnhanced<G> & {
  itemColor?: string
  objectData?: T
}

export type ModaDataGroup<T = unknown> = DataGroup & {
  treeLevel?: number // undocumented property, that is used for styling
  objectData?: T
}

export type ModaTimelineOptions<T> = {
  maxHeight?: number
  minHeight?: number
  showCurrentTime?: boolean
  start: Date
  end: Date
  min: Date
  max: Date
  groupOrder?: string
  template?: TimelineOptionsTemplateFunction<T>
}

export type ModaTimelineProps<
  TItem extends ModaDataItem,
  TGroup extends ModaDataGroup,
> = {
  data: TItem[]
  groups?: TGroup[]
  isLoading: boolean
  options: ModaTimelineOptions<TItem>
  rangeItemTemplate?: TimelineTemplate<TItem>
  groupTemplate?: TimelineTemplate<TGroup>
  emptyMessage?: string
}

export type GroupTemplateProps<T extends ModaDataGroup> = {
  item: T
  fontColor: string
  foregroundColor?: string
  parentElement?: HTMLElement
}

export type ItemTemplateProps<T extends ModaDataItem> = {
  item: T
  fontColor: string
  foregroundColor?: string
}

export type TimelineTemplate<T extends ModaDataItem | ModaDataGroup> =
  T extends ModaDataItem
    ? FC<ItemTemplateProps<T>>
    : T extends ModaDataGroup
      ? FC<GroupTemplateProps<T>>
      : never
