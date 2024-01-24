import { createContext } from 'react'

export interface AzdoBoardsConnectionContextInterface {
  connectionId: string
  organizationUrl: string
}

export const AzdoBoardsConnectionContext =
  createContext<AzdoBoardsConnectionContextInterface | null>(null)
