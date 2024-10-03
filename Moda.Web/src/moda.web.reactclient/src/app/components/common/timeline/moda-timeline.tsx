'use client'

import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import {
  DataGroup,
  DataItem,
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
  RangeItemTemplateProps,
} from '.'
import { ModaEmpty } from '..'
import { getLuminance } from '@/src/utils/color-helper'

const { Text } = Typography

export type ModaTimelineProps<TI = any, TG = any> = {
  data: ModaDataItem<TI>[]
  groups?: ModaDataGroup<TG>[]
  isLoading: boolean
  options: ModaTimelineOptions
  rangeItemTemplate?: (props: RangeItemTemplateProps<TI>) => JSX.Element
  groupTemplate?: (props: GroupTemplateProps<TG>) => JSX.Element
  emptyMessage?: string
}

const RangeItemTemplate = (props: RangeItemTemplateProps<ModaDataItem>) => {
  // TODO: the 0.6 needs to be tested with some of the other colors
  const fontColor =
    getLuminance(props.item.itemColor) > 0.6 ? '#4d4d4d' : '#FFFFFF'

  return (
    <Text style={{ padding: '5px', color: fontColor }}>
      {props.item.content}
    </Text>
  )
}

const GroupTemplate = (props: GroupTemplateProps<ModaDataGroup>) => {
  return (
    <Text style={{ padding: '5px', color: props.fontColor }}>
      {props.item.content}
    </Text>
  )
}

interface BackgroundItemTemplateProps {
  item: ModaDataItem
  fontColor: string
}

const BackgroundItemTemplate = (props: BackgroundItemTemplateProps) => {
  return (
    <div>
      <Text>{props.item.content}</Text>
    </div>
  )
}

const ModaTimeline = (props: ModaTimelineProps) => {
  const [isTimelineLoading, setIsTimelineLoading] = useState(false)

  const elementMapRef = useRef({})
  const timelineRef = useRef<HTMLDivElement>(null)

  const { currentThemeName } = useTheme()
  const itemBackgroundColor =
    currentThemeName === 'light' ? '#ecf0f1' : '#303030'
  const itemForegroundColor =
    currentThemeName === 'light' ? '#c7edff' : '#17354d'
  const itemFontColor = currentThemeName === 'light' ? '#4d4d4d' : '#FFFFFF'
  const backgroundBackgroundColor =
    currentThemeName === 'light' ? '#d0d3d4' : '#61646e'

  const itemTemplateManager: TimelineOptionsTemplateFunction = useCallback(
    (item: DataItem, element: HTMLElement, data: any) => {
      if (!item) return

      const mapId = item?.id
      if (elementMapRef.current?.[mapId]) return elementMapRef.current[mapId]

      // Create a container for the react element (prevents DOM node errors)
      const container = document.createElement('div')
      element.appendChild(container)

      const root = createRoot(container)

      if (item.type === 'range') {
        if (props.rangeItemTemplate) {
          root.render(
            props.rangeItemTemplate({
              item: item,
              fontColor: itemFontColor,
              foregroundColor: itemForegroundColor,
            }),
          )
        } else {
          root.render(
            <RangeItemTemplate item={item} fontColor={itemFontColor} />,
          )
        }
      } else if (item.type === 'background') {
        root.render(
          <BackgroundItemTemplate item={item} fontColor={itemFontColor} />,
        )
      }

      // Store the rendered element container to reference later
      elementMapRef.current[mapId] = container

      // Return the new container
      return container
    },
    [itemFontColor, itemForegroundColor, props],
  )

  const groupTemplateManager: TimelineOptionsTemplateFunction = useCallback(
    (item: DataItem, element: HTMLElement, data: any) => {
      if (!item) return

      const mapId = item?.id
      if (elementMapRef.current?.[mapId]) return elementMapRef.current[mapId]

      // Create a container for the react element (prevents DOM node errors)
      const container = document.createElement('div')
      element.appendChild(container)

      const root = createRoot(container)

      if (props.groupTemplate) {
        root.render(
          props.groupTemplate({
            item: item,
            fontColor: itemFontColor,
          }),
        )
      } else {
        root.render(<GroupTemplate item={item} fontColor={itemFontColor} />)
      }

      // Store the rendered element container to reference later
      elementMapRef.current[mapId] = container

      // Return the new container
      return container
    },
    [itemFontColor, props],
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
      const backgroundColor = item.itemColor ?? itemBackgroundColor
      const newItem = {
        ...item,
        itemColor: backgroundColor,
        style: item.style
          ? item.style
          : item.type === 'range'
            ? `background: ${backgroundColor}; border-color: ${backgroundColor};`
            : item.type === 'background'
              ? `background: ${backgroundBackgroundColor}; border-style: inset; border-width: 1px;`
              : undefined,
      }
      datasetItems.add(newItem)
    })

    let timeline: Timeline

    if (timelineRef.current) {
      if (props.groups?.length > 0) {
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
    backgroundBackgroundColor,
    itemBackgroundColor,
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
