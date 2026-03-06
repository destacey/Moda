import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import ModaColorPicker from './moda-color-picker'

const latestColorPickerProps: { current: any | null } = { current: null }

jest.mock('antd', () => {
  return {
    ColorPicker: (props: any) => {
      latestColorPickerProps.current = props

      return (
        <div>
          <button
            type="button"
            data-testid="color-picker-trigger"
            className="ant-color-picker-trigger"
            tabIndex={0}
          >
            Trigger
          </button>
          <button
            type="button"
            data-testid="color-picker-open"
            onClick={() => props.onOpenChange?.(true)}
          >
            Open
          </button>
          <button
            type="button"
            data-testid="color-picker-close"
            onClick={() => props.onOpenChange?.(false)}
          >
            Close
          </button>
          <button
            type="button"
            data-testid="color-picker-select"
            onClick={() =>
              props.onChange?.({ toHexString: () => '#123456' } as any)
            }
          >
            Select
          </button>
          <div data-testid="color-picker-open-state">
            {props.open ? 'open' : 'closed'}
          </div>
        </div>
      )
    },
    Space: (props: any) => <div>{props.children}</div>,
  }
})

jest.mock('../contexts/theme', () => ({
  __esModule: true,
  default: () => ({
    token: {
      colorPrimary: '#1890ff',
    },
  }),
}))

describe('ModaColorPicker', () => {
  beforeEach(() => {
    latestColorPickerProps.current = null
    jest
      .spyOn(window, 'requestAnimationFrame')
      .mockImplementation((cb: FrameRequestCallback) => {
        cb(0)
        return 1
      })
  })

  afterEach(() => {
    jest.restoreAllMocks()
  })

  it('renders closed by default', () => {
    render(<ModaColorPicker value="#abcdef" onChange={jest.fn()} />)

    expect(screen.getByTestId('color-picker-trigger')).toBeInTheDocument()
    expect(screen.getByTestId('color-picker-open-state')).toHaveTextContent(
      'closed',
    )
  })

  it('calls onChange with selected color when different from current value', () => {
    const onChange = jest.fn()
    render(<ModaColorPicker value="#000000" onChange={onChange} />)

    fireEvent.click(screen.getByTestId('color-picker-select'))

    expect(onChange).toHaveBeenCalledWith('#123456')
  })

  it('clears color when selecting same value', () => {
    const onChange = jest.fn()
    render(<ModaColorPicker value="#123456" onChange={onChange} />)

    fireEvent.click(screen.getByTestId('color-picker-select'))

    expect(onChange).toHaveBeenCalledWith(undefined)
  })

  it('supports usage without onChange handler', () => {
    render(<ModaColorPicker value="#000000" />)

    expect(() => {
      fireEvent.click(screen.getByTestId('color-picker-select'))
    }).not.toThrow()
  })

  it('closes after selection and refocuses trigger', async () => {
    render(<ModaColorPicker value={undefined} onChange={jest.fn()} />)

    fireEvent.click(screen.getByTestId('color-picker-open'))
    expect(screen.getByTestId('color-picker-open-state')).toHaveTextContent('open')

    fireEvent.click(screen.getByTestId('color-picker-select'))

    await waitFor(() => {
      expect(screen.getByTestId('color-picker-open-state')).toHaveTextContent(
        'closed',
      )
    })

    await waitFor(() => {
      expect(document.activeElement).toBe(screen.getByTestId('color-picker-trigger'))
    })
  })

  it('keeps open state controlled by ModaColorPicker', () => {
    render(<ModaColorPicker value={undefined} onChange={jest.fn()} />)

    expect(latestColorPickerProps.current).toBeTruthy()
    expect(latestColorPickerProps.current.open).toBe(false)
  })
})