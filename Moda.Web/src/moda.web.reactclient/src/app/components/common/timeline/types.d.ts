import { FC } from 'react'
import { DataItemEnhanced, TimelineOptionsTemplateFunction, DataGroup } from 'vis-timeline/standalone'

export type ModaDataItem<T = unknown> = DataItemEnhanced & {
  itemColor?: string
  objectData?: T
}

export type ModaDataGroup<T = unknown> = DataGroup & {
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
  template?: TimelineOptionsTemplateFunction<ModaDataItem<T>>
}

export interface ItemTemplateProps<T> {
  // TODO: Have some sort of discriminator or conditional in this type so that we may know for certain if its is item or group
  item: ModaDataItem<T> | ModaDataGroup<T>
  fontColor: string
  foregroundColor?: string | undefined
}

export type ItemTemplate<T> = FC<ItemTemplateProps<T>>
