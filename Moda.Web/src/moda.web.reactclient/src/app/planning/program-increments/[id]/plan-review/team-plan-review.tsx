import { getProgramIncrementsClient } from '@/src/services/clients'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementTeamResponse,
} from '@/src/services/moda-api'
import { Col, Row } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import TeamObjectivesListCard from './team-objectives-list-card'
import TeamRisksListCard from './team-risks-list-card'

export interface TeamPlanReviewProps {
  programIncrement: ProgramIncrementDetailsDto
  team: ProgramIncrementTeamResponse
}

const TeamPlanReview = ({ programIncrement, team }: TeamPlanReviewProps) => {
  const getObjectives = useCallback(
    async (programIncrementId: string, teamId: string) => {
      if (programIncrementId && teamId) {
        const programIncrementsClient = await getProgramIncrementsClient()
        return await programIncrementsClient.getObjectives(
          programIncrementId,
          teamId
        )
      }
    },
    []
  )

  const getRisks = useCallback(
    async (programIncrementId: string, teamId: string) => {
      if (programIncrementId && teamId) {
        const programIncrementsClient = await getProgramIncrementsClient()
        return await programIncrementsClient.getRisks(
          programIncrementId,
          teamId,
          false
        )
      }
    },
    []
  )

  return (
    <>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={24} md={24} lg={12}>
          <TeamObjectivesListCard
            getObjectives={getObjectives}
            teamId={team?.id}
            programIncrementId={programIncrement?.id}
            newObjectivesAllowed={true}
          />
        </Col>
        <Col xs={24} sm={24} md={24} lg={12}>
          <TeamRisksListCard
            getRisks={getRisks}
            programIncrementId={programIncrement?.id}
            teamId={team?.id}
          />
        </Col>
      </Row>
    </>
  )
}

export default TeamPlanReview