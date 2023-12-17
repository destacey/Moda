import { useMutation, useQuery, useQueryClient } from 'react-query'
import { getPlanningIntervalsClient, getRisksClient } from '../clients'
import { QK } from './query-keys'
import {
  CreatePlanningIntervalObjectiveRequest,
  CreatePlanningIntervalRequest,
  CreateRiskRequest,
  UpdatePlanningIntervalObjectiveRequest,
  UpdatePlanningIntervalRequest,
  UpdateRiskRequest,
} from '../moda-api'
import _ from 'lodash'
import { OptionModel } from '@/src/app/components/types'
import dayjs from 'dayjs'

// PROGRAM INCREMENTS

const stateOrder = ['Active', 'Future', 'Completed']
export const useGetPlanningIntervals = () => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENTS],
    queryFn: async () => (await getPlanningIntervalsClient()).getList(),
    select: (data) =>
      data?.sort((a, b) => {
        const aStateIndex = stateOrder.indexOf(a.state)
        const bStateIndex = stateOrder.indexOf(b.state)
        if (aStateIndex !== bStateIndex) {
          return aStateIndex - bStateIndex
        } else {
          return dayjs(b.start).unix() - dayjs(a.start).unix()
        }
      }),
    staleTime: 60000,
  })
}

export const useGetPlanningIntervalById = (id: string) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENTS, id],
    queryFn: async () => (await getPlanningIntervalsClient()).getById(id),
    // staleTime: 60000,
    enabled: !!id,
  })
}

export const useGetPlanningIntervalByKey = (key: number) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENTS, key],
    queryFn: async () => (await getPlanningIntervalsClient()).getByKey(key),
    // staleTime: 60000,
    enabled: !!key,
  })
}

export const useGetPlanningIntervalPredictability = (id: string) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_PREDICTABILITY, id],
    queryFn: async () =>
      (await getPlanningIntervalsClient()).getPredictability(id),
    enabled: !!id,
  })
}

export const useCreatePlanningIntervalMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (planningInterval: CreatePlanningIntervalRequest) =>
      (await getPlanningIntervalsClient()).create(planningInterval),
    onSuccess: (data) => {
      queryClient.invalidateQueries(QK.PROGRAM_INCREMENTS)
    },
  })
}

export const useUpdatePlanningIntervalMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (planningInterval: UpdatePlanningIntervalRequest) =>
      (await getPlanningIntervalsClient()).update(
        planningInterval.id,
        planningInterval,
      ),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENTS])
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENTS, variables.id])
    },
  })
}

// PROGRAM INCREMENT - TEAMS
export const useGetPlanningIntervalTeams = (
  id: string,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_TEAMS, id],
    queryFn: async () => (await getPlanningIntervalsClient()).getTeams(id),
    // staleTime: 60000,
    enabled: !!id && enabled,
  })
}

// PROGRAM INCREMENT - OBJECTIVES
export const useGetPlanningIntervalObjectives = (
  id: string,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVES, id],
    queryFn: async () =>
      (await getPlanningIntervalsClient()).getObjectives(id, null),
    // staleTime: 10000,
    enabled: !!id && enabled,
  })
}

export const useGetPlanningIntervalObjectivesByTeamId = (
  id: string,
  teamId: string,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVES, id, teamId],
    queryFn: async () =>
      (await getPlanningIntervalsClient()).getObjectives(id, teamId),
    // staleTime: 20000,
    enabled: !!id && !!teamId && enabled,
  })
}

export const useGetPlanningIntervalObjectivesHealthReport = (
  idOrKey: string,
  teamId?: string,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVES_HEALTH_REPORT, idOrKey, teamId],
    queryFn: async () =>
      (await getPlanningIntervalsClient()).getObjectivesHealthReport(
        idOrKey,
        teamId,
      ),
    // staleTime: 20000,
    enabled: !!idOrKey && enabled,
  })
}

export const useGetPlanningIntervalObjectiveById = (
  id: string,
  objectiveId: string,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVES, id, objectiveId],
    queryFn: async () =>
      (await getPlanningIntervalsClient()).getObjectiveById(id, objectiveId),
    onError: (error) => {
      console.log(error)
    },
    // staleTime: 10000,
    enabled: !!id && !!objectiveId,
  })
}

export const useGetPlanningIntervalObjectiveByKey = (
  key: number,
  objectiveKey: number,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVES, key, objectiveKey],
    queryFn: async () =>
      (await getPlanningIntervalsClient()).getObjectiveByKey(key, objectiveKey),
    // staleTime: 10000,
    enabled: !!key && !!objectiveKey,
  })
}

export const useGetPlanningIntervalObjectiveStatuses = (
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVE_STATUSES],
    queryFn: async () =>
      (await getPlanningIntervalsClient()).getObjectiveStatuses(),
    // staleTime: 300000,
    enabled: enabled,
  })
}

export const useGetPlanningIntervalObjectiveStatusOptions = () => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_OBJECTIVE_STATUS_OPTIONS],
    queryFn: async () =>
      (await getPlanningIntervalsClient()).getObjectiveStatuses(),
    select: (data) => {
      const statuses = _.sortBy(data, ['order'])
      const options: OptionModel<number>[] = statuses.map((s) => ({
        value: s.id,
        label: s.name,
      }))
      return options
    },
    // staleTime: 300000,
  })
}

export const useGetTeamPlanningIntervalPredictability = (
  id: string,
  teamId: string,
) => {
  return useQuery({
    queryKey: [QK.PROGRAM_INCREMENT_TEAM_PREDICTABILITY, id, teamId],
    queryFn: async () =>
      (await getPlanningIntervalsClient()).getTeamPredictability(id, teamId),
    //staleTime: 30000,
    enabled: !!id && !!teamId,
  })
}

export interface CreatePlanningIntervalObjectiveMutationRequest {
  objective: CreatePlanningIntervalObjectiveRequest
  planningIntervalKey: number
}

export const useCreatePlanningIntervalObjectiveMutation = () => {
  const queryClient = useQueryClient()
  return useMutation(
    async ({
      objective,
      planningIntervalKey,
    }: CreatePlanningIntervalObjectiveMutationRequest) =>
      (await getPlanningIntervalsClient()).createObjective(
        objective.planningIntervalId,
        objective,
      ),
    {
      onSuccess: (data, variables) => {
        queryClient.invalidateQueries([QK.PROGRAM_INCREMENT_OBJECTIVES])
        queryClient.invalidateQueries([
          QK.PROGRAM_INCREMENT_TEAM_PREDICTABILITY,
          variables.objective.planningIntervalId,
        ])
        queryClient.invalidateQueries([
          QK.PROGRAM_INCREMENTS,
          variables.objective.planningIntervalId,
        ])
        queryClient.invalidateQueries([
          QK.PROGRAM_INCREMENTS,
          variables.planningIntervalKey,
        ])
      },
    },
  )
}

export interface UpdatePlanningIntervalObjectiveMutationRequest {
  objective: UpdatePlanningIntervalObjectiveRequest
  planningIntervalKey: number
}

export const useUpdatePlanningIntervalObjectiveMutation = () => {
  const queryClient = useQueryClient()
  return useMutation(
    async ({
      objective,
      planningIntervalKey,
    }: UpdatePlanningIntervalObjectiveMutationRequest) =>
      (await getPlanningIntervalsClient()).updateObjective(
        objective.planningIntervalId,
        objective.objectiveId,
        objective,
      ),
    {
      onSuccess: (data, variables) => {
        queryClient.invalidateQueries([QK.PROGRAM_INCREMENT_OBJECTIVES])
        queryClient.invalidateQueries([
          QK.PROGRAM_INCREMENT_OBJECTIVES,
          variables.objective.planningIntervalId,
        ])
        queryClient.invalidateQueries([
          QK.PROGRAM_INCREMENT_OBJECTIVES,
          variables.planningIntervalKey,
        ])
        queryClient.invalidateQueries([
          QK.PROGRAM_INCREMENT_OBJECTIVES,
          variables.objective.planningIntervalId,
          variables.objective.objectiveId,
        ])
        queryClient.invalidateQueries([
          QK.PROGRAM_INCREMENT_TEAM_PREDICTABILITY,
          variables.objective.planningIntervalId,
        ])
        queryClient.invalidateQueries([
          QK.PROGRAM_INCREMENTS,
          variables.objective.planningIntervalId,
        ])
        queryClient.invalidateQueries([
          QK.PROGRAM_INCREMENTS,
          variables.planningIntervalKey,
        ])
      },
    },
  )
}

// PROGRAM INCREMENT - RISKS
export const useGetPlanningIntervalRisks = (
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
      (await getPlanningIntervalsClient()).getRisks(id, null, includeClosed),
    // staleTime: 10000,
    enabled: !!id && enabled,
  })
}

export const useGetPlanningIntervalRisksByTeamId = (
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
      (await getPlanningIntervalsClient()).getRisks(id, teamId, includeClosed),
    // staleTime: 20000,
    enabled: !!id && !!teamId && enabled,
  })
}

// RISKS
export const useGetRiskById = (id: string) => {
  return useQuery({
    queryKey: [QK.RISKS, id],
    queryFn: async () => (await getRisksClient()).getById(id),
    // staleTime: 10000,
    enabled: !!id,
  })
}

export const useGetRiskByKey = (key: number) => {
  return useQuery({
    queryKey: [QK.RISKS, key],
    queryFn: async () => (await getRisksClient()).getByKey(key),
    // staleTime: 10000,
    enabled: !!key,
  })
}

export const useGetMyRisks = () => {
  return useQuery({
    queryKey: [QK.MY_RISKS],
    queryFn: async () => (await getRisksClient()).getMyRisks(),
    // staleTime: 10000,
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
    // staleTime: 300000,
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
    // staleTime: 300000,
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
    // staleTime: 300000,
  })
}

export const useCreateRiskMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (risk: CreateRiskRequest) =>
      (await getRisksClient()).createRisk(risk),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENT_RISKS])
      queryClient.invalidateQueries([QK.TEAM_RISKS])
      queryClient.invalidateQueries([QK.TEAM_RISKS, variables.teamId])
      queryClient.invalidateQueries([QK.TEAM_OF_TEAMS_RISKS])
      queryClient.invalidateQueries([QK.TEAM_OF_TEAMS_RISKS, variables.teamId])
      queryClient.invalidateQueries([QK.MY_RISKS])
    },
  })
}

export const useUpdateRiskMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (risk: UpdateRiskRequest) =>
      (await getRisksClient()).update(risk.riskId, risk),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.RISKS, variables.riskId])
      queryClient.invalidateQueries([QK.PROGRAM_INCREMENT_RISKS])
      queryClient.invalidateQueries([QK.TEAM_RISKS])
      queryClient.invalidateQueries([QK.TEAM_RISKS, variables.teamId])
      queryClient.invalidateQueries([QK.TEAM_OF_TEAMS_RISKS])
      queryClient.invalidateQueries([QK.TEAM_OF_TEAMS_RISKS, variables.teamId])
      queryClient.invalidateQueries([QK.MY_RISKS])
    },
  })
}
