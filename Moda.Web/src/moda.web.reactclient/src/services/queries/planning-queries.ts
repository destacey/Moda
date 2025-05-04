import { useMutation, useQuery, useQueryClient } from 'react-query'
import { getPlanningIntervalsClient, getRisksClient } from '../clients'
import { QK } from './query-keys'
import { CreateRiskRequest, UpdateRiskRequest } from '../moda-api'
import _ from 'lodash'
import { OptionModel } from '@/src/components/types'

// PLANNING INTERVAL - OBJECTIVES
export const useGetPlanningIntervalObjectivesHealthReport = (
  idOrKey: string,
  teamId?: string,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [QK.PLANNING_INTERVAL_OBJECTIVES_HEALTH_REPORT, idOrKey, teamId],
    queryFn: async () =>
      getPlanningIntervalsClient().getObjectivesHealthReport(idOrKey, teamId),
    // staleTime: 20000,
    enabled: !!idOrKey && enabled,
  })
}

// PLANNING INTERVAL - RISKS
export const useGetPlanningIntervalRisks = (
  id: string,
  includeClosed: boolean = false,
  enabled: boolean = true,
) => {
  return useQuery({
    queryKey: [
      QK.PLANNING_INTERVAL_RISKS,
      id,
      { includeClosed: includeClosed },
    ],
    queryFn: async () =>
      getPlanningIntervalsClient().getRisks(id, null, includeClosed),
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
      QK.PLANNING_INTERVAL_RISKS,
      id,
      teamId,
      { includeClosed: includeClosed },
    ],
    queryFn: async () =>
      getPlanningIntervalsClient().getRisks(id, teamId, includeClosed),
    // staleTime: 20000,
    enabled: !!id && !!teamId && enabled,
  })
}

// RISKS
export const useGetRisk = (idOrKey: string) => {
  return useQuery({
    queryKey: [QK.RISKS, idOrKey],
    queryFn: async () => getRisksClient().getRisk(idOrKey),
    // staleTime: 10000,
    enabled: !!idOrKey,
  })
}

export const useGetMyRisks = () => {
  return useQuery({
    queryKey: [QK.MY_RISKS],
    queryFn: async () => getRisksClient().getMyRisks(),
    // staleTime: 10000,
  })
}

export const useGetRiskStatusOptions = () => {
  return useQuery({
    queryKey: [QK.RISK_STATUS_OPTIONS],
    queryFn: async () => getRisksClient().getStatuses(),
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
    queryFn: async () => getRisksClient().getCategories(),
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
    queryFn: async () => getRisksClient().getGrades(),
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
      getRisksClient().createRisk(risk),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.PLANNING_INTERVAL_RISKS])
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
      getRisksClient().update(risk.riskId, risk),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.RISKS, variables.riskId])
      queryClient.invalidateQueries([QK.PLANNING_INTERVAL_RISKS])
      queryClient.invalidateQueries([QK.TEAM_RISKS])
      queryClient.invalidateQueries([QK.TEAM_RISKS, variables.teamId])
      queryClient.invalidateQueries([QK.TEAM_OF_TEAMS_RISKS])
      queryClient.invalidateQueries([QK.TEAM_OF_TEAMS_RISKS, variables.teamId])
      queryClient.invalidateQueries([QK.MY_RISKS])
    },
  })
}
