'use client'

import { WorkItemListDto } from '@/src/services/moda-api'
import { Table, Transfer, TransferProps } from 'antd'
import type { ColumnsType, TableRowSelection } from 'antd/es/table/interface'
import { difference } from 'lodash'

export interface ManagePlanningIntervalObjectiveWorkItemsFormProps {
  showForm: boolean
  id: string
  onFormSave: () => void
  onFormCancel: () => void
}

interface TableTransferProps extends TransferProps<WorkItemListDto> {
  dataSource: WorkItemListDto[]
  leftColumns: ColumnsType<WorkItemListDto>
  rightColumns: ColumnsType<WorkItemListDto>
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

      const rowSelection: TableRowSelection<WorkItemListDto> = {
        getCheckboxProps: (item) => ({
          disabled: listDisabled,
        }),
        onSelectAll(selected, selectedRows) {
          const treeSelectedKeys = selectedRows.map(({ key }) => key)
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
          style={{ pointerEvents: listDisabled ? 'none' : undefined }}
          onRow={({ key }) => ({
            onClick: () => {
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

const tableColumns: ColumnsType<WorkItemListDto> = [
  {
    dataIndex: 'key',
    title: 'Key',
  },
  {
    dataIndex: 'title',
    title: 'Title',
  },
]
