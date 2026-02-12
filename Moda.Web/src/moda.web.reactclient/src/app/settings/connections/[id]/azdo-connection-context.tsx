import { AzureDevOpsConnectionDetailsDto } from '@/src/services/moda-api'
import { QueryTags } from '@/src/store/features/query-tags'
import {
  QueryActionCreatorResult,
  QueryDefinition,
} from '@reduxjs/toolkit/query'
import { createContext } from 'react'

export interface AzdoConnectionContextInterface {
  connectionId: string
  organizationUrl: string
  reloadConnectionData: () => QueryActionCreatorResult<
    QueryDefinition<
      string,
      any,
      QueryTags,
      AzureDevOpsConnectionDetailsDto,
      'api'
    >
  >
}

export const AzdoConnectionContext =
  createContext<AzdoConnectionContextInterface | null>(null)
