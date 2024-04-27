'use client'

import { WorkItemsListCard } from '@/src/app/components/common/work'
import useAuth from '@/src/app/components/contexts/auth'
import { useDebounce } from '@/src/app/hooks'
import { useSearchWorkItemsQuery } from '@/src/store/features/work-management/workspace-api'
import { PlusOutlined } from '@ant-design/icons'
import { Button, Card, Input, Select, SelectProps } from 'antd'
import { useState } from 'react'

const { Search } = Input

export interface PlanningIntervalObjectiveWorkItemsCardProps {
  objectiveId: string
  canLinkWorkItems: boolean
}

const PlanningIntervalObjectiveWorkItemsCard = ({
  objectiveId,
  canLinkWorkItems,
}: PlanningIntervalObjectiveWorkItemsCardProps) => {
  const [data, setData] = useState<SelectProps['options']>([])
  const [searchQuery, setSearchQuery] = useState<string>('')

  const debounceSearchQuery = useDebounce(searchQuery, 500)
  const {
    data: searchResult,
    isSuccess,
    isLoading,
    isError,
  } = useSearchWorkItemsQuery(debounceSearchQuery, {
    skip: debounceSearchQuery === '',
  })

  //   const {
  //     data: searchResult,
  //     isSuccess,
  //     isFetching,
  //     isError,
  //     refetch: fetch,
  //   } = useSearchWorkItemsQuery(searchQuery, {
  //     skip: searchQuery === '',
  //   })

  const handleSearch = (newValue: string) => {
    setSearchQuery(newValue)
  }

  const handleChange = (newValue: string) => {
    //setSearchQuery(newValue)
  }

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
              <Search
                size="small"
                placeholder="Add work item key"
                allowClear
                onSearch={handleSearch}
              />
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
    </>
  )
}

export default PlanningIntervalObjectiveWorkItemsCard
