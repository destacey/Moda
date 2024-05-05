'use client'

import { useDebounce } from '@/src/app/hooks'
import { ManagePlanningIntervalObjectiveWorkItemsRequest, WorkItemListDto } from '@/src/services/moda-api'
import {
  useGetObjectiveWorkItemsQuery,
  useManageObjectiveWorkItemsMutation
} from '@/src/store/features/planning/planning-interval-api'
import { useSearchWorkItemsQuery } from '@/src/store/features/work-management/workspace-api'
import { SearchOutlined } from '@ant-design/icons'
import { Input, message, Modal, Space, Typography } from 'antd'
import { useEffect, useRef, useState } from 'react'
import { ColDef } from 'ag-grid-community'
import { AgGridReact } from 'ag-grid-react'
import { AgGridTransfer, asDeletableColDefs, asDraggableColDefs } from '@/src/app/components/common/grid/AgGridTransfer'

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
  },
  {
    field: 'title',
    headerName: 'Title',
  },
  {
    field: 'type',
    headerName: 'Type',
  },
  {
    field: 'parent.key',
    headerName: 'Parent Key',
  },
]

const leftWorkItemColDefs = asDraggableColDefs(workItemColDefs);
const rightWorkItemColDefs = asDeletableColDefs(workItemColDefs);

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
  const [targetKeys, setTargetKeys] = useState<string[]>([])
  const [messageApi, contextHolder] = message.useMessage()

  const [searchQuery, setSearchQuery] = useState<string>('')

  const rightGridRef = useRef<AgGridReact<WorkItemModel>>(null);

  const {
    data: existingWorkItemsData,
    isLoading: existingWorkItemsQueryIsLoading,
    isError: existingWorkItemsQueryIsError,
  } = useGetObjectiveWorkItemsQuery({
    planningIntervalId: props.planningIntervalId,
    objectiveId: props.objectiveId,
  })

  const debounceSearchQuery = useDebounce(searchQuery, 500)
  const {
    data: searchResult,
    isSuccess,
    isLoading,
    isError,
  } = useSearchWorkItemsQuery(debounceSearchQuery, {
    skip: debounceSearchQuery === '',
  })

  const [manageObjectiveWorkItems, { error }] =
    useManageObjectiveWorkItemsMutation()

  useEffect(() => {
    if (!existingWorkItemsData) return
    setTargetWorkItems(
      existingWorkItemsData?.map((item) => ({ ...item, disabled: false })) ??
        [],
    )
    setTargetKeys(existingWorkItemsData.map((item) => item.key))
  }, [existingWorkItemsData])

  useEffect(() => {
    if (!searchResult) return

    setSearchResultWorkItems(
      searchResult?.map((item) => ({ ...item, disabled: false })) ?? [],
    )
  }, [searchResult])

  useEffect(() => {
    if (searchQuery === '') {
      setSourceWorkItems(targetWorkItems)
    } else if (targetWorkItems.length === 0) {
      setSourceWorkItems(searchResultWorkItems)
    } else {
      const mergedWorkItems = new Set([
        ...searchResultWorkItems,
        ...targetWorkItems,
      ])
      setSourceWorkItems(Array.from(mergedWorkItems))
    }
  }, [searchQuery, searchResultWorkItems, targetWorkItems])

  const saveWorkItemChanges = async (): Promise<boolean> => {
    try {
      const workItemIds  = [];
      rightGridRef.current?.api.forEachNode((n) => workItemIds.push(n.data.id));
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
        messageApi.success(`Successfully updated PI Objective Work Items.`)
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

  const onChange = (nextTargetKeys: string[]) => {
    var selectedWorkItems = sourceWorkItems?.filter((item) =>
      nextTargetKeys.includes(item.key),
    )
    setTargetKeys(nextTargetKeys)
    setTargetWorkItems(selectedWorkItems)
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
          <Space direction="vertical" style={{display: 'flex', width: '100%'}}>
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
              getRowId={(param) => param.data.id}
            />
            <Text>Search results are limited to 50 records.</Text>
          </Space>
        }
      </Modal>
    </>
  )
}

export default ManagePlanningIntervalObjectiveWorkItemsForm
