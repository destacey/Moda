'use client'

import { WorkItemsListCard } from '@/src/app/components/common/work'
import useAuth from '@/src/app/components/contexts/auth'
import { useDebounce } from '@/src/app/hooks'
import { useSearchWorkItemsQuery } from '@/src/store/features/work-management/workspace-api'
import { FormOutlined } from '@ant-design/icons'
import { Button, Card, Input, Select, SelectProps } from 'antd'
import { useState } from 'react'
import ManagePlanningIntervalObjectiveWorkItemsForm from './manage-planning-interval-objective-work-items-form'

const { Search } = Input

export interface PlanningIntervalObjectiveWorkItemsCardProps {
  objectiveId: string
  canLinkWorkItems: boolean
}

const PlanningIntervalObjectiveWorkItemsCard = ({
  objectiveId,
  canLinkWorkItems,
}: PlanningIntervalObjectiveWorkItemsCardProps) => {
  const [openManageWorkItemsForm, setOpenManageWorkItemsForm] =
    useState<boolean>(false)

  //   const [data, setData] = useState<SelectProps['options']>([])
  //   const [searchQuery, setSearchQuery] = useState<string>('')

  //   const debounceSearchQuery = useDebounce(searchQuery, 500)
  //   const {
  //     data: searchResult,
  //     isSuccess,
  //     isLoading,
  //     isError,
  //   } = useSearchWorkItemsQuery(debounceSearchQuery, {
  //     skip: debounceSearchQuery === '',
  //   })

  //   const handleSearch = (newValue: string) => {
  //     setSearchQuery(newValue)
  //   }

  //   const handleChange = (newValue: string) => {
  //     //setSearchQuery(newValue)
  //   }

  const testWorkItems = [
    {
      id: '1',
      key: 'TEST-1',
      title: 'Work Item 1',
      status: 'In Progress',
    },
    {
      id: '2',
      key: 'TEST-2',
      title: 'Work Item 2',
      status: 'In Progress',
    },
    {
      id: '3',
      key: 'TEST-3',
      title: 'Work Item 3',
      status: 'In Progress',
    },
  ]

  const onManageWorkItemsFormClosed = (wasSaved: boolean) => {
    setOpenManageWorkItemsForm(false)
    if (wasSaved) {
      //refreshObjectives()
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
            {canLinkWorkItems && (
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
        <WorkItemsListCard workItems={testWorkItems} />
      </Card>
      {openManageWorkItemsForm && (
        <ManagePlanningIntervalObjectiveWorkItemsForm
          id={objectiveId}
          showForm={openManageWorkItemsForm}
          onFormSave={() => onManageWorkItemsFormClosed(true)}
          onFormCancel={() => onManageWorkItemsFormClosed(false)}
        />
      )}
    </>
  )
}

export default PlanningIntervalObjectiveWorkItemsCard
