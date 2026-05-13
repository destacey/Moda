import { ThemeName } from '@/src/components/contexts/theme/types'
import {
  WaydDataGroup,
  WaydDataItem,
  WaydTimelineProps,
  TimelineTemplate,
} from '@/src/components/common/timeline/types'
import { DataItemEnhanced } from 'vis-timeline/standalone'
import {
  BackgroundItemTemplate,
  GroupTemplate,
  RangeItemTemplate,
} from '@/src/components/common/timeline/templates'

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
  slate: {
    item: {
      background: '#c0c0c0',
      foreground: '#17354d',
      font: '#f0f0f0',
    },
    background: {
      background: '#787878',
    },
  },
}

export function getDefaultTemplate<
  TItem extends WaydDataItem,
  TGroup extends WaydDataGroup,
>(
  type: 'group',
  props: WaydTimelineProps<TItem, TGroup>,
): TimelineTemplate<TGroup>

export function getDefaultTemplate<
  TItem extends WaydDataItem,
  TGroup extends WaydDataGroup,
>(
  type: DataItemEnhanced['type'],
  props: WaydTimelineProps<TItem, TGroup>,
): TimelineTemplate<TItem>

export function getDefaultTemplate<
  TItem extends WaydDataItem,
  TGroup extends WaydDataGroup,
>(
  type: DataItemEnhanced['type'] | 'group',
  props: WaydTimelineProps<TItem, TGroup>,
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

// Structural comparison of two group arrays — checks the id set and each
// group's `nestedGroups` membership, which together define the hierarchy
// that vis-timeline tracks internally. Returns true when setGroups() would
// produce no meaningful change. Ordering is irrelevant: vis-timeline uses
// `groupOrder`, not array index, to lay out rows. Label/style-only changes
// should also return true — those are patched in place via DataSet.update().
export function groupsStructurallyEqual<TGroup extends WaydDataGroup>(
  a: TGroup[],
  b: TGroup[],
): boolean {
  if (a.length !== b.length) return false

  const byId = new Map<string | number, TGroup>()
  a.forEach((g) => {
    if (g.id !== undefined) byId.set(g.id, g)
  })

  for (const next of b) {
    if (next.id === undefined) return false
    const prev = byId.get(next.id)
    if (!prev) return false

    const prevNested = prev.nestedGroups ?? []
    const nextNested = next.nestedGroups ?? []
    if (prevNested.length !== nextNested.length) return false
    for (let i = 0; i < prevNested.length; i++) {
      if (prevNested[i] !== nextNested[i]) return false
    }
  }

  return true
}
