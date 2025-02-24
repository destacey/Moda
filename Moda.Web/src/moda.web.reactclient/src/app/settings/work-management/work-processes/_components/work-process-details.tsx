'use client'

import { WorkProcessDto } from '@/src/services/moda-api'
import { useGetWorkProcessSchemesQuery } from '@/src/store/features/work-management/work-process-api'
import { Descriptions, List } from 'antd'

const { Item } = Descriptions
const { Item: ListItem } = List
const { Meta: ListItemMeta } = ListItem

interface WorkProcessDetailsProps {
  workProcess: WorkProcessDto
}

const WorkProcessDetails = ({ workProcess }: WorkProcessDetailsProps) => {
  const { data: schemes, isLoading: isLoadingSchemes } =
    useGetWorkProcessSchemesQuery(workProcess?.id, { skip: !workProcess })

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
      <List
        header="Work Types and Workflows"
        size="small"
        loading={isLoadingSchemes}
        dataSource={schemes}
        renderItem={(scheme) => (
          <ListItem>
            <ListItemMeta
              title={scheme.workType.name}
              description={scheme.workType.description}
            />
            <div>{scheme.workflow.name}</div>
            {/* <Link
              href={`/settings/work-management/workflows/${scheme.workflow.key}`}
            >
              {scheme.workflow.name}
            </Link> */}
          </ListItem>
        )}
      />
    </>
  )
}

export default WorkProcessDetails
