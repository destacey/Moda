'use client'

import { createContext, useMemo } from 'react'
import { MenuToggleContextType } from './types'
import { useLocalStorageState } from '@/src/hooks'

export const MenuToggleContext = createContext<MenuToggleContextType | null>(
  null,
)

export const MenuToggleProvider = ({ children }) => {
  const [menuCollapsed, setMenuCollapsed] = useLocalStorageState<boolean>(
    'appMenuCollapsed',
    true,
  )

  const menuToggleContextValue = useMemo(
    () => ({ menuCollapsed, setMenuCollapsed }),
    [menuCollapsed, setMenuCollapsed],
  )

  return (
    <MenuToggleContext.Provider value={menuToggleContextValue}>
      {children}
    </MenuToggleContext.Provider>
  )
}
