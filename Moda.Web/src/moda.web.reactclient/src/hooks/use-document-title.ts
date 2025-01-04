import { useEffect } from 'react'

export const useDocumentTitle = (
  title: string,
  defaultTitle: string = '',
): void => {
  useEffect(() => {
    const previousTitle = document.title
    document.title = title

    return () => {
      document.title = defaultTitle || previousTitle
    }
  }, [title, defaultTitle])
}
