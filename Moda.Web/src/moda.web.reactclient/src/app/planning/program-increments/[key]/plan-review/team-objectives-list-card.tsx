'use client'

import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Badge, Button, Card, List, Space, message } from 'antd'
import ObjectiveListItem from './objective-list-item'
import ModaEmpty from '@/src/app/components/common/moda-empty'
import { useCallback, useState } from 'react'
import useAuth from '@/src/app/components/contexts/auth'
import CreateProgramIncrementObjectiveForm from '../create-program-increment-objective-form'
import dayjs from 'dayjs'
import { UseQueryResult } from 'react-query'
import useTheme from '@/src/app/components/contexts/theme'

export interface TeamObjectivesListCardProps {
  objectivesQuery: UseQueryResult<ProgramIncrementObjectiveListDto[], unknown>
  programIncrementId: string
  teamId: string
  newObjectivesAllowed?: boolean
  refreshProgramIncrement: () => void
}

const statusOrder = ['Not Started', 'In Progress', 'Closed', 'Canceled']

const TeamObjectivesListCard = ({
  objectivesQuery,
  programIncrementId,
  teamId,
  newObjectivesAllowed = false,
  refreshProgramIncrement,
}: TeamObjectivesListCardProps) => {
  const [openCreateObjectiveForm, setOpenCreateObjectiveForm] =
    useState<boolean>(false)
  const { badgeColor } = useTheme()

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.ProgramIncrementObjectives.Manage',
  )
  const canCreateObjectives =
    newObjectivesAllowed && programIncrementId && canManageObjectives

  const refreshObjectives = useCallback(() => {
    objectivesQuery.refetch()
    // this will update the PI predictability on the plan review page title
    refreshProgramIncrement()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const CardTitle = () => {
    const count = objectivesQuery?.data?.length ?? 0
    const showBadge = count > 0
    return (
      <Space>
        {'Objectives'}
        {showBadge && <Badge color={badgeColor} size="small" count={count} />}
      </Space>
    )
  }

  const ObjectivesList = () => {
    if (!objectivesQuery?.data || objectivesQuery?.data.length === 0) {
      return <ModaEmpty message="No objectives" />
    }

    const sortedObjectives = objectivesQuery?.data.sort((a, b) => {
      if (a.isStretch && !b.isStretch) {
        return 1
      } else if (!a.isStretch && b.isStretch) {
        return -1
      } else {
        const aStatusIndex = statusOrder.indexOf(a.status.name)
        const bStatusIndex = statusOrder.indexOf(b.status.name)
        if (aStatusIndex === bStatusIndex) {
          if (a.targetDate && b.targetDate) {
            const targetDateDiff = dayjs(a.targetDate).diff(dayjs(b.targetDate))
            if (targetDateDiff !== 0) {
              return targetDateDiff
            }
          } else if (a.targetDate) {
            return -1
          } else if (b.targetDate) {
            return 1
          }
          return a.key - b.key
        } else {
          return aStatusIndex - bStatusIndex
        }
      }
    })

    return (
      <List
        size="small"
        dataSource={sortedObjectives}
        renderItem={(objective) => (
          <ObjectiveListItem
            objective={objective}
            piKey={objective.programIncrement.key}
            canUpdateObjectives={canManageObjectives}
            refreshObjectives={refreshObjectives}
          />
        )}
      />
    )
  }

  const onCreateObjectiveFormClosed = (wasCreated: boolean) => {
    setOpenCreateObjectiveForm(false)
    if (wasCreated) {
      refreshObjectives()
    }
  }

  return (
    <>
      <Card
        size="small"
        title={<CardTitle />}
        extra={
          canCreateObjectives && (
            <Button
              type="text"
              icon={<PlusOutlined />}
              onClick={() => setOpenCreateObjectiveForm(true)}
            />
          )
        }
      >
        <ObjectivesList />
      </Card>
      {openCreateObjectiveForm && (
        <CreateProgramIncrementObjectiveForm
          programIncrementId={programIncrementId}
          teamId={teamId}
          showForm={openCreateObjectiveForm}
          onFormCreate={() => onCreateObjectiveFormClosed(true)}
          onFormCancel={() => onCreateObjectiveFormClosed(false)}
        />
      )}
    </>
  )
}

export default TeamObjectivesListCard
