import { useEffect, useState, useCallback } from 'react'

export interface UseLocalStorageStateOptions {
  /**
   * Optional version number for the stored data.
   * When the version changes, old data is automatically cleared and defaults are used.
   * This prevents schema conflicts when data structures evolve.
   */
  version?: number
}

export const useLocalStorageState = <T>(
  key: string,
  defaultValue: T,
  options?: UseLocalStorageStateOptions,
): [T, React.Dispatch<React.SetStateAction<T>>] => {
  const version = options?.version
  const versionedKey = version !== undefined ? `${key}:v${version}` : key

  const [value, setValue] = useState<T>(() => {
    if (typeof window === 'undefined') {
      return defaultValue
    }
    try {
      // Try to read versioned key first
      const storedValue = window.localStorage.getItem(versionedKey)
      if (storedValue !== null) {
        return JSON.parse(storedValue)
      }

      // If versioned key not found and we're using versioning, clean up old versions
      if (version !== undefined) {
        // Collect keys to remove (don't modify during iteration)
        const keysToRemove: string[] = []
        for (let i = 0; i < window.localStorage.length; i++) {
          const storageKey = window.localStorage.key(i)
          if (storageKey?.startsWith(`${key}:v`) && storageKey !== versionedKey) {
            keysToRemove.push(storageKey)
          }
        }
        // Remove old versioned keys
        keysToRemove.forEach((k) => window.localStorage.removeItem(k))
      }

      return defaultValue
    } catch (error) {
      console.error(`Error reading localStorage key "${versionedKey}":`, error)
      return defaultValue
    }
  })

  // Update localStorage when value changes.
  useEffect(() => {
    try {
      window.localStorage.setItem(versionedKey, JSON.stringify(value))
    } catch (error) {
      console.error(`Error writing localStorage key "${versionedKey}":`, error)
    }
  }, [versionedKey, value])

  // Listen for changes to localStorage from other tabs/windows.
  useEffect(() => {
    const handleStorageChange = (event: StorageEvent) => {
      if (event.key === versionedKey) {
        try {
          const newValue = event.newValue
            ? JSON.parse(event.newValue)
            : defaultValue
          setValue(newValue)
        } catch (error) {
          console.error(
            `Error parsing localStorage key "${versionedKey}" on storage event:`,
            error,
          )
        }
      }
    }

    window.addEventListener('storage', handleStorageChange, { passive: true })
    return () => {
      window.removeEventListener('storage', handleStorageChange)
    }
  }, [versionedKey, defaultValue])

  const setLocalStorageValue = useCallback(
    (newValue: React.SetStateAction<T>) => {
      setValue((prevValue) => {
        const valueToStore =
          typeof newValue === 'function'
            ? (newValue as (prevState: T) => T)(prevValue)
            : newValue

        // Only update if value actually changed.
        if (JSON.stringify(prevValue) === JSON.stringify(valueToStore)) {
          return prevValue
        }

        return valueToStore
      })
    },
    [],
  )

  return [value, setLocalStorageValue]
}
