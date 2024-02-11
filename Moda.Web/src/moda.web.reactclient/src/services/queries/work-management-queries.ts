import { useQuery } from 'react-query'
import { QK } from './query-keys'
import { getWorkTypesClient } from '../clients'

export const useGetWorkTypes = (includeDisabled: boolean = false) => {
  return useQuery({
    queryKey: [QK.WORK_TYPES, includeDisabled],
    queryFn: async () => (await getWorkTypesClient()).getList(includeDisabled),
    staleTime: 60000,
  })
}
