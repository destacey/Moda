'use client'

import React from 'react'
import { Typography } from 'antd'
import { getLuminance } from '@/src/utils/color-helper'
import { ModaDataGroup, ModaDataItem, TimelineTemplate } from './types'

const { Text } = Typography

export const RangeItemTemplate: TimelineTemplate<ModaDataItem> = (props) => {
  // TODO: The 0.6 needs to be tested with some of the other colors
  // TODO: ItemColor is optional,
  const fontColor =
    getLuminance(props.item.itemColor ?? '') > 0.6 ? '#4d4d4d' : '#FFFFFF'

  return (
    <Text style={{ padding: '5px', color: fontColor }}>
      {props.item.content}
    </Text>
  )
}

export const GroupTemplate: TimelineTemplate<ModaDataGroup> = (props) => {
  return (
    <Text
      style={{
        color: props.fontColor,
      }}
    >
      {
        // TODO: Fix TS Error. VisJS DataGroup.content is either `string` or `HTMLElement`, and `HTMLElement` is not a valid reactNode.
      }
      {props.item.content as string}
    </Text>
  )
}

export const BackgroundItemTemplate: TimelineTemplate<ModaDataItem> = (
  props,
) => {
  return (
    <div>
      <Text>{props.item.content}</Text>
    </div>
  )
}
