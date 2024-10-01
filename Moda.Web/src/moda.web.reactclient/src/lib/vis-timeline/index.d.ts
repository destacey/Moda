import 'vis-timeline/standalone'
import { DataItem } from 'vis-timeline/standalone'

declare module 'vis-timeline/standalone' {
  type TimelineOptionsTemplateFunction<TItem = DataItem, TEl = HTMLElement, TData = unknown> = (
    item?: TItem,
    element?: TEl,
    data?: TData
  ) => string | HTMLElement
}
