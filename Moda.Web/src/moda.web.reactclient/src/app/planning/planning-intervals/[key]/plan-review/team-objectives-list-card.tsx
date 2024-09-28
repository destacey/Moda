'use client'

import { PlanningIntervalObjectiveListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Badge, Button, Card, List, Space, message } from 'antd'
import ObjectiveListItem from './objective-list-item'
import ModaEmpty from '@/src/app/components/common/moda-empty'
import { useCallback, useEffect, useMemo, useState } from 'react'
import CreatePlanningIntervalObjectiveForm from '../../../components/create-planning-interval-objective-form'
import useTheme from '@/src/app/components/contexts/theme'
import {
  DndContext,
  DragEndEvent,
  KeyboardSensor,
  PointerSensor,
  TouchSensor,
  closestCorners,
  useSensor,
  useSensors,
} from '@dnd-kit/core'
import {
  SortableContext,
  arrayMove,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable'
import { useUpdateObjectivesOrderMutation } from '@/src/store/features/planning/planning-interval-api'

export interface TeamObjectivesListCardProps {
  objectivesData: PlanningIntervalObjectiveListDto[]
  refreshObjectives: () => void
  planningIntervalId: string
  teamId: string
  newObjectivesAllowed?: boolean
  refreshPlanningInterval: () => void
  onObjectiveClick: (objectiveId: string) => void
  canManageObjectives: boolean
  canCreateHealthChecks: boolean
}

const sortOrderedObjectives = (
  objectivesData: PlanningIntervalObjectiveListDto[],
) => {
  // .slice() is used to prevent: TypeError: Cannot assign to read only property '0' of object '[object Array]'
  return objectivesData.slice().sort((a, b) => {
    if (a.order === null && b.order === null) return a.key - b.key
    if (a.order === null) return 1
    if (b.order === null) return -1
    if (a.order === b.order) return a.key - b.key
    return a.order - b.order
  })
}

const TeamObjectivesListCard = ({
  objectivesData,
  refreshObjectives,
  planningIntervalId,
  teamId,
  newObjectivesAllowed = false,
  refreshPlanningInterval,
  onObjectiveClick,
  canManageObjectives,
  canCreateHealthChecks,
}: TeamObjectivesListCardProps) => {
  const [openCreateObjectiveForm, setOpenCreateObjectiveForm] =
    useState<boolean>(false)
  const [objectives, setObjectives] = useState<
    PlanningIntervalObjectiveListDto[]
  >([])

  const [messageApi, contextHolder] = message.useMessage()

  const { badgeColor } = useTheme()

  const canCreateObjectives =
    newObjectivesAllowed && planningIntervalId && canManageObjectives

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(TouchSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    }),
  )

  const [updateObjectivesOrder, { error: updateObjectivesOrderError }] =
    useUpdateObjectivesOrderMutation()

  useEffect(() => {
    if (!objectivesData) return

    setObjectives(sortOrderedObjectives(objectivesData))
  }, [objectivesData])

  useEffect(() => {
    if (!updateObjectivesOrderError) return

    setObjectives(sortOrderedObjectives(objectivesData))

    // TODO: show error message not working
    messageApi.error('Error updating objectives order.  Resetting order...')

    console.error(
      'Error updating objectives order:',
      updateObjectivesOrderError,
    )
  }, [messageApi, objectivesData, updateObjectivesOrderError])

  const refresh = useCallback(() => {
    refreshObjectives()
    // this will update the PI predictability on the plan review page title
    refreshPlanningInterval()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const cardTitle = useMemo(() => {
    const count = objectives.length ?? 0
    const showBadge = count > 0
    return (
      <Space>
        {'Objectives'}
        {showBadge && <Badge color={badgeColor} size="small" count={count} />}
      </Space>
    )
  }, [objectives.length, badgeColor])

  const onCreateObjectiveFormClosed = (wasCreated: boolean) => {
    setOpenCreateObjectiveForm(false)
    if (wasCreated) {
      refresh()
    }
  }

  const newObjectiveOrderValue = () => {
    if (!objectives || objectives.length === 0) return null

    const lastObjectiveOrder = objectives[objectives.length - 1].order

    return !lastObjectiveOrder ? null : lastObjectiveOrder + 1
  }

  const getObjectivePosition = (id) => objectives.findIndex((o) => o.id === id)

  const onDragEnd = (event: DragEndEvent) => {
    const { active, over } = event

    if (active.id === over.id) return

    const originalPosition = getObjectivePosition(active.id)
    const newPosition = getObjectivePosition(over.id)

    const updatedObjectives = arrayMove(
      objectives,
      originalPosition,
      newPosition,
    )

    setObjectives(updatedObjectives)

    // after optimistic update
    const changedObjectivesDictionary: { [key: string]: number } = {}
    updatedObjectives.forEach((o, i) => {
      const position = i + 1
      if (o.order !== position) {
        changedObjectivesDictionary[o.id] = position
      }
    })

    updateObjectivesOrder({
      planningIntervalId: planningIntervalId,
      objectives: changedObjectivesDictionary,
    })
  }

  return (
    <>
      {contextHolder}
      <Card
        size="small"
        title={cardTitle}
        extra={
          canCreateObjectives && (
            <Button
              type="text"
              icon={<PlusOutlined />}
              onClick={() => setOpenCreateObjectiveForm(true)}
            />
          )
        }
        styles={{ body: { padding: 4 } }}
      >
        <DndContext
          sensors={sensors}
          collisionDetection={closestCorners}
          onDragEnd={onDragEnd}
        >
          <List
            size="small"
            dataSource={objectives}
            locale={{
              emptyText: <ModaEmpty message="No objectives" />,
            }}
            renderItem={(objective) => (
              <SortableContext
                items={objectives.map((objective) => objective.id)}
                strategy={verticalListSortingStrategy}
              >
                <ObjectiveListItem
                  objective={objective}
                  piKey={objective.planningInterval.key}
                  canUpdateObjectives={canManageObjectives}
                  canCreateHealthChecks={canCreateHealthChecks}
                  refreshObjectives={refresh}
                  onObjectiveClick={onObjectiveClick}
                />
              </SortableContext>
            )}
          />
        </DndContext>
      </Card>
      {openCreateObjectiveForm && (
        <CreatePlanningIntervalObjectiveForm
          planningIntervalId={planningIntervalId}
          teamId={teamId}
          order={newObjectiveOrderValue()}
          showForm={openCreateObjectiveForm}
          onFormCreate={() => onCreateObjectiveFormClosed(true)}
          onFormCancel={() => onCreateObjectiveFormClosed(false)}
        />
      )}
    </>
  )
}

export default TeamObjectivesListCard
