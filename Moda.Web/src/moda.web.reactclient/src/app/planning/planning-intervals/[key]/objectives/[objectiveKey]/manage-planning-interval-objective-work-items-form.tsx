'use client'

import { useDebounce } from '@/src/hooks'
import {
  ManagePlanningIntervalObjectiveWorkItemsRequest,
  WorkItemListDto,
} from '@/src/services/moda-api'
import {
  useGetObjectiveWorkItemsQuery,
  useGetPlanningIntervalObjectiveQuery,
  useManageObjectiveWorkItemsMutation,
} from '@/src/store/features/planning/planning-interval-api'
import { useSearchWorkItemsQuery } from '@/src/store/features/work-management/workspace-api'
import { SearchOutlined } from '@ant-design/icons'
import { Flex, Input, Modal, Typography } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { ColDef } from 'ag-grid-community'
import {
  AgGridTransfer,
  asDeletableColDefs,
  asDraggableColDefs,
} from '@/src/components/common/grid/ag-grid-transfer'
import { useMessage } from '@/src/components/contexts/messaging'
import { workItemKeyComparator } from '@/src/components/common/work'

const { Text } = Typography

export interface ManagePlanningIntervalObjectiveWorkItemsFormProps {
  showForm: boolean
  planningIntervalKey: number
  objectiveKey: number
  onFormComplete: () => void
  onFormCancel: () => void
}

const workItemColDefs: ColDef<WorkItemListDto>[] = [
  {
    field: 'key',
    headerName: 'Key',
    width: 125,
  },
  {
    field: 'title',
    headerName: 'Title',
    width: 250,
  },
  {
    field: 'type',
    headerName: 'Type',
    width: 100,
  },
  {
    field: 'status',
    headerName: 'Status',
    width: 100,
  },
  {
    field: 'team.name',
    headerName: 'Team',
    width: 150,
  },
  {
    field: 'parent.key',
    headerName: 'Parent Key',
    width: 125,
  },
  {
    field: 'project.name',
    headerName: 'Project',
    width: 200,
  },
]

const leftColDefs = [...asDraggableColDefs(workItemColDefs)]

const defaultSort = (a: WorkItemListDto, b: WorkItemListDto) => {
  return workItemKeyComparator(a.key, b.key)
}

const defaultColDef: ColDef = {
  filter: false,
}

const ManagePlanningIntervalObjectiveWorkItemsForm = (
  props: ManagePlanningIntervalObjectiveWorkItemsFormProps,
) => {
  const {
    showForm,
    planningIntervalKey,
    objectiveKey,
    onFormComplete,
    onFormCancel,
  } = props

  const [isOpen, setIsOpen] = useState(showForm)
  const [isSaving, setIsSaving] = useState(false)
  const [searchResultWorkItems, setSearchResultWorkItems] = useState<
    WorkItemListDto[]
  >([])
  const [sourceWorkItems, setSourceWorkItems] = useState<WorkItemListDto[]>([])
  const [targetWorkItems, setTargetWorkItems] = useState<WorkItemListDto[]>([])
  const messageApi = useMessage()

  const [searchQuery, setSearchQuery] = useState<string>('')

  const { data: objectiveData } = useGetPlanningIntervalObjectiveQuery({
    planningIntervalKey: planningIntervalKey.toString(),
    objectiveKey: objectiveKey.toString(),
  })

  const {
    data: existingWorkItemsData,
    isLoading: existingWorkItemsQueryIsLoading,
    isError: existingWorkItemsQueryIsError,
  } = useGetObjectiveWorkItemsQuery({
    planningIntervalKey: planningIntervalKey.toString(),
    objectiveKey: objectiveKey.toString(),
  })

  const debounceSearchQuery = useDebounce(searchQuery, 500)
  const { data: searchResult } = useSearchWorkItemsQuery(debounceSearchQuery, {
    skip: debounceSearchQuery === '',
  })

  const [manageObjectiveWorkItems, { error }] =
    useManageObjectiveWorkItemsMutation()

  useEffect(() => {
    if (!existingWorkItemsData) return
    setTargetWorkItems(
      existingWorkItemsData?.workItems?.slice().sort(defaultSort) ?? [],
    )
  }, [existingWorkItemsData])

  useEffect(() => {
    if (!searchResult) return

    setSearchResultWorkItems(searchResult ?? [])
  }, [searchResult])

  useEffect(() => {
    const selectedIds =
      existingWorkItemsData?.workItems?.map((item) => item.id) ?? []

    const filteredWorkItems = searchResultWorkItems
      .filter((item) => !selectedIds.includes(item.id))
      .sort(defaultSort)

    setSourceWorkItems(filteredWorkItems)
  }, [existingWorkItemsData, searchResultWorkItems])

  const onDragStop = useCallback((items: WorkItemListDto[]) => {
    // using the functional update form of setState to ensure we are using the latest state
    if (items.length === 0) return

    setSourceWorkItems((prevSource) =>
      prevSource.filter((p) => !items.some((i) => i.id === p.id)),
    )

    setTargetWorkItems((prevTarget) =>
      [...prevTarget, ...items].sort(defaultSort),
    )
  }, [])

  const handleDelete = useCallback((item: WorkItemListDto) => {
    // using the functional update form of setState to ensure we are using the latest state
    if (!item) return

    setTargetWorkItems((prevTarget) =>
      prevTarget.filter((p) => p.id !== item.id),
    )

    setSourceWorkItems((prevSource) => [...prevSource, item].sort(defaultSort))
  }, [])

  const rightColDefs = useMemo(
    () => asDeletableColDefs(workItemColDefs, handleDelete),
    [handleDelete],
  )

  const formAction = async (): Promise<boolean> => {
    try {
      // TODO: get the objectiveData from the API and use it to get the work items
      const request: ManagePlanningIntervalObjectiveWorkItemsRequest = {
        planningIntervalId: objectiveData?.planningInterval.id,
        objectiveId: objectiveData?.id,
        workItemIds: targetWorkItems.map((item) => item.id),
      }
      await manageObjectiveWorkItems({
        request,
        cacheKey: objectiveKey.toString(),
      })
      messageApi.success('Successfully updated objective work items.')
      return true
    } catch (error) {
      messageApi.error(
        `Failed to update objective work items. Error: ${error.detail}`,
      )
      console.error(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await formAction()) {
        setIsOpen(false)
        onFormComplete()
      }
    } catch (error) {
      console.error(error)
      messageApi.error(
        'An error occurred while managing the work items. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    onFormCancel()
  }, [onFormCancel])

  const handleSearch = (e) => {
    setSearchQuery(e.target.value)
  }

  return (
    <Modal
      title="Manage PI Objective Work Items"
      open={isOpen}
      width={'80vw'}
      onOk={handleOk}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      maskClosable={false}
      keyboard={false} // disable esc key to close modal
      destroyOnClose={true}
    >
      {
        <Flex gap="small" vertical>
          <Input
            size="small"
            placeholder="Search for work items by key, title, or parent key"
            allowClear
            onChange={handleSearch}
            suffix={<SearchOutlined />}
          />
          <AgGridTransfer
            leftGridData={sourceWorkItems}
            rightGridData={targetWorkItems}
            leftColumnDef={leftColDefs}
            rightColumnDef={rightColDefs}
            onDragStop={onDragStop}
            getRowId={(param) => param.data.id}
            GridProps={{
              defaultColDef,
            }}
          />
          <Text>Search results are limited to 50 records.</Text>
        </Flex>
      }
    </Modal>
  )
}

export default ManagePlanningIntervalObjectiveWorkItemsForm
