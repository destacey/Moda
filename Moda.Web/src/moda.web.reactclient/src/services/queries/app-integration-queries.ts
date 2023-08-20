import { useQuery } from 'react-query'
import { QK } from './query-keys'
import { getAzureDevOpsBoardsConnectionsClient } from '../clients'

// CONNECTIONS
export const useGetConnections = (includeDisabled: boolean = false) => {
  return useQuery({
    queryKey: [QK.CONNECTIONS, includeDisabled],
    queryFn: async () =>
      (await getAzureDevOpsBoardsConnectionsClient()).getList(includeDisabled),
    staleTime: 60000,
  })
}
