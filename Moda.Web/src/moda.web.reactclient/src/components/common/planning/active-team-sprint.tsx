'use client'

import { useGetActiveSprintQuery } from '@/src/store/features/organizations/team-api'
import { FC } from 'react'

export interface ActiveTeamSprintProps {
  teamId: string
}

const ActiveTeamSprint: FC<ActiveTeamSprintProps> = ({ teamId }) => {
  const { data: sprintData, isLoading } = useGetActiveSprintQuery(teamId)

  if (!sprintData) {
    return null
  }

  return (
    <div>
      <h2>Active Sprint for Team {teamId}</h2>
    </div>
  )
}

export default ActiveTeamSprint
