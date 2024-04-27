'use client'

import { useDebounce } from '@/src/app/hooks'
import { WorkItemListDto } from '@/src/services/moda-api'
import { useSearchWorkItemsQuery } from '@/src/store/features/work-management/workspace-api'
import { Modal, Spin, Table, Transfer, TransferProps, message } from 'antd'
import type { ColumnsType, TableRowSelection } from 'antd/es/table/interface'
import { difference } from 'lodash'
import { useEffect, useState } from 'react'

export type TransferDirection = 'left' | 'right'

export interface ManagePlanningIntervalObjectiveWorkItemsFormProps {
    showForm: boolean
    id: string
    onFormSave: () => void
    onFormCancel: () => void
}

interface TableTransferProps extends TransferProps<WorkItemListDto> {
    leftColumns: ColumnsType<WorkItemListDto>
    rightColumns: ColumnsType<WorkItemListDto>
}

const TableTransfer = ({
    leftColumns,
    rightColumns,
    ...restProps
}: TableTransferProps) => (
    <Transfer oneWay {...restProps}>
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
    {
        dataIndex: 'type',
        title: 'Type',
    },
    {
        dataIndex: 'status',
        title: 'Status',
    },
]

const ManagePlanningIntervalObjectiveWorkItemsForm = (
    props: ManagePlanningIntervalObjectiveWorkItemsFormProps,
) => {
    const [isOpen, setIsOpen] = useState(props.showForm)
    const [isSaving, setIsSaving] = useState(false)
    const [workItems, setWorkItems] = useState<WorkItemListDto[]>([])
    const [targetKeys, setTargetKeys] = useState<string[]>([])
    const [messageApi, contextHolder] = message.useMessage()

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

    const merge = (searchResult: WorkItemListDto[], workItems: WorkItemListDto[]) : WorkItemListDto[] => {
        const spread = [...searchResult ?? []]
        workItems.forEach((item) => {
            const found = searchResult.find((x) => x.key === item.key)
            if(!found) spread.push(item)
        })
        return spread;
    }

    // useEffect(() => {
    //   if (!props.id) return
    //   setSearchQuery('AHTG')
    // }, [props.id])

    // useEffect(() => {
    //   console.log('searchResult:', searchResult?.length)
    //   setWorkItems(searchResult ?? [])
    // }, [searchQuery, searchResult])

    const handleOk = async () => {
        setIsSaving(true)
        try {
            if (true) {
                // add api call to save
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

    const handleSearch = (direction: TransferDirection, newValue: string) => {
        console.log('handleSearch', direction, newValue)
        if (direction === 'left') {
            setSearchQuery(newValue)
        }
    }

    const onChange = (nextTargetKeys: string[]) => {
        var workItems = searchResult?.filter((item) => nextTargetKeys.includes(item.key))
        setWorkItems(workItems)
        setTargetKeys(nextTargetKeys)
    }

    return (
        <>
            {contextHolder}
            <Modal
                title="Manage PI Objective Work Items"
                open={isOpen}
                width={900}
                onOk={handleOk}
                okText="Save"
                confirmLoading={isSaving}
                onCancel={handleCancel}
                maskClosable={false}
                keyboard={false} // disable esc key to close modal
                destroyOnClose={true}
            >
                {
                    <Spin spinning={isLoading} size="large">
                        <TableTransfer
                            dataSource={merge(searchResult, workItems)}
                            targetKeys={targetKeys}
                            showSearch={true}
                            onChange={onChange}
                            onSearch={handleSearch}
                            filterOption={(inputValue, item, direction) =>
                                direction === 'left'
                            }
                            leftColumns={tableColumns}
                            rightColumns={tableColumns}
                        />
                    </Spin>
                }
            </Modal>
        </>
    )
}

export default ManagePlanningIntervalObjectiveWorkItemsForm
