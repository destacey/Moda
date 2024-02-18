import { WorkProcessDto } from '@/src/services/moda-api'
import { Descriptions } from 'antd'

const { Item } = Descriptions

interface WorkProcessDetailsProps {
  workProcess: WorkProcessDto
}

const WorkProcessDetails = ({ workProcess }: WorkProcessDetailsProps) => {
  if (!workProcess) return null

  return (
    <>
      <Descriptions>
        <Item label="Ownership">{workProcess.ownership.name}</Item>
        <Item label="Is Active?">{workProcess.isActive?.toString()}</Item>
      </Descriptions>
      <Descriptions>
        <Item label="Description">{workProcess.description}</Item>
      </Descriptions>
    </>
  )
}

export default WorkProcessDetails
