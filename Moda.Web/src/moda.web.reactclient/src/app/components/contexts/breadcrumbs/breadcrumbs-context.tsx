import { createContext, useCallback, useState } from 'react'
import { BreadcrumbContextType } from './types'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { usePathname } from 'next/navigation'

export const BreadcrumbsContext = createContext<BreadcrumbContextType | null>(
  null
)

export const BreadcrumbsProvider = ({ children }) => {
  const [ breadcrumbRoute, setBreadcrumbRoute ] = useState<{pathname: string, title?: string, route?: ItemType[]}>(null)
  const pathname = usePathname()

  const setRoute = useCallback((route: ItemType[]) => {
    setBreadcrumbRoute({pathname, route: route})
  }, [pathname])

  const setTitle = useCallback((title: string) => {
    setBreadcrumbRoute({pathname, title})
  }, [pathname])

  return (
    <BreadcrumbsContext.Provider value={{ breadcrumbRoute, setBreadcrumbTitle: setTitle, setBreadcrumbRoute: setRoute }}>
      {children}
    </BreadcrumbsContext.Provider>
  )
}
