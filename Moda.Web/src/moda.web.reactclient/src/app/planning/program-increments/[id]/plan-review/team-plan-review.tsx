import { getProgramIncrementsClient } from '@/src/services/clients'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementTeamResponse,
} from '@/src/services/moda-api'
import { Col, Row, Segmented, Space, Typography } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import TeamObjectivesListCard from './team-objectives-list-card'
import TeamRisksListCard from './team-risks-list-card'
import Link from 'next/link'
import { BarsOutlined, BuildOutlined } from '@ant-design/icons'
import { SegmentedLabeledOption } from 'antd/es/segmented'
import { ProgramIncrementObjectivesTimeline } from '../../../components'

export interface TeamPlanReviewProps {
  programIncrement: ProgramIncrementDetailsDto
  team: ProgramIncrementTeamResponse
}

const TeamPlanReview = ({ programIncrement, team }: TeamPlanReviewProps) => {
  const [currentView, setCurrentView] = useState<string | number>('List')

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

  const viewSelectorOptions: SegmentedLabeledOption[] = [
    {
      value: 'List',
      icon: <BarsOutlined alt="List" title="List" />,
    },
    {
      value: 'Timeline',
      icon: <BuildOutlined alt="Timeline" title="Timeline" />,
    },
  ]

  const ListView = () => {
    return (
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={24} md={24} lg={12}>
          <TeamObjectivesListCard
            getObjectives={getObjectives}
            teamId={team?.id}
            programIncrementId={programIncrement?.id}
            newObjectivesAllowed={!programIncrement?.objectivesLocked ?? false}
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
    )
  }

  const TimelineView = () => {
    return (
      <ProgramIncrementObjectivesTimeline
        getObjectives={getObjectives}
        programIncrement={programIncrement}
        teamId={team?.id}
      />
    )
  }

  return (
    <>
      <Space
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          paddingBottom: '16px',
        }}
      >
        <Typography.Title level={3} style={{ margin: '0' }}>
          <Link href={`/organizations/teams/${team?.localId}`}>
            {team?.name}
          </Link>
        </Typography.Title>
        <Segmented
          options={viewSelectorOptions}
          value={currentView}
          onChange={setCurrentView}
        />
      </Space>
      {currentView === 'List' && <ListView />}
      {currentView === 'Timeline' && <TimelineView />}
    </>
  )
}

export default TeamPlanReview
