import { useEffect, useState } from 'react'

export const useDocumentTitle = (title: string) => {
  const [documentTitle, setDocumentTitle] = useState(title)

  useEffect(() => {
    document.title = documentTitle
  }, [documentTitle])

  return [documentTitle, setDocumentTitle]
}
