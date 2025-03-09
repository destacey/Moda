import { useEffect, useState, useCallback } from 'react'

export const useLocalStorageState = <T>(
  key: string,
  defaultValue: T,
): [T, React.Dispatch<React.SetStateAction<T>>] => {
  const [value, setValue] = useState<T>(() => {
    if (typeof window === 'undefined') {
      return defaultValue
    }
    try {
      const storedValue = window.localStorage.getItem(key)
      return storedValue !== null ? JSON.parse(storedValue) : defaultValue
    } catch (error) {
      console.error(`Error reading localStorage key "${key}":`, error)
      return defaultValue
    }
  })

  // Update localStorage when value changes.
  useEffect(() => {
    try {
      window.localStorage.setItem(key, JSON.stringify(value))
    } catch (error) {
      console.error(`Error writing localStorage key "${key}":`, error)
    }
  }, [key, value])

  // Listen for changes to localStorage from other tabs/windows.
  useEffect(() => {
    const handleStorageChange = (event: StorageEvent) => {
      if (event.key === key) {
        try {
          const newValue = event.newValue
            ? JSON.parse(event.newValue)
            : defaultValue
          setValue(newValue)
        } catch (error) {
          console.error(
            `Error parsing localStorage key "${key}" on storage event:`,
            error,
          )
        }
      }
    }

    window.addEventListener('storage', handleStorageChange)
    return () => {
      window.removeEventListener('storage', handleStorageChange)
    }
  }, [key, defaultValue])

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
