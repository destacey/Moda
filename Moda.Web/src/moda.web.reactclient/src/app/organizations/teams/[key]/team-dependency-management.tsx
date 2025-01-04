'use client'

import TeamDependenciesGrid from '@/src/components/common/organizations/team-dependencies-grid'
import { TeamDetailsDto } from '@/src/services/moda-api'
import { useGetTeamDependenciesQuery } from '@/src/store/features/organizations/team-api'
import { useEffect } from 'react'

export interface TeamDependencyManagementProps {
  team: TeamDetailsDto
}

const TeamDependencyManagement: React.FC<TeamDependencyManagementProps> = (
  props,
) => {
  const {
    data: dependencyData,
    isLoading,
    error,
    refetch,
  } = useGetTeamDependenciesQuery(props.team?.id, { skip: !props.team?.id })

  useEffect(() => {
    error && console.error(error)
  }, [error])

  return (
    <TeamDependenciesGrid
      team={props.team}
      dependencies={dependencyData}
      isLoading={isLoading}
      refetch={refetch}
    />
  )
}

export default TeamDependencyManagement
