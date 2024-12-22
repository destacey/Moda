import { renderHook, act } from '@testing-library/react'
import { useLocalStorageState } from '.'

describe('useLocalStorageState', () => {
  // Suppress console errors during tests
  beforeAll(() => {
    console.error = jest.fn()
  })

  beforeEach(() => {
    // Mock localStorage
    const localStorageMock = (() => {
      let store: Record<string, string> = {}
      return {
        getItem: (key: string) => store[key] || null,
        setItem: (key: string, value: string) => {
          store[key] = value
        },
        removeItem: (key: string) => {
          delete store[key]
        },
        clear: () => {
          store = {}
        },
      }
    })()

    Object.defineProperty(window, 'localStorage', {
      value: localStorageMock,
      writable: true,
    })

    localStorage.clear()
  })

  it('returns the default value if key does not exist', () => {
    const { result } = renderHook(() => useLocalStorageState('key', 'default'))
    expect(result.current[0]).toBe('default')
  })

  it('reads the stored value from localStorage', () => {
    localStorage.setItem('key', JSON.stringify('storedValue'))
    const { result } = renderHook(() => useLocalStorageState('key', 'default'))
    expect(result.current[0]).toBe('storedValue')
  })

  it('updates localStorage when value changes', () => {
    const { result } = renderHook(() => useLocalStorageState('key', 'default'))
    act(() => result.current[1]('newValue'))
    expect(localStorage.getItem('key')).toBe(JSON.stringify('newValue'))
  })

  it('handles invalid JSON gracefully', () => {
    localStorage.setItem('key', 'invalidJSON')
    const { result } = renderHook(() => useLocalStorageState('key', 'default'))
    expect(result.current[0]).toBe('default')
  })

  it('avoids redundant updates to localStorage', () => {
    localStorage.setItem('key', JSON.stringify('value'))
    const { result } = renderHook(() => useLocalStorageState('key', 'value'))
    const setItemSpy = jest.spyOn(Storage.prototype, 'setItem')

    act(() => result.current[1]('value'))
    expect(setItemSpy).not.toHaveBeenCalled()
  })
})
