import { useMutation, useQuery, useQueryClient } from 'react-query'
import { QK } from './query-keys'
import { getHealthChecksClient } from '../clients'
import { CreateHealthCheckRequest, UpdateHealthCheckRequest } from '../moda-api'
import _ from 'lodash'
import { OptionModel } from '@/src/app/components/types'

// HEALTH CHECKS
export const useGetHealthCheckById = (id: string) => {
  return useQuery({
    queryKey: [QK.HEALTH_CHECKS, id],
    queryFn: async () => (await getHealthChecksClient()).getById(id),
    enabled: !!id,
  })
}

export const useCreateHealthCheckMutation = () => {
  return useMutation(
    async (healthCheck: CreateHealthCheckRequest) =>
      (await getHealthChecksClient()).create(healthCheck),
    {
      onSuccess: (data, variables) => {},
    },
  )
}

export const useUpdateHealthCheckMutation = () => {
  const queryClient = useQueryClient()
  return useMutation(
    async (healthCheck: UpdateHealthCheckRequest) =>
      (await getHealthChecksClient()).update(healthCheck.id, healthCheck),
    {
      onSuccess: (data) => {
        queryClient.invalidateQueries([QK.HEALTH_CHECKS, data.id])
      },
    },
  )
}

export const useGetHealthStatusOptions = () => {
  return useQuery({
    queryKey: [QK.HEALTH_STATUS_OPTIONS],
    queryFn: async () => (await getHealthChecksClient()).getStatuses(),
    select: (data) => {
      const statuses = _.sortBy(data, ['order'])
      return statuses.map((c) => ({
        value: c.id,
        label: c.name,
      })) as OptionModel<number>[]
    },
  })
}
