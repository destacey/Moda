export interface ModaTimelineOptions {
  maxHeight?: number | undefined
  minHeight?: number | undefined
  showCurrentTime?: boolean | undefined
  start: Date
  end: Date
  min: Date
  max: Date
  template?: TimelineOptionsTemplateFunction | undefined
}
