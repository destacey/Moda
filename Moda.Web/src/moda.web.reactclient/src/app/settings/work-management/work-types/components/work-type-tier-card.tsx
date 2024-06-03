import { ModaEmpty } from '@/src/app/components/common'
import { WorkTypeLevelDto, WorkTypeTierDto } from '@/src/services/moda-api'
import { Button, Card, List, Typography } from 'antd'
import { useEffect, useState } from 'react'
import { WorkTypeLevelCard } from '.'
import { PlusOutlined } from '@ant-design/icons'
import CreateWorkTypeLevelForm from './create-work-type-level-form'

const { Text } = Typography

interface WorkTypeTierCardProps {
  tier: WorkTypeTierDto
  levels: WorkTypeLevelDto[]
  refreshLevels: () => void
  canCreateWorkTypeLevels: boolean
  canUpdateWorkTypeLevels: boolean
}

const WorkTypeTierCard = (props: WorkTypeTierCardProps) => {
  const [orderedLevels, setOrderedLevels] = useState<WorkTypeLevelDto[]>([])
  const [openCreateWorkTypeLevelForm, setOpenCreateWorkTypeLevelForm] =
    useState<boolean>(false)

  const canCreateNewLevel =
    props.tier.name === 'Portfolio' && props.canCreateWorkTypeLevels

  useEffect(() => {
    if (!props.levels) return
    setOrderedLevels(props.levels.sort((a, b) => a.order - b.order))
  }, [props.levels])

  const onCreateObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenCreateWorkTypeLevelForm(false)
    if (wasSaved) {
      props.refreshLevels()
    }
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
        <List
          size="small"
          dataSource={orderedLevels}
          locale={{
            emptyText: <ModaEmpty message="No work type levels" />,
          }}
          renderItem={(level) => (
            <WorkTypeLevelCard
              level={level}
              canUpdateWorkTypeLevels={props.canUpdateWorkTypeLevels}
            />
          )}
        />
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
