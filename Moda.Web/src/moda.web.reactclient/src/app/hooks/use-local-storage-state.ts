import { useEffect, useState } from "react"

export const useLocalStorageState = <T = any>(key: string, defaultValue: T): [T, (value:T) => void] => {
  const [value, setValue] = useState<T>(() => {
    // If window is undefined, we are on the server and localStorage is not available
    if (typeof window !== "undefined") {
      const storedValue = window.localStorage.getItem(key)
      return storedValue ? JSON.parse(storedValue) : defaultValue
    }
  })

  useEffect(() => {
    window.localStorage.setItem(key, JSON.stringify(value))
  }, [key, value])

  return [value, setValue];
}