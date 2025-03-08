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

  useEffect(() => {
    try {
      window.localStorage.setItem(key, JSON.stringify(value))
    } catch (error) {
      console.error(`Error writing localStorage key "${key}":`, error)
    }
  }, [key, value])

  const setLocalStorageValue = useCallback(
    (newValue: React.SetStateAction<T>) => {
      setValue((prevValue) => {
        const valueToStore =
          typeof newValue === 'function'
            ? (newValue as (prevState: T) => T)(prevValue)
            : newValue

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
