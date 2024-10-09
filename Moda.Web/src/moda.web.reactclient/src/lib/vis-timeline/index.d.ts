import { DataItem } from 'vis-timeline/standalone'
import 'vis-timeline/standalone'

declare module 'vis-timeline/standalone' {
  export interface DataItemEnhanced extends Omit<DataItem, 'type'> {
    type: 'box' | 'point' | 'range' | 'background'
  }

  export type TimelineOptionsTemplateFunction<
    TItem = DataItemEnhanced,
    TEl = HTMLElement,
    TData = unknown,
  > = (item?: TItem, element?: TEl, data?: TData) => string | HTMLElement
}
