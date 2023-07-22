'use client'

import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Button, Card, Empty, List, Space } from 'antd'
import ObjectiveListItem from './objective-list-item'
import ModaEmpty from '@/src/app/components/common/moda-empty'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '@/src/app/components/contexts/auth'
import CreateProgramIncrementObjectiveForm from '../create-program-increment-objective-form'

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

  useEffect(() => {
    loadObjectives(programIncrementId, teamId)
  }, [loadObjectives, programIncrementId, teamId])

  const cardTitle = () => {
    let title = `Objectives`
    if (objectives?.length > 0) {
      title += ` (${objectives.length})`
    }
    return title
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
        return a.localId.toString().localeCompare(b.localId.toString())
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
        title={cardTitle()}
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
      <CreateProgramIncrementObjectiveForm
        programIncrementId={programIncrementId}
        teamId={teamId}
        showForm={openCreateObjectiveModal}
        onFormCreate={() => onCreateObjectiveFormClosed(true)}
        onFormCancel={() => onCreateObjectiveFormClosed(false)}
      />
    </>
  )
}

export default TeamObjectivesListCard
