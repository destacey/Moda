'use client'

import { ModaEmpty } from '@/src/app/components/common'
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
import {
  Input,
  Modal,
  Space,
  Table,
  Transfer,
  TransferProps,
  Typography,
  message,
} from 'antd'
import type { ColumnsType, TableRowSelection } from 'antd/es/table/interface'
import { difference } from 'lodash'
import { useEffect, useState } from 'react'

const { Text } = Typography

export type TransferDirection = 'left' | 'right'

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

interface TableTransferProps extends TransferProps<WorkItemModel> {
  leftColumns: ColumnsType<WorkItemModel>
  rightColumns: ColumnsType<WorkItemModel>
}

const TableTransfer = ({
  leftColumns,
  rightColumns,
  ...restProps
}: TableTransferProps) => (
  <Transfer {...restProps}>
    {({
      direction,
      filteredItems,
      onItemSelectAll,
      onItemSelect,
      selectedKeys: listSelectedKeys,
      disabled: listDisabled,
    }) => {
      const columns = direction === 'left' ? leftColumns : rightColumns

      const rowSelection: TableRowSelection<WorkItemModel> = {
        getCheckboxProps: (item) => ({
          disabled: listDisabled || item.disabled,
        }),
        onSelectAll(selected, selectedRows) {
          const treeSelectedKeys = selectedRows
            .filter((item) => !item.disabled)
            .map(({ key }) => key)
          const diffKeys = selected
            ? difference(treeSelectedKeys, listSelectedKeys)
            : difference(listSelectedKeys, treeSelectedKeys)
          onItemSelectAll(diffKeys as string[], selected)
        },
        onSelect({ key }, selected) {
          onItemSelect(key as string, selected)
        },
        selectedRowKeys: listSelectedKeys,
      }

      return (
        <Table
          rowSelection={rowSelection}
          columns={columns}
          dataSource={filteredItems}
          size="small"
          pagination={false}
          scroll={{ y: '50vh' }}
          style={{
            pointerEvents: listDisabled ? 'none' : undefined,
          }}
          locale={{
            emptyText: <ModaEmpty message="No work items found" />,
          }}
          onRow={({ key, disabled: itemDisabled }) => ({
            onClick: () => {
              if (itemDisabled || listDisabled) return
              onItemSelect(
                key as string,
                !listSelectedKeys.includes(key as string),
              )
            },
          })}
        />
      )
    }}
  </Transfer>
)

const tableColumns: ColumnsType<WorkItemModel> = [
  {
    dataIndex: 'key',
    title: 'Key',
    key: '1',
  },
  {
    dataIndex: 'title',
    title: 'Title',
    key: '2',
  },
  {
    dataIndex: 'type',
    title: 'Type',
    key: '3',
  },
  {
    dataIndex: ['parent', 'key'],
    title: 'Parent Key',
    key: '4',
  },
]

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
      const request: ManagePlanningIntervalObjectiveWorkItemsRequest = {
        planningIntervalId: props.planningIntervalId,
        objectiveId: props.objectiveId,
        workItemIds: targetWorkItems.map((item) => item.id),
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
          <Space direction="vertical">
            <Input
              size="small"
              placeholder="Search for work items by key, title, or parent key"
              allowClear
              onChange={handleSearch}
              suffix={<SearchOutlined />}
            />
            <TableTransfer
              dataSource={sourceWorkItems}
              targetKeys={targetKeys}
              onChange={onChange}
              leftColumns={tableColumns}
              rightColumns={tableColumns}
            />
            <Text>Search results are limited to 50 records.</Text>
          </Space>
        }
      </Modal>
    </>
  )
}

export default ManagePlanningIntervalObjectiveWorkItemsForm
