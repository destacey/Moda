'use client'

import { useDebounce } from '@/src/app/hooks'
import {
  ManagePlanningIntervalObjectiveWorkItemsRequest,
  WorkItemListDto,
} from '@/src/services/moda-api'
import {
  useGetObjectiveWorkItemsQuery,
  useManageObjectiveWorkItemsMutation,
} from '@/src/store/features/planning/planning-interval-api'
import { useSearchWorkItemsQuery } from '@/src/store/features/work-management/workspace-api'
import { SearchOutlined } from '@ant-design/icons'
import { Input, message, Modal, Space, Typography } from 'antd'
import { useEffect, useRef, useState } from 'react'
import { ColDef, RowSelectionOptions } from 'ag-grid-community'
import { AgGridReact } from 'ag-grid-react'
import {
  AgGridTransfer,
  asDeletableColDefs,
  asDraggableColDefs,
} from '@/src/app/components/common/grid/ag-grid-transfer'

const { Text } = Typography

export interface ManagePlanningIntervalObjectiveWorkItemsFormProps {
  showForm: boolean
  planningIntervalId: string
  objectiveId: string
  onFormSave: () => void
  onFormCancel: () => void
}

type WorkItemModel = WorkItemListDto & {
  disabled: boolean
}

const workItemColDefs: ColDef<WorkItemModel>[] = [
  {
    field: 'key',
    headerName: 'Key',
    minWidth: 100,
  },
  {
    field: 'title',
    headerName: 'Title',
    minWidth: 250,
  },
  {
    field: 'type',
    headerName: 'Type',
    minWidth: 100,
  },
  {
    field: 'status',
    headerName: 'Status',
    minWidth: 100,
  },
  {
    field: 'team.name',
    headerName: 'Team',
    minWidth: 100,
  },
  {
    field: 'parent.key',
    headerName: 'Parent Key',
    minWidth: 100,
  },
]

const leftWorkItemColDefs = [...asDraggableColDefs(workItemColDefs)]

const leftGridRowSelection: RowSelectionOptions<WorkItemModel> = {
  mode: 'multiRow',
  checkboxes: true,
  headerCheckbox: true,
  enableClickSelection: false,
}

const defaultColDef: ColDef = {
  tooltipValueGetter: (params) => params.value,
}

const rightWorkItemColDefs = asDeletableColDefs(workItemColDefs)

const ManagePlanningIntervalObjectiveWorkItemsForm = (
  props: ManagePlanningIntervalObjectiveWorkItemsFormProps,
) => {
  const [isOpen, setIsOpen] = useState(props.showForm)
  const [isSaving, setIsSaving] = useState(false)
  const [searchResultWorkItems, setSearchResultWorkItems] = useState<
    WorkItemModel[]
  >([])
  const [sourceWorkItems, setSourceWorkItems] = useState<WorkItemModel[]>([])
  const [targetWorkItems, setTargetWorkItems] = useState<WorkItemModel[]>([])
  const [messageApi, contextHolder] = message.useMessage()

  const [searchQuery, setSearchQuery] = useState<string>('')

  const rightGridRef = useRef<AgGridReact<WorkItemModel>>(null)

  const {
    data: existingWorkItemsData,
    isLoading: existingWorkItemsQueryIsLoading,
    isError: existingWorkItemsQueryIsError,
  } = useGetObjectiveWorkItemsQuery({
    planningIntervalId: props.planningIntervalId,
    objectiveId: props.objectiveId,
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
      existingWorkItemsData?.workItems.map((item) => ({
        ...item,
        disabled: false,
      })) ?? [],
    )
  }, [existingWorkItemsData])

  useEffect(() => {
    if (!searchResult) return

    setSearchResultWorkItems(
      searchResult?.map((item) => ({ ...item, disabled: false })) ?? [],
    )
  }, [searchResult])

  useEffect(() => {
    let selectedIds = []
    rightGridRef.current?.api?.forEachNode((n) => selectedIds.push(n.data.id))

    setSourceWorkItems(
      searchResultWorkItems.filter((item) => !selectedIds.includes(item.id)),
    )
  }, [searchQuery, searchResultWorkItems, targetWorkItems])

  const saveWorkItemChanges = async (): Promise<boolean> => {
    try {
      const workItemIds = []
      rightGridRef.current?.api.forEachNode((n) => workItemIds.push(n.data.id))
      const request: ManagePlanningIntervalObjectiveWorkItemsRequest = {
        planningIntervalId: props.planningIntervalId,
        objectiveId: props.objectiveId,
        workItemIds,
      }
      await manageObjectiveWorkItems(request)
      messageApi.success('Successfully updated objective work items.')
      return true
    } catch (error) {
      messageApi.error(
        `Failed to update objective work items. Error: ${error.supportMessage}`,
      )
      console.error(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await saveWorkItemChanges()) {
        setIsOpen(false)
        setIsSaving(false)
        props.onFormSave()
      } else {
        setIsSaving(false)
      }
    } catch (errorInfo) {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
  }

  const handleSearch = (e) => {
    setSearchQuery(e.target.value)
  }

  return (
    <>
      {contextHolder}
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
          <Space
            direction="vertical"
            style={{ display: 'flex', width: '100%' }}
          >
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
              leftColumnDef={leftWorkItemColDefs}
              rightColumnDef={rightWorkItemColDefs}
              rightGridRef={rightGridRef}
              removeRowFromSource
              getRowId={(param) => param.data.id}
              GridProps={{
                tooltipShowDelay: 0,
                tooltipHideDelay: 1000,
                defaultColDef,
              }}
              leftGridRowSelection={leftGridRowSelection}
            />
            <Text>Search results are limited to 50 records.</Text>
          </Space>
        }
      </Modal>
    </>
  )
}

export default ManagePlanningIntervalObjectiveWorkItemsForm
