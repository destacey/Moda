'use client'

import { ColorPicker, ColorPickerProps } from 'antd'
import {
  generate,
  green,
  presetPalettes,
  red,
  yellow,
  grey,
} from '@ant-design/colors'
import useTheme from '../contexts/theme'
import { useState } from 'react'
import { Color } from 'antd/es/color-picker'

export interface ModaColorPickerProps {
  color?: string | undefined
  onChange: (color: string) => void
}

type Presets = Required<ColorPickerProps>['presets'][number]

const genPresets = (presets = presetPalettes) =>
  Object.entries(presets).map<Presets>(([label, colors]) => ({ label, colors }))

const customPanelRender: ColorPickerProps['panelRender'] = (
  _,
  { components: { Presets } },
) => <Presets />

const ModaColorPicker = (props: ModaColorPickerProps) => {
  const [selectedColor, setSelectedColor] = useState<string | undefined>(
    props.color,
  )
  const { token } = useTheme()
  const colorPresets = genPresets({
    primary: generate(token.colorPrimary)
      .slice(2, 8)
      .filter((_, index) => index % 2 === 0),
    red: red.slice(2, 8).filter((_, index) => index % 2 === 0),
    green: green.slice(2, 8).filter((_, index) => index % 2 === 0),
    yellow: yellow.slice(2, 8).filter((_, index) => index % 2 === 0),
    grey: grey.slice(1, 6).filter((_, index) => index % 2 === 0),
  })

  const onPickerChange = (color: Color) => {
    const colorValue = color.toHexString()
    if (colorValue !== selectedColor) {
      setSelectedColor(colorValue)
    } else {
      setSelectedColor(undefined)
    }
    props.onChange(colorValue)
  }

  return (
    <ColorPicker
      defaultFormat="hex"
      value={selectedColor}
      presets={colorPresets}
      allowClear
      showText
      format="hex"
      size="small"
      onChange={onPickerChange}
      panelRender={customPanelRender}
    />
  )
}

export default ModaColorPicker
