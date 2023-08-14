import { useMutation, useQuery, useQueryClient } from 'react-query'
import { getProgramIncrementsClient } from '../clients'
import { QK } from './query-keys'
import {
  CreateProgramIncrementObjectiveRequest,
  CreateProgramIncrementRequest,
  UpdateProgramIncrementObjectiveRequest,
  UpdateProgramIncrementRequest,
} from '../moda-api'
import _ from 'lodash'
import { OptionModel } from '@/src/app/components/types'

// PROGRAM INCREMENTS

export const useGetProgramIncrements = () => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENTS],
    queryFn: async () => (await getProgramIncrementsClient()).getList(),
    staleTime: 60000,
  })
}

export const useGetProgramIncrementById = (id: string) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENTS, id],
    queryFn: async () => (await getProgramIncrementsClient()).getById(id),
    staleTime: 60000,
    enabled: !!id,
  })
}

export const useGetProgramIncrementByLocalId = (localId: number) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENTS, localId],
    queryFn: async () =>
      (await getProgramIncrementsClient()).getByLocalId(localId),
    staleTime: 60000,
    enabled: !!localId,
  })
}

export const useCreateProgramIncrementMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (programIncrement: CreateProgramIncrementRequest) =>
      (await getProgramIncrementsClient()).create(programIncrement),
    onSuccess: (data) => {
      queryClient.invalidateQueries(QK.PROGRAM_INCREMENTS)
    },
  })
}

export const useUpdateProgramIncrementMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (programIncrement: UpdateProgramIncrementRequest) =>
      (await getProgramIncrementsClient()).update(
        programIncrement.id,
        programIncrement,
      ),
    onSuccess: (data) => {
      queryClient.invalidateQueries(QK.PROGRAM_INCREMENTS)
    },
  })
}

// PROGRAM INCREMENT - TEAMS
export const useGetProgramIncrementTeams = (
  id: string,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_TEAMS, id],
    queryFn: async () => (await getProgramIncrementsClient()).getTeams(id),
    staleTime: 60000,
    enabled: !!id && enabled,
  })
}

// PROGRAM INCREMENT - OBJECTIVES
export const useGetProgramIncrementObjectives = (
  id: string,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVES, id],
    queryFn: async () =>
      (await getProgramIncrementsClient()).getObjectives(id, null),
    staleTime: 10000,
    enabled: !!id && enabled,
  })
}

export const useGetProgramIncrementObjectivesByTeamId = (
  id: string,
  teamId: string,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVES, id, teamId],
    queryFn: async () =>
      (await getProgramIncrementsClient()).getObjectives(id, teamId),
    staleTime: 10000,
    enabled: !!id && !!teamId && enabled,
  })
}

export const useGetProgramIncrementObjectiveById = (
  id: string,
  objectiveId: string,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVES, id, objectiveId],
    queryFn: async () =>
      (await getProgramIncrementsClient()).getObjectiveById(id, objectiveId),
    onError: (error) => {
      console.log(error)
    },
    staleTime: 10000,
    enabled: !!id && !!objectiveId,
  })
}

export const useGetProgramIncrementObjectiveByLocalId = (
  id: number,
  objectiveId: number,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVES, id, objectiveId],
    queryFn: async () =>
      (await getProgramIncrementsClient()).getObjectiveByLocalId(
        id,
        objectiveId,
      ),
    staleTime: 10000,
    enabled: !!id && !!objectiveId,
  })
}

export const useGetProgramIncrementObjectiveStatuses = (
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVE_STATUSES],
    queryFn: async () =>
      (await getProgramIncrementsClient()).getObjectiveStatuses(),
    staleTime: 300000,
    enabled: enabled,
  })
}

export const useGetProgramIncrementObjectiveStatusOptions = () => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVE_STATUS_OPTIONS],
    queryFn: async () =>
      (await getProgramIncrementsClient()).getObjectiveStatuses(),
    select: (data) => {
      const statuses: OptionModel<number>[] = data.map((s) => ({
        value: s.id,
        label: s.name,
      }))
      return _.sortBy(statuses, ['order'])
    },
    staleTime: 300000,
  })
}

export const useCreateProgramIncrementObjectiveMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (objective: CreateProgramIncrementObjectiveRequest) =>
      (await getProgramIncrementsClient()).createObjective(
        objective.programIncrementId,
        objective,
      ),
    onSuccess: () => {
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENT_OBJECTIVES])
    },
  })
}

export const useUpdateProgramIncrementObjectiveMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (objective: UpdateProgramIncrementObjectiveRequest) =>
      (await getProgramIncrementsClient()).updateObjective(
        objective.programIncrementId,
        objective.objectiveId,
        objective,
      ),
    onSuccess: () => {
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENT_OBJECTIVES])
    },
  })
}

// PROGRAM INCREMENT - RISKS
export const useGetProgramIncrementRisks = (
  id: string,
  includeClosed: boolean = false,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [
      QK.PROGRAM_INCREMENT_RISKS,
      id,
      { includeClosed: includeClosed },
    ],
    queryFn: async () =>
      (await getProgramIncrementsClient()).getRisks(id, null, includeClosed),
    staleTime: 10000,
    enabled: !!id && enabled,
  })
}

export const useGetProgramIncrementRisksByTeamId = (
  id: string,
  teamId: string,
  includeClosed: boolean = false,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [
      QK.PROGRAM_INCREMENT_RISKS,
      id,
      teamId,
      { includeClosed: includeClosed },
    ],
    queryFn: async () =>
      (await getProgramIncrementsClient()).getRisks(id, teamId, includeClosed),
    staleTime: 10000,
    enabled: !!id && !!teamId && enabled,
  })
}
