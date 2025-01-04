'use client'

import { ColorPicker, ColorPickerProps, Space } from 'antd'
import {
  generate,
  green,
  presetPalettes,
  red,
  yellow,
  grey,
  orange,
  cyan,
  purple,
  magenta,
} from '@ant-design/colors'
import useTheme from '../contexts/theme'
import { useState } from 'react'
import { Color } from 'antd/es/color-picker'

export interface ModaColorPickerProps {
  color?: string | undefined
  onChange: (color: string) => void
}

type Presets = Required<ColorPickerProps>['presets'][number]

const redPalette = red.slice(2, 8).filter((_, index) => index % 2 === 0)
const orangePalette = orange.slice(2, 8).filter((_, index) => index % 2 === 0)
const yellowPalette = yellow.slice(2, 8).filter((_, index) => index % 2 === 0)
const greenPalette = green.slice(2, 8).filter((_, index) => index % 2 === 0)
const cyanPalette = cyan.slice(2, 8).filter((_, index) => index % 2 === 0)
const purplePalette = purple.slice(2, 8).filter((_, index) => index % 2 === 0)
const magentaPalette = magenta.slice(2, 8).filter((_, index) => index % 2 === 0)
const greyPalette = grey.slice(1, 6).filter((_, index) => index % 2 === 0)

const genPresets = (presets = presetPalettes) =>
  Object.entries(presets).map<Presets>(([label, colors]) => ({ label, colors }))

const customPanelRender: ColorPickerProps['panelRender'] = (
  _,
  { components: { Presets } },
) => (
  <Space style={{ width: '85px' }}>
    <Presets />
  </Space>
)

const ModaColorPicker = (props: ModaColorPickerProps) => {
  const [selectedColor, setSelectedColor] = useState<string | undefined>(
    props.color,
  )
  const { token } = useTheme()

  const primaryPalette = generate(token.colorPrimary)
    .slice(2, 8)
    .filter((_, index) => index % 2 === 0)

  const colorPresets = genPresets({
    colors: [
      ...primaryPalette,
      ...redPalette,
      ...orangePalette,
      ...yellowPalette,
      ...greenPalette,
      ...cyanPalette,
      ...purplePalette,
      ...magentaPalette,
      ...greyPalette,
    ],
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
