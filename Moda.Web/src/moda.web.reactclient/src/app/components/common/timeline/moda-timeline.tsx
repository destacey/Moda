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
import { ModaDataItem, ModaTimelineOptions, RangeItemTemplateProps } from '.'
import { ModaEmpty } from '..'

const { Text } = Typography

export type ModaTimelineProps = {
  data: ModaDataItem[]
  groups?: DataGroup[]
  isLoading: boolean
  options: ModaTimelineOptions
  rangeItemTemplate?: (RangeItemTemplateProps) => JSX.Element
  emptyMessage?: string
}

const RangeItemTemplate = (props: RangeItemTemplateProps) => {
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
      createRoot(container).render(
        <Text style={{ padding: '5px', color: itemFontColor }}>
          {item.content}
        </Text>,
      )

      // Store the rendered element container to reference later
      elementMapRef.current[mapId] = container

      // Return the new container
      return container
    },
    [itemFontColor],
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
      const newItem = {
        style: item.style
          ? item.style
          : item.type == 'range'
            ? `background: ${itemBackgroundColor}; border-color: ${itemBackgroundColor};`
            : item.type == 'background'
              ? `background: ${backgroundBackgroundColor}; border-style: inset; border-width: 1px;`
              : '',
        ...item,
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

  return (
    <Spin
      spinning={props.isLoading || isTimelineLoading}
      tip="Loading timeline..."
      size="large"
    >
      <div ref={timelineRef} />
      {(!props.data || props.data.length === 0) &&
        (!props.groups || props.groups.length === 0) && (
          <ModaEmpty message={props.emptyMessage ?? 'No timeline data'} />
        )}
    </Spin>
  )
}

export default ModaTimeline
