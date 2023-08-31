import { createContext, useCallback, useEffect, useState } from 'react'
import { BreadcrumbContextType } from './types'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { usePathname } from 'next/navigation'
import { routes } from '.'

export const BreadcrumbsContext = createContext<BreadcrumbContextType | null>(
  null,
)

export const BreadcrumbsProvider = ({ children }) => {
  const [breadcrumbRoute, setBreadcrumbRoute] = useState<{
    pathname: string
    route?: ItemType[]
    defaultRoute?: ItemType[]
  }>(null)
  const [isVisible, setIsVisible] = useState(true)
  const [overrideForPath, setOverrideForPath] = useState<string>('')
  const pathname = usePathname()

  const setRoute = useCallback(
    (route: ItemType[]) => {
      setBreadcrumbRoute({ pathname, route })
    },
    [pathname],
  )

  const generateRoute = useCallback(
    (pathname: string, lastItemTitleOverride?: string): ItemType[] => {
      const pathSegments = pathname.split('/').filter((item) => item !== '')
      return pathSegments.map((item, index) => {
        // Set the title of the last path segment to the title passed in from the page
        let titleOverride = {}
        if (lastItemTitleOverride && index === pathSegments.length - 1) {
          titleOverride = { title: lastItemTitleOverride }
        }

        return {
          path: item,
          href: '/' + pathSegments.slice(0, index + 1).join('/'),
          ...routes[item],
          ...titleOverride,
        }
      })
    },
    [],
  )

  const setTitle = useCallback(
    (title: string) => {
      const route = generateRoute(pathname, title)
      setBreadcrumbRoute({ pathname, route })
      setOverrideForPath(pathname)
    },
    [pathname, generateRoute],
  )

  useEffect(() => {
    setIsVisible(true)
    // Only set the default route if it hasn't been set already

    if (breadcrumbRoute?.pathname !== pathname && overrideForPath !== pathname) {
      const defaultRoute = generateRoute(pathname)
      setBreadcrumbRoute({ pathname, route: defaultRoute })
    }
  }, [pathname, generateRoute, breadcrumbRoute, overrideForPath])

  return (
    <BreadcrumbsContext.Provider
      value={{
        breadcrumbRoute,
        isVisible: isVisible,
        setBreadcrumbIsVisible: setIsVisible,
        setBreadcrumbTitle: setTitle,
        setBreadcrumbRoute: setRoute,
      }}
    >
      {children}
    </BreadcrumbsContext.Provider>
  )
}
