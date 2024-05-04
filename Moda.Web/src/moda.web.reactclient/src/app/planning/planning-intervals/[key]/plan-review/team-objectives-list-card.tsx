'use client'

import { PlanningIntervalObjectiveListDto } from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Badge, Button, Card, List, Space } from 'antd'
import ObjectiveListItem from './objective-list-item'
import ModaEmpty from '@/src/app/components/common/moda-empty'
import { useCallback, useEffect, useMemo, useState } from 'react'
import useAuth from '@/src/app/components/contexts/auth'
import CreatePlanningIntervalObjectiveForm from '../../../components/create-planning-interval-objective-form'
import { UseQueryResult } from 'react-query'
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

export interface TeamObjectivesListCardProps {
  objectivesQuery: UseQueryResult<PlanningIntervalObjectiveListDto[], unknown>
  planningIntervalId: string
  teamId: string
  newObjectivesAllowed?: boolean
  refreshPlanningInterval: () => void
}

const TeamObjectivesListCard = ({
  objectivesQuery,
  planningIntervalId,
  teamId,
  newObjectivesAllowed = false,
  refreshPlanningInterval,
}: TeamObjectivesListCardProps) => {
  const [openCreateObjectiveForm, setOpenCreateObjectiveForm] =
    useState<boolean>(false)
  const [objectives, setObjectives] = useState<
    PlanningIntervalObjectiveListDto[]
  >([])

  const { badgeColor } = useTheme()

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.PlanningIntervalObjectives.Manage',
  )
  const canCreateObjectives =
    newObjectivesAllowed && planningIntervalId && canManageObjectives
  const canCreateHealthChecks =
    !!canManageObjectives &&
    hasClaim('Permission', 'Permissions.HealthChecks.Create')

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(TouchSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    }),
  )

  useEffect(() => {
    if (!objectivesQuery.data) return

    setObjectives(
      objectivesQuery.data.sort((a, b) => {
        if (a.order === null && b.order === null) return a.key - b.key
        if (a.order === null) return 1
        if (b.order === null) return -1
        if (a.order === b.order) return a.key - b.key
        return a.order - b.order
      }),
    )
  }, [objectivesQuery.data])

  const refreshObjectives = useCallback(() => {
    objectivesQuery.refetch()
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
      refreshObjectives()
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

    setObjectives((objectives) => {
      const originalPosition = getObjectivePosition(active.id)
      const newPosition = getObjectivePosition(over.id)

      return arrayMove(objectives, originalPosition, newPosition)
    })

    // TODO: call the api to update the objective order
  }

  return (
    <>
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
                  refreshObjectives={refreshObjectives}
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
