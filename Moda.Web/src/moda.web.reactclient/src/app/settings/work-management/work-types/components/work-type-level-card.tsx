import { WorkTypeLevelDto } from '@/src/services/moda-api'
import { Button, List } from 'antd'
import { EditWorkTypeLevelForm } from '.'
import { useState } from 'react'
import { EditOutlined } from '@ant-design/icons'

const { Item } = List
const { Meta } = Item

interface WorkTypeLevelCardProps {
  level: WorkTypeLevelDto
  canUpdateWorkTypeLevels: boolean
  refreshLevels: () => void
}

const WorkTypeLevelCard = (props: WorkTypeLevelCardProps) => {
  const [openEditWorkTypeLevelForm, setOpenEditWorkTypeLevelForm] =
    useState<boolean>(false)

  const onEditObjectiveFormClosed = (wasSaved: boolean) => {
    setOpenEditWorkTypeLevelForm(false)
    if (wasSaved) {
      props.refreshLevels()
    }
  }

  return (
    <>
      <Item
        key={props.level.id}
        actions={[
          props.canUpdateWorkTypeLevels && (
            <Button
              type="text"
              title="Edit work type level"
              icon={<EditOutlined />}
              onClick={() => setOpenEditWorkTypeLevelForm(true)}
            />
          ),
        ]}
      >
        <Meta title={props.level.name} description={props.level.description} />
      </Item>
      {props.canUpdateWorkTypeLevels && (
        <EditWorkTypeLevelForm
          levelId={props.level.id}
          showForm={openEditWorkTypeLevelForm}
          onFormSave={() => onEditObjectiveFormClosed(true)}
          onFormCancel={() => onEditObjectiveFormClosed(false)}
        />
      )}
    </>
  )
}

export default WorkTypeLevelCard
