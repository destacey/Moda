'use client'

import { WorkItemDetailsDto } from '@/src/services/moda-api'
import { Descriptions } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import WorkItemSteps from './work-item-steps'

const { Item } = Descriptions

export interface WorkItemDetailsProps {
  workItem: WorkItemDetailsDto
}

const WorkItemDetails = ({ workItem }: WorkItemDetailsProps) => {
  if (!workItem) return null

  const teamLink =
    workItem.team?.type === 'Team'
      ? `/organizations/teams/${workItem.team?.key}`
      : `/organizations/team-of-teams/${workItem.team?.key}`

  return (
    <>
      <Descriptions>
        <Item label="Key">{workItem.key}</Item>
        <Item label="Type">{workItem.type}</Item>
        <Item label="Status">{workItem.status}</Item>
        <Item label="Status Category">{workItem.statusCategory.name}</Item>
        <Item label="Priority">{workItem.priority}</Item>
        <Item label="Team">
          <Link href={teamLink}>{workItem.team?.name}</Link>
        </Item>
        <Item label="Parent">
          {workItem.parent ? (
            <Link
              href={`/work/workspaces/${workItem.workspace.key}/work-items/${workItem.parent.key}`}
            >
              {workItem.parent.key} - {workItem.parent.title}
            </Link>
          ) : (
            'No Parent'
          )}
        </Item>
        <Item label="Assigned To">
          {workItem.assignedTo ? (
            <Link href={`/organizations/employees/${workItem.assignedTo.key}`}>
              {workItem.assignedTo.name}
            </Link>
          ) : (
            'Unassigned'
          )}
        </Item>
        <Item label="Created By">
          {workItem.createdBy ? (
            <Link href={`/organizations/employees/${workItem.createdBy.key}`}>
              {workItem.createdBy.name}
            </Link>
          ) : (
            'Unknown'
          )}
        </Item>
        <Item label="Created">
          {dayjs(workItem.created).format('MMM D, YYYY @ h:mm A')}
        </Item>
        <Item label="Last Modified By">
          {workItem.lastModifiedBy ? (
            <Link
              href={`/organizations/employees/${workItem.lastModifiedBy.key}`}
            >
              {workItem.lastModifiedBy.name}
            </Link>
          ) : (
            'Unknown'
          )}
        </Item>
        <Item label="Updated">
          {dayjs(workItem.lastModified).format('MMM D, YYYY @ h:mm A')}
        </Item>
      </Descriptions>
      <br />
      <WorkItemSteps workItem={workItem} />
    </>
  )
}

export default WorkItemDetails
