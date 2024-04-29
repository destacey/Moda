'use client'

import { WorkItemsListCard } from '@/src/app/components/common/work'
import { FormOutlined } from '@ant-design/icons'
import { Button, Card, Input } from 'antd'
import { useState } from 'react'
import ManagePlanningIntervalObjectiveWorkItemsForm from './manage-planning-interval-objective-work-items-form'
import { useGetObjectiveWorkItemsQuery } from '@/src/store/features/planning/planning-interval-api'

const { Search } = Input

export interface PlanningIntervalObjectiveWorkItemsCardProps {
  planningIntervalId: string
  objectiveId: string
  canLinkWorkItems: boolean
}

const PlanningIntervalObjectiveWorkItemsCard = (
  props: PlanningIntervalObjectiveWorkItemsCardProps,
) => {
  const [openManageWorkItemsForm, setOpenManageWorkItemsForm] =
    useState<boolean>(false)

  const {
    data: workItemsData,
    isLoading,
    isError,
    refetch,
  } = useGetObjectiveWorkItemsQuery({
    planningIntervalId: props.planningIntervalId,
    objectiveId: props.objectiveId,
  })

  //   const testWorkItems = [
  //     {
  //       id: '1',
  //       key: 'TEST-1',
  //       title: 'Work Item 1',
  //       status: 'In Progress',
  //     },
  //     {
  //       id: '2',
  //       key: 'TEST-2',
  //       title: 'Work Item 2',
  //       status: 'In Progress',
  //     },
  //     {
  //       id: '3',
  //       key: 'TEST-3',
  //       title: 'Work Item 3',
  //       status: 'In Progress',
  //     },
  //   ]

  const onManageWorkItemsFormClosed = (wasSaved: boolean) => {
    setOpenManageWorkItemsForm(false)
    if (wasSaved) {
      refetch()
    }
  }

  return (
    <>
      <Card
        size="small"
        title="Work Items"
        style={{ width: 400 }}
        // add search from api input
        extra={
          <>
            {props.canLinkWorkItems && (
              <Button
                type="text"
                icon={<FormOutlined />}
                title="Manage work items"
                onClick={() => setOpenManageWorkItemsForm(true)}
              />
              //   <Search
              //     size="small"
              //     placeholder="Add work item key"
              //     allowClear
              //     onSearch={handleSearch}
              //   />
              //   <Select
              //     showSearch
              //     allowClear
              //     value={searchQuery}
              //     placeholder="Add work item key"
              //     size="small"
              //     style={{ width: 200 }}
              //     //style={props.style}
              //     defaultActiveFirstOption={false}
              //     suffixIcon={null}
              //     filterOption={false}
              //     onSearch={handleSearch}
              //     onChange={handleChange}
              //     notFoundContent={
              //       isLoading ? 'Loading...' : 'No work items found'
              //     }
              //     options={(!searchQuery ? [] : searchResult || []).map((d) => ({
              //       value: d.key,
              //       label: `${d.key} - ${d.title}`,
              //     }))}
              //   />
            )}
          </>
        }

        // extra={
        //   <>
        //     {canCreateLinks && (
        //       <Button
        //         type="text"
        //         icon={<PlusOutlined />}
        //         onClick={() => setOpenCreateLinkForm(true)}
        //       />
        //     )}
        //     {hasLinks && (canUpdateLinks || canDeleteLinks) && (
        //       <EditModeButton />
        //     )}
        //   </>
        // }
      >
        <WorkItemsListCard workItems={workItemsData} />
      </Card>
      {openManageWorkItemsForm && (
        <ManagePlanningIntervalObjectiveWorkItemsForm
          planningIntervalId={props.planningIntervalId}
          objectiveId={props.objectiveId}
          showForm={openManageWorkItemsForm}
          onFormSave={() => onManageWorkItemsFormClosed(true)}
          onFormCancel={() => onManageWorkItemsFormClosed(false)}
        />
      )}
    </>
  )
}

export default PlanningIntervalObjectiveWorkItemsCard
