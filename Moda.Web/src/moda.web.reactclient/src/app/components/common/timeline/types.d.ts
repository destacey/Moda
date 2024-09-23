export type ModaDataItem<TObjectData = DataItem> =
  | DataItem
  | (DataItem & {
      objectData?: TObjectData
    })

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

export interface RangeItemTemplateProps {
  item: ModaDataItem
  fontColor: string
  foregroundColor?: string | undefined
}
