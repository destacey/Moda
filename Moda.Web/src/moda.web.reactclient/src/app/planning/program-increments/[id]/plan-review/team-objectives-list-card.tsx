'use client'

import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Badge, Button, Card, List, Space } from 'antd'
import ObjectiveListItem from './objective-list-item'
import ModaEmpty from '@/src/app/components/common/moda-empty'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '@/src/app/components/contexts/auth'
import CreateProgramIncrementObjectiveForm from '../create-program-increment-objective-form'
import dayjs from 'dayjs'

export interface TeamObjectivesListCardProps {
  getObjectives: (
    programIncrementId: string,
    teamId: string
  ) => Promise<ProgramIncrementObjectiveListDto[]>
  programIncrementId: string
  teamId: string
  newObjectivesAllowed?: boolean
}

const TeamObjectivesListCard = ({
  getObjectives,
  programIncrementId,
  teamId,
  newObjectivesAllowed = false,
}: TeamObjectivesListCardProps) => {
  const [objectives, setObjectives] = useState<
    ProgramIncrementObjectiveListDto[]
  >([])
  const [openCreateObjectiveModal, setOpenCreateObjectiveModal] =
    useState<boolean>(false)

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.ProgramIncrementObjectives.Manage'
  )
  const canCreateObjectives =
    newObjectivesAllowed && programIncrementId && canManageObjectives

  const loadObjectives = useCallback(
    async (programIncrementId: string, teamId: string) => {
      const objectiveDtos = await getObjectives(programIncrementId, teamId)
      setObjectives(objectiveDtos)
    },
    [getObjectives]
  )

  const refreshObjectives = useCallback(() => {
    loadObjectives(programIncrementId, teamId)
  }, [loadObjectives, programIncrementId, teamId])

  useEffect(() => {
    loadObjectives(programIncrementId, teamId)
  }, [loadObjectives, programIncrementId, teamId])

  const CardTitle = () => {
    const count = objectives?.length ?? 0
    const showBadge = count > 0
    return (
      <Space>
        {'Objectives'}
        {showBadge && <Badge color="white" size="small" count={count} />}
      </Space>
    )
  }

  const ObjectivesList = () => {
    if (!objectives || objectives.length === 0) {
      return <ModaEmpty message="No objectives" />
    }
    const sortedObjectives = objectives.sort((a, b) => {
      if (a.isStretch && !b.isStretch) {
        return 1
      } else if (!a.isStretch && b.isStretch) {
        return -1
      } else {
        const statusOrder = ['Not Started', 'In Progress', 'Closed', 'Canceled']
        const aStatusIndex = statusOrder.indexOf(a.status.name)
        const bStatusIndex = statusOrder.indexOf(b.status.name)
        if (aStatusIndex === bStatusIndex) {
          if (a.targetDate && b.targetDate) {
            return dayjs(a.targetDate).isAfter(dayjs(b.targetDate)) ? 1 : -1
          } else if (a.targetDate) {
            return -1
          } else if (b.targetDate) {
            return 1
          } else {
            return 0
          }
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
            piLocalId={objective.programIncrement.localId}
            canUpdateObjectives={canManageObjectives}
            refreshObjectives={refreshObjectives}
          />
        )}
      />
    )
  }

  const onCreateObjectiveFormClosed = (wasCreated: boolean) => {
    setOpenCreateObjectiveModal(false)
    if (wasCreated) {
      loadObjectives(programIncrementId, teamId)
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
              onClick={() => setOpenCreateObjectiveModal(true)}
            />
          )
        }
      >
        <ObjectivesList />
      </Card>
      {canManageObjectives && (
        <CreateProgramIncrementObjectiveForm
          programIncrementId={programIncrementId}
          teamId={teamId}
          showForm={openCreateObjectiveModal}
          onFormCreate={() => onCreateObjectiveFormClosed(true)}
          onFormCancel={() => onCreateObjectiveFormClosed(false)}
        />
      )}
    </>
  )
}

export default TeamObjectivesListCard
