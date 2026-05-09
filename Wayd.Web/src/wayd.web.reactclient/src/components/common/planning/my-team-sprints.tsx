'use client'

import useAuth from '@/src/components/contexts/auth'
import { useGetEmployeeTeamMembershipsQuery } from '@/src/store/features/organization/team-members-api'
import { SizingMethod } from '@/src/services/wayd-api'
import { Flex, Typography } from 'antd'
import ActiveTeamSprint from './active-team-sprint'

const { Title } = Typography

const MyTeamSprints = () => {
  const { user } = useAuth()
  const employeeId = user?.employeeId

  const { data: memberships } = useGetEmployeeTeamMembershipsQuery(
    { employeeId: employeeId! },
    { skip: !employeeId },
  )

  const teamMemberships =
    memberships
      ?.filter((m) => m.team.type === 'Team')
      .sort((a, b) => a.team.name.localeCompare(b.team.name)) ?? []

  if (!employeeId || teamMemberships.length === 0) return null

  return (
    <Flex vertical gap="middle">
      <Title level={5} style={{ margin: 0 }}>
        My Team Sprints
      </Title>
      {teamMemberships.map((m) => (
        <ActiveTeamSprint
          key={m.team.id}
          teamId={m.team.id}
          sizingMethod={SizingMethod.StoryPoints}
          showTeamLink
        />
      ))}
    </Flex>
  )
}

export default MyTeamSprints

