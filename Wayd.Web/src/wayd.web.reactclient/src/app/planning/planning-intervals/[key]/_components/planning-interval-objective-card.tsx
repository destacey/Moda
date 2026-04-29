'use client'

import { PlanningIntervalObjectiveListDto } from '@/src/services/wayd-api'
import {
  Button,
  Card,
  Dropdown,
  Flex,
  MenuProps,
  Progress,
  Space,
  Tag,
  Typography,
} from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { CSSProperties, HTMLAttributes, ReactNode, useState } from 'react'
import { MoreOutlined } from '@ant-design/icons'
import HealthCheckTag from '@/src/components/common/health-check/health-check-tag'
import WaydTooltip from '@/src/components/common/wayd-tooltip'
import { ItemType } from 'antd/es/menu/interface'
import CreateHealthCheckForm from '@/src/components/common/health-check/create-health-check-form'
import { useAppDispatch, useAppSelector } from '@/src/hooks'
import { beginHealthCheckCreate } from '@/src/store/features/health-check-slice'
import { EditPlanningIntervalObjectiveForm } from '../../_components'
import { getObjectiveStatusColor } from '@/src/utils'

const { Text } = Typography

export interface PlanningIntervalObjectiveCardProps {
  objective: PlanningIntervalObjectiveListDto
  piKey: number
  canUpdateObjectives: boolean
  canCreateHealthChecks: boolean
  refreshObjectives: () => void
  onObjectiveClick: (objectiveKey: number) => void
  leading?: ReactNode
  cardRef?: (node: HTMLDivElement | null) => void
  cardAttributes?: HTMLAttributes<HTMLDivElement>
  cardStyle?: CSSProperties
  /**
   * Shows the objective's team code as the first metadata tag.
   * Defaults to true.
   */
  showTeamTag?: boolean
}

const PlanningIntervalObjectiveCard = ({
  objective,
  piKey,
  canUpdateObjectives,
  canCreateHealthChecks,
  refreshObjectives,
  onObjectiveClick,
  leading,
  cardRef,
  cardAttributes,
  cardStyle,
  showTeamTag = true,
}: PlanningIntervalObjectiveCardProps) => {
  const [openUpdateObjectiveForm, setOpenUpdateObjectiveForm] =
    useState<boolean>(false)
  const showMenu = canUpdateObjectives || canCreateHealthChecks

  const dispatch = useAppDispatch()
  const editingObjectiveId = useAppSelector(
    (state) => state.healthCheck.createContext.objectiveId,
  )

  const title = () => {
    return (
      <Link
        onClick={(e) => e.stopPropagation()}
        href={`/planning/planning-intervals/${piKey}/objectives/${objective.key}`}
      >
        {objective.key} - {objective.name}
      </Link>
    )
  }

  const description = () => {
    const startDate = objective.startDate
      ? `Start: ${
          objective.startDate
            ? dayjs(objective.startDate)?.format('M/D/YYYY')
            : ''
        }`
      : null
    const targetDate = objective.targetDate
      ? `Target: ${
          objective.targetDate
            ? dayjs(objective.targetDate)?.format('M/D/YYYY')
            : ''
        }`
      : null
    const showProgress = objective.status?.name !== 'Not Started'
    const progressStatus = ['Canceled', 'Missed'].includes(
      objective.status?.name,
    )
      ? 'exception'
      : undefined

    return (
      <>
        <Space wrap>
          {showTeamTag && objective.team?.code && (
            <WaydTooltip title={`Team - ${objective.team?.name ?? objective.team.code}`}>
              <Tag>{objective.team.code}</Tag>
            </WaydTooltip>
          )}
          <Tag color={getObjectiveStatusColor(objective.status.name)}>
            {objective.status.name}
          </Tag>
          {objective.isStretch && <Tag>Stretch</Tag>}
          <HealthCheckTag
            healthCheck={objective?.healthCheck}
            planningIntervalId={objective?.planningInterval?.id}
            objectiveId={objective?.id}
          />
          <Text>
            {startDate}
            {startDate && targetDate && ' - '}
            {targetDate}
          </Text>
        </Space>
        {showProgress && (
          <Progress
            percent={objective.progress}
            status={progressStatus}
            size="small"
          />
        )}
      </>
    )
  }

  const menuItems: MenuProps['items'] = (() => {
    const items: ItemType[] = []
    if (canUpdateObjectives || canCreateHealthChecks) {
      items.push(
        {
          key: 'edit',
          label: 'Edit',
          disabled: !canUpdateObjectives,
          onClick: (info) => {
            info.domEvent.stopPropagation()
            setOpenUpdateObjectiveForm(true)
          },
        },
        {
          key: 'createHealthCheck',
          label: 'Create Health Check',
          disabled: !canCreateHealthChecks,
          onClick: (info) => {
            info.domEvent.stopPropagation()
            dispatch(
              beginHealthCheckCreate({
                planningIntervalId: objective.planningInterval.id,
                objectiveId: objective.id,
              }),
            )
          },
        },
        {
          key: 'healthReport',
          onClick: (info) => info.domEvent.stopPropagation(),
          label: (
            <Link
              href={`/planning/planning-intervals/${objective.planningInterval.key}/objectives/${objective.key}/health-report`}
            >
              Health Report
            </Link>
          ),
        },
      )
    }
    return items
  })()

  const onEditObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenUpdateObjectiveForm(false)
    if (wasSaved) {
      refreshObjectives()
    }
  }

  const onCreateHealthCheckFormClosed = (wasSaved: boolean) => {
    if (wasSaved) {
      refreshObjectives()
    }
  }

  return (
    <>
      <Card
        size="small"
        ref={cardRef}
        {...cardAttributes}
        style={cardStyle}
        hoverable
        onClick={() => onObjectiveClick(objective.key)}
        styles={{ body: { padding: '12px 16px' } }}
      >
        <Flex align="flex-start">
          {leading && (
            <div style={{ marginRight: 12, paddingTop: 4 }}>{leading}</div>
          )}
          <div style={{ flex: 1, minWidth: 0 }}>
            <Flex justify="space-between" align="flex-start" gap={8}>
              <div style={{ minWidth: 0, overflowWrap: 'anywhere' }}>
                {title()}
              </div>
              {showMenu && (
                <Dropdown menu={{ items: menuItems }} trigger={['click']}>
                  <Button
                    type="text"
                    size="small"
                    icon={<MoreOutlined />}
                    onClick={(e) => e.stopPropagation()}
                  />
                </Dropdown>
              )}
            </Flex>
            <div style={{ marginTop: 8 }}>{description()}</div>
          </div>
        </Flex>
      </Card>
      {openUpdateObjectiveForm && (
        <EditPlanningIntervalObjectiveForm
          objectiveKey={objective?.key}
          planningIntervalKey={objective?.planningInterval?.key}
          onFormSave={() => onEditObjectiveFormClosed(true)}
          onFormCancel={() => onEditObjectiveFormClosed(false)}
        />
      )}
      {editingObjectiveId == objective?.id && (
        <CreateHealthCheckForm onClose={onCreateHealthCheckFormClosed} />
      )}
    </>
  )
}

export default PlanningIntervalObjectiveCard
