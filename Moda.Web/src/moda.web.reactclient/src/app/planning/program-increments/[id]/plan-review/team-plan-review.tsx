import { getProgramIncrementsClient } from '@/src/services/clients'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementObjectiveListDto,
  ProgramIncrementTeamResponse,
  RiskListDto,
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
  const [objectives, setObjectives] = useState<
    ProgramIncrementObjectiveListDto[]
  >([])
  const [risks, setRisks] = useState<RiskListDto[]>([])

  const getObjectives = useCallback(
    async (programIncrementId: string, teamId: string) => {
      const programIncrementsClient = await getProgramIncrementsClient()
      return await programIncrementsClient.getObjectives(
        programIncrementId,
        teamId
      )
    },
    []
  )

  const getRisks = useCallback(
    async (
      programIncrementId: string,
      teamId: string,
      includeClosed = false
    ) => {
      const programIncrementsClient = await getProgramIncrementsClient()
      return await programIncrementsClient.getRisks(
        programIncrementId,
        teamId,
        includeClosed
      )
    },
    []
  )

  useEffect(() => {
    const loadData = async () => {
      if (programIncrement && team) {
        setObjectives(await getObjectives(programIncrement.id, team.id))
        setRisks(await getRisks(programIncrement.id, team.id))
      }
    }
    loadData()
  }, [getObjectives, getRisks, programIncrement, team])

  return (
    <>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={24} md={24} lg={12}>
          <TeamObjectivesListCard
            objectives={objectives}
            teamId={team?.id}
            programIncrementId={programIncrement?.id}
            newObjectivesAllowed={true}
          />
        </Col>
        <Col xs={24} sm={24} md={24} lg={12}>
          <TeamRisksListCard
            risks={risks}
            programIncrementId={programIncrement?.id}
            teamId={team?.id}
          />
        </Col>
      </Row>
    </>
  )
}

export default TeamPlanReview
