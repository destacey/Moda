import { useMutation, useQuery, useQueryClient } from 'react-query'
import { getProgramIncrementsClient, getRisksClient } from '../clients'
import { QK } from './query-keys'
import {
  CreateProgramIncrementObjectiveRequest,
  CreateProgramIncrementRequest,
  CreateRiskRequest,
  UpdateProgramIncrementObjectiveRequest,
  UpdateProgramIncrementRequest,
  UpdateRiskRequest,
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
    onSuccess: (data, context) => {
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENTS])
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENTS, context.id])
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
      const statuses = _.sortBy(data, ['order'])
      const options: OptionModel<number>[] = statuses.map((s) => ({
        value: s.id,
        label: s.name,
      }))
      return options
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
    onSuccess: (data, context) => {
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENT_OBJECTIVES])
      queryClient.invalidateQueries([
        QK.PROGRAM_INCREMENT_OBJECTIVES,
        context.programIncrementId,
      ])
      queryClient.invalidateQueries([
        QK.PROGRAM_INCREMENT_OBJECTIVES,
        context.programIncrementId,
        context.objectiveId,
      ])
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

// RISKS
export const useGetRiskById = (id: string) => {
  return useQuery({
    queryKey: [QK.RISKS, id],
    queryFn: async () => (await getRisksClient()).getById(id),
    staleTime: 10000,
    enabled: !!id,
  })
}

export const useGetMyRisks = () => {
  return useQuery({
    queryKey: [QK.MY_RISKS],
    queryFn: async () => (await getRisksClient()).getMyRisks(),
    staleTime: 10000,
  })
}

export const useGetRiskStatusOptions = () => {
  return useQuery({
    queryKey: [QK.RISK_STATUS_OPTIONS],
    queryFn: async () => (await getRisksClient()).getStatuses(),
    select: (data) => {
      const statuses = _.sortBy(data, ['order'])
      const options: OptionModel<number>[] = statuses.map((c) => ({
        value: c.id,
        label: c.name,
      }))
      return options
    },
    staleTime: 300000,
  })
}

export const useGetRiskCategoryOptions = () => {
  return useQuery({
    queryKey: [QK.RISK_CATEGORY_OPTIONS],
    queryFn: async () => (await getRisksClient()).getCategories(),
    select: (data) => {
      const categories = _.sortBy(data, ['order'])
      const options: OptionModel<number>[] = categories.map((c) => ({
        value: c.id,
        label: c.name,
      }))
      return options
    },
    staleTime: 300000,
  })
}

export const useGetRiskGradeOptions = () => {
  return useQuery({
    queryKey: [QK.RISK_GRADE_OPTIONS],
    queryFn: async () => (await getRisksClient()).getGrades(),
    select: (data) => {
      const grades = _.sortBy(data, ['order'])
      const options: OptionModel<number>[] = grades.map((c) => ({
        value: c.id,
        label: c.name,
      }))
      return options
    },
    staleTime: 300000,
  })
}

export const useCreateRiskMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (risk: CreateRiskRequest) =>
      (await getRisksClient()).createRisk(risk),
    onSuccess: (data, context) => {
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENT_RISKS])
      queryClient.invalidateQueries([QK.TEAM_RISKS])
      queryClient.invalidateQueries([QK.TEAM_RISKS, context.teamId])
      queryClient.invalidateQueries([QK.TEAM_OF_TEAMS_RISKS])
      queryClient.invalidateQueries([QK.TEAM_OF_TEAMS_RISKS, context.teamId])
      queryClient.invalidateQueries([QK.MY_RISKS])
    },
  })
}

export const useUpdateRiskMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (risk: UpdateRiskRequest) =>
      (await getRisksClient()).update(risk.riskId, risk),
    onSuccess: (data, context) => {
      queryClient.invalidateQueries([QK.RISKS, context.riskId])
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENT_RISKS])
      queryClient.invalidateQueries([QK.TEAM_RISKS])
      queryClient.invalidateQueries([QK.TEAM_RISKS, context.teamId])
      queryClient.invalidateQueries([QK.TEAM_OF_TEAMS_RISKS])
      queryClient.invalidateQueries([QK.TEAM_OF_TEAMS_RISKS, context.teamId])
      queryClient.invalidateQueries([QK.MY_RISKS])
    },
  })
}