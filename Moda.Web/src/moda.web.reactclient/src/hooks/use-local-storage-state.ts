import { useEffect, useState } from 'react'

export const useLocalStorageState = <T = any>(
  key: string,
  defaultValue: T,
): [T, (value: T) => void] => {
  const [value, setValue] = useState<T>(() => {
    // Safe access to localStorage
    if (typeof window === 'undefined') {
      return defaultValue // Return default value during SSR
    }

    try {
      const storedValue = window.localStorage.getItem(key)
      return storedValue ? JSON.parse(storedValue) : defaultValue
    } catch (error) {
      console.error(`Error reading localStorage key "${key}":`, error)
      return defaultValue
    }
  })

  useEffect(() => {
    // Update localStorage only if value changes
    try {
      window.localStorage.setItem(key, JSON.stringify(value))
    } catch (error) {
      console.error(`Error writing localStorage key "${key}":`, error)
    }
  }, [key, value])

  return [
    value,
    (newValue: T) => {
      setValue((prevValue) => {
        // Prevent redundant updates to localStorage
        if (JSON.stringify(prevValue) !== JSON.stringify(newValue)) {
          return newValue
        }
        return prevValue
      })
    },
  ]
}
