import { getProgramIncrementsClient } from '@/src/services/clients'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementObjectiveListDto,
  ProgramIncrementTeamResponse,
  RiskListDto,
} from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Button, Card, Col, Row, Space } from 'antd'
import { useCallback, useEffect, useState } from 'react'

export interface ProgramIncrementTeamPlanReviewProps {
  programIncrement: ProgramIncrementDetailsDto
  team: ProgramIncrementTeamResponse
}

const ProgramIncrementTeamPlanReview = ({
  programIncrement,
  team,
}: ProgramIncrementTeamPlanReviewProps) => {
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

  // TODO: we don't want the 8px margin on the last card
  // TODO: background color for cards
  const ObjectiveCards = () => {
    return objectives?.map((objective) => {
      return (
        <Card key={objective.localId} style={{ marginBottom: '8px' }}>
          {objective.name}
        </Card>
      )
    })
  }

  // TODO: we don't want the 8px margin on the last card
  // TODO: background color for cards
  const RiskCards = () => {
    return risks?.map((risk) => {
      return (
        <Card key={risk.localId} style={{ marginBottom: '8px' }}>
          {risk.summary}
        </Card>
      )
    })
  }

  return (
    <>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={24} md={12}>
          <Card
            title="Objectives"
            extra={<Button type="text" icon={<PlusOutlined />} />}
          >
            <Space direction="vertical">
              <ObjectiveCards />
            </Space>
          </Card>
        </Col>
        <Col xs={24} sm={24} md={12}>
          <Card
            title="Risks"
            extra={<Button type="text" icon={<PlusOutlined />} />}
          >
            <Space direction="vertical">
              <RiskCards />
            </Space>
          </Card>
        </Col>
      </Row>
    </>
  )
}

export default ProgramIncrementTeamPlanReview
