// src/__tests__/ModaColorPicker.test.tsx
import React from 'react'
import { render, fireEvent } from '@testing-library/react'
import ModaColorPicker from './moda-color-picker'

// Mock the antd components used by the component
jest.mock('antd', () => {
  return {
    // Replace the ColorPicker with a button that simulates color selection
    ColorPicker: (props: any) => {
      const handleClick = () => {
        // Create a dummy color object that implements toHexString
        const dummyColor = { toHexString: () => '#123456' }
        if (props.onChange) {
          props.onChange(dummyColor)
        }
      }
      return (
        <button data-testid="color-picker" onClick={handleClick}>
          ColorPicker
        </button>
      )
    },
    // Simply render children for Space
    Space: (props: any) => <div>{props.children}</div>,
  }
})

// Mock the useTheme hook to supply a token for generating palettes
jest.mock('../contexts/theme', () => ({
  __esModule: true,
  default: () => ({
    token: {
      colorPrimary: '#1890ff',
    },
  }),
}))

describe('ModaColorPicker Component', () => {
  it('renders the color picker button', () => {
    const { getByTestId } = render(
      <ModaColorPicker value="#abcdef" onChange={() => {}} />,
    )
    const button = getByTestId('color-picker')
    expect(button).toBeInTheDocument()
  })

  it('calls onChange with the new color when a different color is selected', () => {
    const onChangeMock = jest.fn()
    // Provide an initial value different from our dummy value "#123456"
    const { getByTestId } = render(
      <ModaColorPicker value="#000000" onChange={onChangeMock} />,
    )
    const button = getByTestId('color-picker')
    fireEvent.click(button)
    expect(onChangeMock).toHaveBeenCalledWith('#123456')
  })

  it('clears the color (calls onChange with undefined) when the same color is selected', () => {
    const onChangeMock = jest.fn()
    // Set the initial value to the dummy color "#123456"
    const { getByTestId } = render(
      <ModaColorPicker value="#123456" onChange={onChangeMock} />,
    )
    const button = getByTestId('color-picker')
    fireEvent.click(button)
    expect(onChangeMock).toHaveBeenCalledWith(undefined)
  })
})
