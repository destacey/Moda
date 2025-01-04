'use client'

import { createContext } from 'react'
import { MenuToggleContextType } from './types'
import { useLocalStorageState } from '@/src/hooks'

export const MenuToggleContext = createContext<MenuToggleContextType | null>(
  null,
)

export const MenuToggleProvider = ({ children }) => {
  const [menuCollapsed, setMenuCollapsed] = useLocalStorageState<boolean>(
    'modaMenuCollapsed',
    true,
  )

  return (
    <MenuToggleContext.Provider value={{ menuCollapsed, setMenuCollapsed }}>
      {children}
    </MenuToggleContext.Provider>
  )
}
