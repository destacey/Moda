import { AzureDevOpsBoardsConnectionDetailsDto } from '@/src/services/moda-api'
import { QueryTags } from '@/src/store/features/query-tags'
import {
  QueryActionCreatorResult,
  QueryDefinition,
} from '@reduxjs/toolkit/query'
import { createContext } from 'react'

export interface AzdoBoardsConnectionContextInterface {
  connectionId: string
  organizationUrl: string
  reloadConnectionData: () => QueryActionCreatorResult<
    QueryDefinition<
      string,
      any,
      QueryTags,
      AzureDevOpsBoardsConnectionDetailsDto,
      'api'
    >
  >
}

export const AzdoBoardsConnectionContext =
  createContext<AzdoBoardsConnectionContextInterface | null>(null)
