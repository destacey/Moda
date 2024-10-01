import { ThemeName } from '@/src/app/components/contexts/theme/types'
import {
  ModaDataGroup,
  ModaDataItem,
  ModaTimelineProps,
  TimelineTemplate,
} from '@/src/app/components/common/timeline/types'
import { DataItemEnhanced } from 'vis-timeline/standalone'
import {
  BackgroundItemTemplate,
  GroupTemplate,
  RangeItemTemplate,
} from '@/src/app/components/common/timeline/templates'

type TimeLineColorData = {
  item: {
    background: string
    foreground: string
    font: string
  }
  background: {
    background: string
  }
}

export const DefaultTimeLineColors: Record<ThemeName, TimeLineColorData> = {
  light: {
    item: {
      background: '#ecf0f1',
      foreground: '#c7edff',
      font: '#4d4d4d',
    },
    background: {
      background: '#d0d3d4',
    },
  },
  dark: {
    item: {
      background: '#303030',
      foreground: '#17354d',
      font: '#FFFFFF',
    },
    background: {
      background: '#61646e',
    },
  },
}

export function getDefaultTemplate<
  TItem extends ModaDataItem,
  TGroup extends ModaDataGroup,
>(
  type: 'group',
  props: ModaTimelineProps<TItem, TGroup>,
): TimelineTemplate<TGroup>

export function getDefaultTemplate<
  TItem extends ModaDataItem,
  TGroup extends ModaDataGroup,
>(
  type: DataItemEnhanced['type'],
  props: ModaTimelineProps<TItem, TGroup>,
): TimelineTemplate<TItem>

export function getDefaultTemplate<
  TItem extends ModaDataItem,
  TGroup extends ModaDataGroup,
>(
  type: DataItemEnhanced['type'] | 'group',
  props: ModaTimelineProps<TItem, TGroup>,
) {
  switch (type) {
    case 'range':
      return props.rangeItemTemplate ?? RangeItemTemplate
    case 'background':
      return BackgroundItemTemplate
    case 'group':
      return props.groupTemplate ?? GroupTemplate
    case 'box':
    case 'point':
    default:
      return undefined
  }
}
