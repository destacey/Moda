import { ModaEmpty } from '@/src/components/common'
import { WorkTypeLevelDto, WorkTypeTierDto } from '@/src/services/moda-api'
import { Button, Card, List, Typography } from 'antd'
import { useEffect, useState } from 'react'
import { CreateWorkTypeLevelForm, WorkTypeLevelCard } from '.'
import { PlusOutlined } from '@ant-design/icons'
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
import { useUpdateWorkTypeLevelsOrderMutation } from '@/src/store/features/work-management/work-type-level-api'

const { Text } = Typography

interface WorkTypeTierCardProps {
  tier: WorkTypeTierDto
  levels: WorkTypeLevelDto[]
  refreshLevels: () => void
  canCreateWorkTypeLevels: boolean
  canUpdateWorkTypeLevels: boolean
}

const sortOrderedLevels = (levels: WorkTypeLevelDto[]) => {
  // .slice() is used to prevent: TypeError: Cannot assign to read only property '0' of object '[object Array]'
  return levels.slice().sort((a, b) => {
    return a.order - b.order
  })
}

const WorkTypeTierCard = (props: WorkTypeTierCardProps) => {
  const [orderedLevels, setOrderedLevels] = useState<WorkTypeLevelDto[]>([])
  const [openCreateWorkTypeLevelForm, setOpenCreateWorkTypeLevelForm] =
    useState<boolean>(false)

  const canOrder = props.tier.name === 'Portfolio'

  const canCreateNewLevel = canOrder && props.canCreateWorkTypeLevels

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(TouchSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    }),
  )

  const [updateLevelsOrder] = useUpdateWorkTypeLevelsOrderMutation()

  useEffect(() => {
    if (!props.levels) return
    setOrderedLevels(sortOrderedLevels(props.levels))
  }, [props.levels])

  const onCreateObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenCreateWorkTypeLevelForm(false)
    if (wasSaved) {
      props.refreshLevels()
    }
  }

  const getLevelPosition = (id) => orderedLevels.findIndex((o) => o.id === id)

  const onDragEnd = (event: DragEndEvent) => {
    if (!canOrder) return

    const { active, over } = event

    if (active.id === over.id) return

    const originalPosition = getLevelPosition(active.id)
    const newPosition = getLevelPosition(over.id)

    const updatedLevels = arrayMove(
      orderedLevels,
      originalPosition,
      newPosition,
    )

    setOrderedLevels(updatedLevels)

    // after optimistic update
    // eslint-disable-next-line prefer-const
    let changedLevelsDictionary: { [key: string]: number } = {}
    updatedLevels.forEach((o, i) => {
      const position = i + 1
      if (o.order !== position) {
        changedLevelsDictionary[o.id] = position
      }
    })

    updateLevelsOrder({
      levels: changedLevelsDictionary,
    })
  }

  return (
    <>
      <Card
        size="small"
        title={props.tier.name}
        extra={
          canCreateNewLevel && (
            <Button
              type="text"
              title="Create new work type level"
              icon={<PlusOutlined />}
              onClick={() => setOpenCreateWorkTypeLevelForm(true)}
            />
          )
        }
      >
        <Text>{props.tier.description}</Text>
        <br />
        <br />
        <DndContext
          sensors={sensors}
          collisionDetection={closestCorners}
          onDragEnd={onDragEnd}
        >
          <List
            size="small"
            style={{ width: '100%' }}
            dataSource={orderedLevels}
            locale={{
              emptyText: <ModaEmpty message="No work type levels" />,
            }}
            renderItem={(level) => (
              <SortableContext
                items={orderedLevels.map((level) => level.id)}
                strategy={verticalListSortingStrategy}
              >
                <WorkTypeLevelCard
                  level={level}
                  canUpdateWorkTypeLevels={props.canUpdateWorkTypeLevels}
                  canOrder={canOrder}
                  refreshLevels={props.refreshLevels}
                />
              </SortableContext>
            )}
          />
        </DndContext>
      </Card>
      {openCreateWorkTypeLevelForm && (
        <CreateWorkTypeLevelForm
          showForm={openCreateWorkTypeLevelForm}
          onFormSave={() => onCreateObjectiveFormClosed(true)}
          onFormCancel={() => onCreateObjectiveFormClosed(false)}
        />
      )}
    </>
  )
}

export default WorkTypeTierCard
