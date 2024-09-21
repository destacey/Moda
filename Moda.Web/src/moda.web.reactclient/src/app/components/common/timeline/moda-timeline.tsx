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
import { ModaTimelineOptions } from '.'
import { ModaEmpty } from '..'

const { Text } = Typography

export interface ModaTimelineProps {
  data: DataItem[]
  groups?: DataGroup[]
  isLoading: boolean
  options: ModaTimelineOptions
}

interface RangeItemTemplateProps {
  item: DataItem
  fontColor: string
}

const RangeItemTemplate = (props: RangeItemTemplateProps) => {
  return (
    <Text style={{ padding: '5px', color: props.fontColor }}>
      {props.item.content}
    </Text>
  )
}

interface BackgroundItemTemplateProps {
  item: DataItem
}

const BackgroundItemTemplate = (props: BackgroundItemTemplateProps) => {
  return <Text>{props.item.content}</Text>
}

const ModaTimeline = (props: ModaTimelineProps) => {
  const [isTimelineLoading, setIsTimelineLoading] = useState(false)
  const [itemBackgroundColor, setItemBackgroundColor] = useState('')
  const [itemForegroundColor, setItemForegroundColor] = useState('')
  const [itemFontColor, setItemFontColor] = useState('')

  const timelineRef = useRef<HTMLDivElement>(null)

  const { currentThemeName } = useTheme()
  useEffect(() => {
    const isLightTheme = currentThemeName === 'light'
    setItemBackgroundColor(isLightTheme ? '#ecf0f1' : '#303030')
    setItemForegroundColor(isLightTheme ? '#c7edff' : '#17354d')
    setItemFontColor(isLightTheme ? '#4d4d4d' : '#FFFFFF')
  }, [currentThemeName])

  const itemTemplateManager: TimelineOptionsTemplateFunction = useCallback(
    (item: DataItem, element: HTMLElement, data: any) => {
      if (!item || !element) {
        return null
      }

      const root = createRoot(element)

      if (item.type === 'range') {
        root.render(<RangeItemTemplate item={item} fontColor={itemFontColor} />)
      } else if (item.type === 'background') {
        root.render(<BackgroundItemTemplate item={item} />)
      }

      return
    },
    [itemFontColor],
  )

  const groupTemplateManager: TimelineOptionsTemplateFunction = useCallback(
    (item: DataItem, element: HTMLElement, data: any) => {
      if (!item || !element) {
        return null
      }

      createRoot(element).render(
        <Text style={{ padding: '5px', color: itemFontColor }}>
          {item.content}
        </Text>,
      )

      return
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
      groupOrder: 'order',
      xss: { disabled: false },
      template: props.options.template ?? itemTemplateManager,
      groupTemplate: groupTemplateManager,
    }
  }, [
    groupTemplateManager,
    itemTemplateManager,
    props.options.end,
    props.options.max,
    props.options.maxHeight,
    props.options.min,
    props.options.showCurrentTime,
    props.options.start,
    props.options.template,
  ])

  useEffect(() => {
    if (
      props.data.length === 0 &&
      (!props.groups || props.groups?.length === 0)
    )
      return

    setIsTimelineLoading(true)

    const datasetItems = new DataSet([])
    props.data.map((item) => {
      const newItem = {
        style: item.style
          ? item.style
          : `background: ${itemBackgroundColor}; border-color: ${itemBackgroundColor};`,
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
      if (timeline) {
        timeline.destroy()
      }
    }
  }, [itemBackgroundColor, options, props.data, props.groups])

  return (
    <Spin
      spinning={props.isLoading || isTimelineLoading}
      tip="Loading timeline..."
      size="large"
    >
      <div ref={timelineRef} />
      {(!props.data || props.data.length === 0) &&
        (!props.groups || props.groups.length === 0) && (
          <ModaEmpty message="No timeline data" />
        )}
    </Spin>
  )
}

export default ModaTimeline
