import { useMutation, useQuery, useQueryClient } from 'react-query'
import { QK } from './query-keys'
import { getLinksClient } from '../clients'
import { CreateLinkRequest, UpdateLinkRequest } from '../moda-api'

// LINKS
export const useGetLinks = (objectId: string) => {
  return useQuery({
    queryKey: [QK.LINKS, objectId],
    queryFn: async () => (await getLinksClient()).getList(objectId),
    staleTime: 10000,
    enabled: !!objectId,
  })
}

export const useGetLinkById = (id: string) => {
  return useQuery({
    queryKey: [QK.LINKS, id],
    queryFn: async () => (await getLinksClient()).getById(id),
    staleTime: 10000,
    enabled: !!id,
  })
}

export const useCreateLinkMutation = () => {
  const queryClient = useQueryClient()
  return useMutation(
    async (link: CreateLinkRequest) => (await getLinksClient()).create(link),
    {
      onSuccess: (data, variables) => {
        queryClient.invalidateQueries([QK.LINKS, variables.objectId])
      },
    },
  )
}

export const useUpdateLinkMutation = () => {
  const queryClient = useQueryClient()
  return useMutation(
    async (link: UpdateLinkRequest) =>
      (await getLinksClient()).update(link.id, link),
    {
      onSuccess: (data) => {
        queryClient.invalidateQueries([QK.LINKS, data.id])
        queryClient.invalidateQueries([QK.LINKS, data.objectId])
      },
    },
  )
}

interface DeleteLinkMutationRequest {
  id: string
  objectId: string
}

export const useDeleteLinkMutation = () => {
  const queryClient = useQueryClient()
  return useMutation(
    async ({ id, objectId }: DeleteLinkMutationRequest) =>
      (await getLinksClient()).delete(id),
    {
      onSuccess: (data, variables) => {
        queryClient.invalidateQueries([QK.LINKS, variables.id])
        queryClient.invalidateQueries([QK.LINKS, variables.objectId])
      },
    },
  )
}
