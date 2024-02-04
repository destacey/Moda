import { AzureDevOpsBoardsConnectionDetailsDto } from '@/src/services/moda-api'
import { createContext } from 'react'
import { QueryObserverResult } from 'react-query'

export interface AzdoBoardsConnectionContextInterface {
  connectionId: string
  organizationUrl: string
  reloadConnectionData: () => Promise<
    QueryObserverResult<AzureDevOpsBoardsConnectionDetailsDto, unknown>
  >
}

export const AzdoBoardsConnectionContext =
  createContext<AzdoBoardsConnectionContextInterface | null>(null)
