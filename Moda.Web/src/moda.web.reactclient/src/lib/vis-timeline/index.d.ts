import { DataItem } from 'vis-timeline/standalone'
import 'vis-timeline/standalone'

declare module 'vis-timeline/standalone' {
  export interface DataItemEnhanced<TGroup = unknown> extends Omit<DataItem, 'type' | 'group'> {
    type: 'box' | 'point' | 'range' | 'background'
    group?: TGroup
  }

  export type TimelineOptionsTemplateFunction<
    TItem = DataItemEnhanced,
    TEl = HTMLElement,
    TData = unknown,
  > = (item?: TItem, element?: TEl, data?: TData) => string | HTMLElement
}
