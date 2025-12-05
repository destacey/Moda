'use client'

import React, {
  memo,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react'
import { createRoot } from 'react-dom/client'
import { DataSet } from 'vis-data'
import { Timeline, TimelineOptions } from 'vis-timeline/standalone'
import { TimelineOptionsTemplateFunction } from '@/src/lib/vis-timeline'
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
import {
  FileImageOutlined,
  FullscreenExitOutlined,
  FullscreenOutlined,
  UndoOutlined,
} from '@ant-design/icons'
import { saveElementAsImage } from '@/src/utils'
import { Options } from 'html2canvas'
import { TimelineOptionsItemCallbackFunction } from 'vis-timeline'
import dayjs from 'dayjs'

const ModaTimeline = <TItem extends ModaDataItem, TGroup extends ModaDataGroup>(
  props: ModaTimelineProps<TItem, TGroup>,
) => {
  const [isTimelineLoading, setIsTimelineLoading] = useState(false)
  const [isFullScreen, setIsFullScreen] = useState(false)
  const [reinitTrigger, setReinitTrigger] = useState(0)

  // Store both container and root for cleanup
  const elementMapRef = useRef<
    Record<
      string | number,
      { container: HTMLElement; root: ReturnType<typeof createRoot> }
    >
  >({})
  const timelineRef = useRef<HTMLDivElement>(null)
  const containerRef = useRef<HTMLDivElement>(null)
  const timelineInstanceRef = useRef<Timeline | null>(null)
  const datasetItemsRef = useRef<DataSet<TItem> | null>(null)
  const datasetGroupsRef = useRef<DataSet<TGroup> | null>(null)
  const initialWindowRef = useRef<{ start: Date; end: Date } | null>(null)
  const isInitializedRef = useRef(false)
  const dynamicOptionsRef = useRef<TimelineOptions>({})

  const { currentThemeName, token } = useTheme()

  const enableFullScreenToggle = props.allowFullScreen ?? false
  const enableSaveAsImage = props.allowSaveAsImage ?? false

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
      if (elementMapRef.current?.[mapId])
        return elementMapRef.current[mapId].container

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

      // Store the rendered element container and root to reference later
      elementMapRef.current[mapId] = { container, root }

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
      if (elementMapRef.current?.[mapId])
        return elementMapRef.current[mapId].container

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

      // Store the rendered element container and root to reference later
      elementMapRef.current[mapId] = { container, root }

      // Return the new container
      return container
    },
    [colors.item.font, colors.item.foreground, props],
  )

  const onMoveProp = props.onMove
  const onMove = useCallback<TimelineOptionsItemCallbackFunction>(
    (item, callback) => {
      const original = datasetItemsRef.current?.get(item.id)

      if (!original) return

      if (
        dayjs(original.end).isSame(dayjs(item.end)) &&
        dayjs(original.start).isSame(dayjs(item.start))
      )
        return

      // TODO: Account for total days in a month when moving months

      // Call callback to confirm the move visually
      callback(item)

      // Notify parent if they have a handler
      onMoveProp?.(item)
    },
    [onMoveProp],
  )

  const baseOptions = useMemo((): TimelineOptions => {
    return {
      editable: {
        updateTime: onMoveProp ? true : false,
        updateGroup: false,
        remove: false,
        add: false,
        overrideItems: false,
      },
      onMove,
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
    onMove,
    onMoveProp,
  ])

  useEffect(() => {
    // Always update dynamicOptionsRef based on fullscreen state
    const maxHeight = isFullScreen ? window.innerHeight - 100 : baseOptions.maxHeight
    const updatedOptions = {
      ...baseOptions,
      maxHeight,
    }
    dynamicOptionsRef.current = updatedOptions

    // Update existing timeline instance if already initialized
    if (timelineInstanceRef.current && isInitializedRef.current) {
      timelineInstanceRef.current.setOptions(updatedOptions)
    }

    // Add resize listener when in fullscreen
    if (isFullScreen) {
      const handleResize = () => {
        if (!timelineInstanceRef.current || !isInitializedRef.current) return
        const newMaxHeight = window.innerHeight - 100
        const resizedOptions = {
          ...baseOptions,
          maxHeight: newMaxHeight,
        }
        dynamicOptionsRef.current = resizedOptions
        timelineInstanceRef.current.setOptions(resizedOptions)
      }

      window.addEventListener('resize', handleResize)
      return () => {
        window.removeEventListener('resize', handleResize)
      }
    }
  }, [isFullScreen, baseOptions])

  // Initialize or reinitialize timeline when structure changes (item count or group count)
  useEffect(() => {
    // Don't initialize if loading or no data
    if (props.isLoading) return
    if (!timelineRef.current) return
    if (Object.keys(dynamicOptionsRef.current).length === 0) return

    // Don't initialize if no data
    if (props.data.length === 0) return

    // If already initialized, don't reinitialize - use DataSet updates instead
    if (isInitializedRef.current) return

    setIsTimelineLoading(true)

    const datasetItems = new DataSet([] as TItem[])
    props.data.forEach((item) => {
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

    datasetItemsRef.current = datasetItems

    if (props.groups?.length && props.groups.length > 0) {
      const datasetGroups = new DataSet(props.groups)
      datasetGroupsRef.current = datasetGroups
      timelineInstanceRef.current = new Timeline(
        timelineRef.current,
        datasetItems,
        datasetGroups,
        dynamicOptionsRef.current,
      )
    } else {
      timelineInstanceRef.current = new Timeline(
        timelineRef.current,
        datasetItems,
        dynamicOptionsRef.current,
      )
    }

    // Store initial window for reset functionality
    const opts = dynamicOptionsRef.current
    if (opts.start && opts.end) {
      initialWindowRef.current = {
        start: opts.start as Date,
        end: opts.end as Date,
      }
    }

    isInitializedRef.current = true

    setTimeout(() => {
      setIsTimelineLoading(false)
    }, 800)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    props.isLoading,
    props.data.length, // Reinit when item count changes
    props.groups?.length, // Reinit when group count changes
    reinitTrigger, // Reinit when explicitly triggered (e.g., groups removed)
    // Intentionally NOT including props.data or props.groups to avoid reinit on content changes
    colors.item.background,
    colors.background.background,
  ])

  // Update data when it changes (after initialization)
  useEffect(() => {
    if (!isInitializedRef.current || !datasetItemsRef.current) return

    const processedItems = props.data.map((item) => {
      const backgroundColor = item.itemColor ?? colors.item.background
      return {
        ...item,
        itemColor: backgroundColor,
        style: item.style
          ? item.style
          : item.type === 'range'
            ? `background: ${backgroundColor}; border-color: ${backgroundColor};`
            : item.type === 'background'
              ? `background: ${colors.background.background}; border-style: inset; border-width: 1px;`
              : undefined,
      } as TItem
    })

    const currentIds = datasetItemsRef.current.getIds()
    const newIds = processedItems.map((item) => item.id)

    const toRemove = currentIds.filter((id) => !newIds.includes(id))
    if (toRemove.length > 0) {
      datasetItemsRef.current.remove(toRemove)
      toRemove.forEach((id) => {
        if (elementMapRef.current[id]) {
          const { root } = elementMapRef.current[id]
          setTimeout(() => {
            try {
              root.unmount()
            } catch (error) {
              console.error('Error unmounting root:', error)
            }
          }, 0)
          delete elementMapRef.current[id]
        }
      })
    }

    processedItems.forEach((item) => {
      const existing = datasetItemsRef.current!.get(item.id)
      if (existing) {
        datasetItemsRef.current!.update(item as any)
      } else {
        datasetItemsRef.current!.add(item as any)
      }
    })
  }, [props.data, colors.item.background, colors.background.background])

  // Update groups when they change (after initialization)
  useEffect(() => {
    if (!isInitializedRef.current || !timelineInstanceRef.current) return

    // Don't reinitialize if we're just loading - wait for data to arrive
    if (props.isLoading) return

    // Groups removed - need to reinitialize without groups
    if (
      (!props.groups || props.groups.length === 0) &&
      datasetGroupsRef.current
    ) {
      if (timelineInstanceRef.current) {
        timelineInstanceRef.current.destroy()
        timelineInstanceRef.current = null
      }

      datasetItemsRef.current = null
      datasetGroupsRef.current = null
      isInitializedRef.current = false

      // Trigger reinit by updating state
      setReinitTrigger((prev) => prev + 1)
      return
    }

    // Groups arrived but we initialized without them - add groups to existing timeline
    if (props.groups && props.groups.length > 0 && !datasetGroupsRef.current) {
      const datasetGroups = new DataSet(props.groups)
      datasetGroupsRef.current = datasetGroups
      timelineInstanceRef.current.setGroups(datasetGroups)

      // Re-apply options to ensure they're still set correctly after adding groups
      timelineInstanceRef.current.setOptions(dynamicOptionsRef.current)
      return
    }

    if (!datasetGroupsRef.current || !props.groups) return

    const currentIds = datasetGroupsRef.current.getIds()
    const newIds = props.groups.map((g) => g.id)

    const toRemove = currentIds.filter((id) => !newIds.includes(id))
    if (toRemove.length > 0) {
      datasetGroupsRef.current.remove(toRemove)
    }

    props.groups.forEach((group) => {
      const existing = datasetGroupsRef.current!.get(group.id)
      if (existing) {
        datasetGroupsRef.current!.update(group as any)
      } else {
        datasetGroupsRef.current!.add(group as any)
      }
    })
  }, [props.groups, props.isLoading])

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      const roots = Object.values(elementMapRef.current).map(({ root }) => root)
      setTimeout(() => {
        roots.forEach((root) => {
          try {
            root.unmount()
          } catch (error) {
            console.error('Error unmounting root:', error)
          }
        })
      }, 0)
      elementMapRef.current = {}
      if (timelineInstanceRef.current) {
        timelineInstanceRef.current.destroy()
        timelineInstanceRef.current = null
      }
      datasetItemsRef.current = null
      datasetGroupsRef.current = null
      isInitializedRef.current = false
    }
  }, [])

  const toggleFullScreen = () => {
    if (!enableFullScreenToggle) return
    setIsFullScreen(!isFullScreen)
  }

  const saveTimelineAsImage = () => {
    if (timelineRef.current) {
      const canvasOptions: Partial<Options> = {
        backgroundColor: token.colorBgContainer,
      }

      saveElementAsImage(timelineRef.current, 'timeline.png', canvasOptions)
    }
  }

  const resetWindow = () => {
    if (timelineInstanceRef.current && initialWindowRef.current) {
      timelineInstanceRef.current.setWindow(
        initialWindowRef.current.start,
        initialWindowRef.current.end,
        { animation: true },
      )
    }
  }

  const isLoading = props.isLoading || isTimelineLoading
  const hasData = props.data && props.data.length > 0
  const showControls = !isLoading && hasData

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
        {enableFullScreenToggle && showControls && (
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
              zIndex: 1000,
            }}
          />
        )}
        {enableSaveAsImage && showControls && (
          <Button
            type="text"
            shape="circle"
            title="Save Timeline as Image"
            aria-label="Save Timeline as Image"
            icon={<FileImageOutlined />}
            onClick={saveTimelineAsImage}
            size="small"
            style={{
              position: 'absolute',
              top: isFullScreen ? 25 : 5,
              right: isFullScreen ? 65 : 45,
              zIndex: 1000,
            }}
          />
        )}
        {showControls && (
          <Button
            type="text"
            shape="circle"
            title="Reset Timeline View"
            aria-label="Reset Timeline View"
            icon={<UndoOutlined />}
            onClick={resetWindow}
            size="small"
            style={{
              position: 'absolute',
              top: isFullScreen ? 25 : 5,
              right: enableSaveAsImage
                ? isFullScreen
                  ? 105
                  : 85
                : enableFullScreenToggle
                  ? isFullScreen
                    ? 65
                    : 45
                  : 5,
              zIndex: 1000,
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

export default memo(ModaTimeline)
