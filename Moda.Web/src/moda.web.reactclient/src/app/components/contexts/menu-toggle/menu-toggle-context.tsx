import { createContext, useEffect, useState } from 'react'
import { MenuToggleContextType } from './types'
import { useLocalStorageState } from '@/src/app/hooks'

export const MenuToggleContext = createContext<MenuToggleContextType | null>(
  null
)

export const MenuToggleProvider = ({ children }) => {
  const [menuCollapsed, setMenuCollapsed] = useLocalStorageState<boolean>(
    'modaMenuCollapsed',
    true
  )

  return (
    <MenuToggleContext.Provider value={{ menuCollapsed, setMenuCollapsed }}>
      {children}
    </MenuToggleContext.Provider>
  )
}
