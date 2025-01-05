'use client'

import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { createRoot } from 'react-dom/client'
import {
  DataSet,
  Timeline,
  TimelineOptions,
  TimelineOptionsTemplateFunction,
} from 'vis-timeline/standalone'
import { Button, Spin } from 'antd'
import useTheme from '../../contexts/theme'
import { ModaEmpty } from '..'
import './moda-timeline.css'
import {
  DefaultTimeLineColors,
  getDefaultTemplate,
} from './moda-timeline.utils'
import {
  ModaDataGroup,
  ModaDataItem,
  ModaTimelineProps,
  TimelineTemplate,
} from '.'
import { FullscreenExitOutlined, FullscreenOutlined } from '@ant-design/icons'

const ModaTimeline = <TItem extends ModaDataItem, TGroup extends ModaDataGroup>(
  props: ModaTimelineProps<TItem, TGroup>,
) => {
  const [isTimelineLoading, setIsTimelineLoading] = useState(false)
  const [isFullScreen, setIsFullScreen] = useState(false)

  const elementMapRef = useRef<Record<string | number, HTMLElement>>({})
  const timelineRef = useRef<HTMLDivElement>(null)
  const containerRef = useRef<HTMLDivElement>(null)

  const { currentThemeName, token } = useTheme()

  const enableFullScreenToggle = props.allowFullScreen ?? false

  const colors = useMemo(
    () => DefaultTimeLineColors[currentThemeName],
    [currentThemeName],
  )

  const itemTemplateManager = useCallback<
    TimelineOptionsTemplateFunction<TItem>
  >(
    (item, element, _) => {
      if (!item) return ''

      const mapId = item.id ?? 0
      if (elementMapRef.current?.[mapId]) return elementMapRef.current[mapId]

      // Create a container for the React element (prevents DOM node errors)
      const container = document.createElement('div')

      if (element) element.appendChild(container)

      const root = createRoot(container)

      // Unfortunately, typescript doesn't seem to handle nested constrained generics very well (see: https://github.com/microsoft/TypeScript/issues/23132)
      //  so we must add the type annotation here to avoid the error
      const Template: TimelineTemplate<ModaDataItem> = getDefaultTemplate(
        item.type,
        props,
      )

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
    TimelineOptionsTemplateFunction<TGroup>
  >(
    (item, element, _) => {
      if (!item) return ''

      const mapId = item.id ?? 0
      if (elementMapRef.current?.[mapId]) return elementMapRef.current[mapId]

      // Create a container for the react element (prevents DOM node errors)
      const container = document.createElement('div')
      element?.appendChild(container)

      const root = createRoot(container)

      // Unfortunately, typescript doesn't seem to handle nested constrained generics very well (see: https://github.com/microsoft/TypeScript/issues/23132)
      //  so we must add the type annotation here to avoid the error
      const Template: TimelineTemplate<ModaDataGroup> = getDefaultTemplate(
        'group',
        props,
      )

      if (Template) {
        root.render(
          <Template
            item={item}
            fontColor={colors.item.font}
            foregroundColor={colors.item.foreground}
            parentElement={element}
          />,
        )
      }

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
      groupHeightMode: 'auto',
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

    const datasetItems = new DataSet([] as TItem[])
    props.data.map((item) => {
      const backgroundColor = item.itemColor ?? colors.item.background
      const newItem: TItem = {
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

      datasetItems.add(newItem)
    })

    let timeline: Timeline

    if (timelineRef.current) {
      if (props.groups?.length && props.groups?.length > 0) {
        // const datasetGroups = new DataSet([] as TGroup[])
        // props.groups.map((group) => {
        //   const newGroup: TGroup = {
        //     ...group,
        //   }

        //   datasetGroups.add(newGroup)
        // })

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
      // it takes just under a second to render the timeline
      setIsTimelineLoading(false)
    }, 800)

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

  const toggleFullScreen = () => {
    if (!enableFullScreenToggle) return
    setIsFullScreen(!isFullScreen)
  }

  const isLoading = props.isLoading || isTimelineLoading

  return (
    <Spin spinning={isLoading} tip="Loading timeline..." size="large">
      <div
        ref={containerRef}
        style={{
          position: isFullScreen ? 'fixed' : 'relative',
          top: 0,
          left: 0,
          width: isFullScreen ? '100vw' : 'auto',
          height: isFullScreen ? '100vh' : 'auto',
          zIndex: isFullScreen ? 1000 : 'auto',
          transition: 'all 0.3s ease',
          background: isFullScreen ? token.colorBgContainer : 'transparent',
          padding: isFullScreen ? 20 : 0,
          overflow: isFullScreen ? 'auto' : 'unset',
        }}
      >
        {enableFullScreenToggle && !isLoading && (
          <Button
            type="text"
            shape="circle"
            title={isFullScreen ? 'Exit Fullscreen' : 'Enter Fullscreen'}
            aria-label={
              isFullScreen ? 'Exit Fullscreen Mode' : 'Enter Fullscreen Mode'
            }
            icon={
              isFullScreen ? <FullscreenExitOutlined /> : <FullscreenOutlined />
            }
            onClick={toggleFullScreen}
            size="small"
            style={{
              position: 'absolute',
              top: isFullScreen ? 25 : 5,
              right: isFullScreen ? 25 : 5,
              zIndex: 1100,
            }}
          />
        )}
        <div ref={timelineRef} />
        {!isLoading &&
          (!props.data || props.data.length === 0) &&
          (!props.groups || props.groups.length === 0) && (
            <ModaEmpty message={props.emptyMessage ?? 'No timeline data'} />
          )}
      </div>
    </Spin>
  )
}
ModaTimeline.displayName = 'ModaTimeline'

export default ModaTimeline
