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
        get length() {
          return Object.keys(store).length
        },
        key: (index: number) => {
          const keys = Object.keys(store)
          return keys[index] || null
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

  describe('versioning', () => {
    it('uses versioned key when version is provided', () => {
      const { result } = renderHook(() =>
        useLocalStorageState('key', 'default', { version: 1 }),
      )
      act(() => result.current[1]('newValue'))
      expect(localStorage.getItem('key:v1')).toBe(JSON.stringify('newValue'))
    })

    it('reads from versioned key when version is provided', () => {
      localStorage.setItem('key:v1', JSON.stringify('storedValue'))
      const { result } = renderHook(() =>
        useLocalStorageState('key', 'default', { version: 1 }),
      )
      expect(result.current[0]).toBe('storedValue')
    })

    it('returns default value when versioned key does not exist', () => {
      const { result } = renderHook(() =>
        useLocalStorageState('key', 'default', { version: 2 }),
      )
      expect(result.current[0]).toBe('default')
    })

    it('cleans up old versions when version changes', () => {
      // Set up old version
      localStorage.setItem('key:v1', JSON.stringify('oldValue'))
      localStorage.setItem('key:v2', JSON.stringify('olderValue'))

      // Use new version
      renderHook(() => useLocalStorageState('key', 'default', { version: 3 }))

      // Old versions should be cleaned up
      expect(localStorage.getItem('key:v1')).toBeNull()
      expect(localStorage.getItem('key:v2')).toBeNull()
      expect(localStorage.getItem('key:v3')).toBe(JSON.stringify('default'))
    })

    it('maintains backward compatibility without version', () => {
      localStorage.setItem('key', JSON.stringify('storedValue'))
      const { result } = renderHook(() => useLocalStorageState('key', 'default'))
      expect(result.current[0]).toBe('storedValue')
      expect(localStorage.getItem('key')).toBe(JSON.stringify('storedValue'))
    })

    it('does not interfere with non-versioned keys', () => {
      localStorage.setItem('key', JSON.stringify('nonVersioned'))
      localStorage.setItem('key:v1', JSON.stringify('versioned'))

      // Non-versioned usage
      const { result: nonVersioned } = renderHook(() =>
        useLocalStorageState('key', 'default'),
      )
      expect(nonVersioned.current[0]).toBe('nonVersioned')

      // Versioned usage
      const { result: versioned } = renderHook(() =>
        useLocalStorageState('key', 'default', { version: 1 }),
      )
      expect(versioned.current[0]).toBe('versioned')
    })
  })
})
