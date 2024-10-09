'use client'

import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import {
  DataGroup,
  DataItemEnhanced,
  DataSet,
  Timeline,
  TimelineOptions,
  TimelineOptionsTemplateFunction,
} from 'vis-timeline/standalone'
import './moda-timeline.css'
import { Spin, Typography } from 'antd'
import useTheme from '../../contexts/theme'
import { createRoot } from 'react-dom/client'
import {
  GroupTemplateProps,
  ModaDataGroup,
  ModaDataItem,
  ModaTimelineOptions,
} from '.'
import { ModaEmpty } from '..'
import { getLuminance } from '@/src/utils/color-helper'
import { ItemTemplate } from '@/src/app/components/common/timeline/types'
import { ThemeName } from '@/src/app/components/contexts/theme/types'

const { Text } = Typography

export type ModaTimelineProps<T> = {
  data: ModaDataItem<T>[]
  groups?: ModaDataGroup<T>[]
  isLoading: boolean
  options: ModaTimelineOptions<T>
  rangeItemTemplate?: ItemTemplate<T>
  groupTemplate?: ItemTemplate<T>
  emptyMessage?: string
}

const RangeItemTemplate: ItemTemplate<unknown> = (props) => {
  // TODO: The 0.6 needs to be tested with some of the other colors
  const fontColor =
    getLuminance((props.item as ModaDataItem).itemColor ?? props.foregroundColor ?? '') > 0.6 ? '#4d4d4d' : '#FFFFFF'

  return (
    <Text style={{ padding: '5px', color: fontColor }}>
      {props.item.content}
    </Text>
  )
}

const GroupTemplate: ItemTemplate<unknown> = (props) => {
  return (
    <Text style={{ padding: '5px', color: props.fontColor }}>
      {props.item.content}
    </Text>
  )
}

//   return <Text style={{ padding: '5px', color: fontColor }}>{props.item.content}</Text>
// }

const BackgroundItemTemplate: ItemTemplate<unknown> = (props) => {
  return (
    <div>
      <Text>{props.item.content}</Text>
    </div>
  )
}

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

const DefaultTimeLineColors: Record<ThemeName, TimeLineColorData> = {
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

const getDefaultTemplate = <T,>(
  type: DataItemEnhanced['type'] | 'group',
  props: ModaTimelineProps<T>,
): ItemTemplate<T> | ItemTemplate<unknown> | undefined => {
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

const ModaTimeline = <T,>(props: ModaTimelineProps<T>) => {
  const [isTimelineLoading, setIsTimelineLoading] = useState(false)

  const elementMapRef = useRef<Record<string | number, HTMLElement>>({})
  const timelineRef = useRef<HTMLDivElement>(null)

  const { currentThemeName } = useTheme()

  const colors = useMemo(
    () => DefaultTimeLineColors[currentThemeName],
    [currentThemeName],
  )

  const itemTemplateManager = useCallback<
    TimelineOptionsTemplateFunction<ModaDataItem<T>>
  >(
    (item, element, data) => {
      if (!item) return ''

      const mapId = item.id ?? 0
      if (elementMapRef.current?.[mapId]) return elementMapRef.current[mapId]

      // Create a container for the react element (prevents DOM node errors)
      const container = document.createElement('div')

      if (element) element.appendChild(container)

      const root = createRoot(container)

      const Template = getDefaultTemplate<T>(item.type, props)

      if (Template)
        root.render(
          <Template
            item={item}
            fontColor={colors.item.font}
            foregroundColor={colors.item.foreground}
          />,
        )

      // Store the rendered element container to reference later
      elementMapRef.current[mapId] = container

      // Return the new container
      return container
    },
    [colors.item.font, colors.item.foreground, props],
  )

  const groupTemplateManager = useCallback<
    TimelineOptionsTemplateFunction<ModaDataGroup<T>>
  >(
    (item, element, data) => {
      if (!item) return ''

      const mapId = item.id ?? 0
      if (elementMapRef.current?.[mapId]) return elementMapRef.current[mapId]

      // Create a container for the react element (prevents DOM node errors)
      const container = document.createElement('div')
      element?.appendChild(container)

      const root = createRoot(container)

      const Template = getDefaultTemplate<T>('group', props)

      if (Template)
        root.render(
          <Template
            item={item}
            fontColor={colors.item.font}
            foregroundColor={colors.item.foreground}
          />,
        )
      
      // Store the rendered element container to reference later
      elementMapRef.current[mapId] = container

      // Return the new container
      return container
    },
    [colors.item.font, colors.item.foreground, props],
  )

  const options = useMemo((): TimelineOptions => {
    return {
      editable: false,
      selectable: true,
      orientation: 'top',
      maxHeight: props.options.maxHeight ?? 650,
      minHeight: 200,
      moveable: true,
      showCurrentTime: props.options.showCurrentTime ?? true,
      verticalScroll: true,
      zoomKey: 'ctrlKey',
      start: props.options.start,
      end: props.options.end,
      min: props.options.min,
      max: props.options.max,
      groupOrder: props.options.groupOrder ?? 'order',
      xss: { disabled: false },
      template: props.options.template ?? itemTemplateManager,
      groupTemplate: groupTemplateManager,
    }
  }, [
    groupTemplateManager,
    itemTemplateManager,
    props.options.end,
    props.options.groupOrder,
    props.options.max,
    props.options.maxHeight,
    props.options.min,
    props.options.showCurrentTime,
    props.options.start,
    props.options.template,
  ])

  useEffect(() => {
    if (
      props.isLoading ||
      (props.data.length === 0 && (!props.groups || props.groups?.length === 0))
    )
      return

    setIsTimelineLoading(true)

    const datasetItems = new DataSet([])
    props.data.map((item) => {
      const backgroundColor = item.itemColor ?? colors.item.background
      const newItem: ModaDataItem<T> = {
        ...item,
        itemColor: backgroundColor,
        style: item.style
          ? item.style
          : item.type === 'range'
            ? `background: ${backgroundColor}; border-color: ${backgroundColor};`
            : item.type === 'background'
              ? `background: ${colors.background.background}; border-style: inset; border-width: 1px;`
              : undefined,
      }

      // TODO: Looks like visjs DataSet generic type has no overlap with visJs DataItem.
      //  Further investigation needed in order to fix this type error
      datasetItems.add(newItem)
    })

    let timeline: Timeline

    if (timelineRef.current) {
      if (props.groups?.length && props.groups?.length > 0) {
        timeline = new Timeline(
          timelineRef.current,
          datasetItems,
          new DataSet(props.groups),
          options,
        )
      } else {
        timeline = new Timeline(timelineRef.current, datasetItems, options)
      }
    }

    setTimeout(() => {
      // it takes about 1 second to render the timeline
      setIsTimelineLoading(false)
    }, 1000)

    // cleanup function to remove the timeline when the component is unmounted
    return () => {
      elementMapRef.current = {}
      if (timeline) {
        timeline.destroy()
      }
    }
  }, [
    colors.background.background,
    colors.item.background,
    options,
    props.data,
    props.groups,
    props.isLoading,
  ])

  const isLoading = props.isLoading || isTimelineLoading

  return (
    <Spin spinning={isLoading} tip="Loading timeline..." size="large">
      <div ref={timelineRef} />
      {!isLoading &&
        (!props.data || props.data.length === 0) &&
        (!props.groups || props.groups.length === 0) && (
          <ModaEmpty message={props.emptyMessage ?? 'No timeline data'} />
        )}
    </Spin>
  )
}

export default ModaTimeline
