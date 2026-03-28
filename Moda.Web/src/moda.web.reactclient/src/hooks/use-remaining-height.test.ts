import { renderHook, act } from '@testing-library/react'
import { useRemainingHeight } from './use-remaining-height'
import { RefObject } from 'react'

// --- Helpers ---

function createMockRef(top: number): RefObject<HTMLElement> {
  return {
    current: {
      getBoundingClientRect: () => ({
        top,
        left: 0,
        right: 0,
        bottom: 0,
        width: 0,
        height: 0,
        x: 0,
        y: 0,
        toJSON: () => {},
      }),
    } as HTMLElement,
  }
}

function setWindowHeight(height: number) {
  Object.defineProperty(window, 'innerHeight', {
    writable: true,
    configurable: true,
    value: height,
  })
}

// Store original values
const originalInnerHeight = window.innerHeight

afterEach(() => {
  Object.defineProperty(window, 'innerHeight', {
    writable: true,
    configurable: true,
    value: originalInnerHeight,
  })
})

describe('useRemainingHeight', () => {
  it('returns default height of 500 when ref is null', () => {
    const ref = { current: null } as RefObject<HTMLElement>
    const { result } = renderHook(() => useRemainingHeight(ref))

    expect(result.current).toBe(500)
  })

  it('calculates remaining height based on element top position', () => {
    setWindowHeight(1000)
    const ref = createMockRef(200)

    const { result } = renderHook(() => useRemainingHeight(ref))

    // 1000 - 200 - 30 (default offset) = 770
    expect(result.current).toBe(770)
  })

  it('applies custom bottom offset', () => {
    setWindowHeight(1000)
    const ref = createMockRef(200)

    const { result } = renderHook(() => useRemainingHeight(ref, 50))

    // 1000 - 200 - 50 = 750
    expect(result.current).toBe(750)
  })

  it('enforces minimum height of 300', () => {
    setWindowHeight(400)
    const ref = createMockRef(250)

    const { result } = renderHook(() => useRemainingHeight(ref))

    // 400 - 250 - 30 = 120, but min is 300
    expect(result.current).toBe(300)
  })

  it('recalculates on window resize', () => {
    setWindowHeight(1000)
    const ref = createMockRef(100)

    const { result } = renderHook(() => useRemainingHeight(ref))

    expect(result.current).toBe(870) // 1000 - 100 - 30

    act(() => {
      setWindowHeight(800)
      window.dispatchEvent(new Event('resize'))
    })

    expect(result.current).toBe(670) // 800 - 100 - 30
  })

  it('cleans up resize listener on unmount', () => {
    const removeSpy = jest.spyOn(window, 'removeEventListener')
    const ref = createMockRef(100)

    const { unmount } = renderHook(() => useRemainingHeight(ref))
    unmount()

    expect(removeSpy).toHaveBeenCalledWith(
      'resize',
      expect.any(Function),
    )
    removeSpy.mockRestore()
  })
})
