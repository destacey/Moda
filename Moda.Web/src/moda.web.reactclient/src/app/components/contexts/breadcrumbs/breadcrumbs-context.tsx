import { createContext, use, useEffect, useState } from 'react'
import { BreadcrumbsContextType, BreadcrumbModel } from './types'

export const BreadcrumbsContext = createContext<BreadcrumbsContextType | null>(
  null
)

export const BreadcrumbsProvider = ({ children }) => {
  const [breadcrumbs, setBreadcrumbs] = useState<BreadcrumbModel[]>()

  // useEffect(() => {
  //   setBreadcrumbs([])
  // }, [breadcrumbs])

  return (
    <BreadcrumbsContext.Provider value={{ breadcrumbs, setBreadcrumbs }}>
      {children}
    </BreadcrumbsContext.Provider>
  )
}
