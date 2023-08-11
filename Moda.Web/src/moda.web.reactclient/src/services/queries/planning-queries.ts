import { useQuery } from 'react-query'
import { getProgramIncrementsClient } from '../clients'
import { QK } from './query-keys'

// PROGRAM INCREMENTS

export const useGetProgramIncrements = () => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENTS],
    queryFn: async () => (await getProgramIncrementsClient()).getList(),
  })
}

export const useGetProgramIncrementById = (id: string) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENTS, id],
    queryFn: async () => (await getProgramIncrementsClient()).getById(id),
    enabled: !!id,
  })
}

export const useGetProgramIncrementByLocalId = (localId: number) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENTS, localId],
    queryFn: async () =>
      (await getProgramIncrementsClient()).getByLocalId(localId),
    enabled: !!localId,
  })
}
