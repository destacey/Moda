'use client'

import { useConfirmModal } from '@/src/hooks'
import { TeamListItem } from '@/src/app/organizations/types'
import {
  getPlanningIntervalsClient,
  getTeamsClient,
  getTeamsOfTeamsClient,
} from '@/src/services/clients'
import { Modal, Spin, Table, Transfer, TransferProps } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import type { Key } from 'react'
import difference from 'lodash/difference'
import {
  ManagePlanningIntervalTeamsRequest,
  PlanningIntervalTeamResponse,
} from '@/src/services/wayd-api'
import type { ColumnsType, TableRowSelection } from 'antd/es/table/interface'
import { useMessage } from '@/src/components/contexts/messaging'

export interface ManagePlanningIntervalTeamsFormProps {
  id: string
  onFormSave: () => void
  onFormCancel: () => void
}

interface PlanningIntervalTeamModel {
  key: string
  name: string
  code: string
  disabled: boolean
  teamOfTeams: string | undefined
}

interface TableTransferProps extends TransferProps<PlanningIntervalTeamModel> {
  dataSource: PlanningIntervalTeamModel[]
  leftColumns: ColumnsType<PlanningIntervalTeamModel>
  rightColumns: ColumnsType<PlanningIntervalTeamModel>
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

      const rowSelection: TableRowSelection<PlanningIntervalTeamModel> = {
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
          style={{ pointerEvents: listDisabled ? 'none' : undefined }}
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

const tableColumns: ColumnsType<PlanningIntervalTeamModel> = [
  {
    dataIndex: 'name',
    title: 'Name',
  },
  {
    dataIndex: 'code',
    title: 'Code',
  },
  {
    dataIndex: 'teamOfTeams',
    title: 'Team of Teams',
  },
]

const ManagePlanningIntervalTeamsForm = ({
  id,
  onFormSave,
  onFormCancel,
}: ManagePlanningIntervalTeamsFormProps) => {
  const [isLoading, setIsLoading] = useState(false)
  const [teams, setTeams] = useState<PlanningIntervalTeamModel[]>([])
  const [targetKeys, setTargetKeys] = useState<Key[]>([])
  const messageApi = useMessage()

  // TODO: should this be in a custom hook? The teams index page has a similar call.
  const getTeams = useCallback(async () => {
    const [teamsDtos, teamOfTeamsDtos] = await Promise.all([
      getTeamsClient().getList(false),
      getTeamsOfTeamsClient().getList(false),
    ])

    return [...teamsDtos, ...teamOfTeamsDtos].map((team: TeamListItem) => ({
      key: team.id,
      name: team.name,
      code: team.code ?? '',
      disabled: false,
      teamOfTeams: team.teamOfTeams?.name,
    }))
  }, [])

  const getPlanningIntervalTeams = useCallback(async (id: string) => {
    const piTeamsDtos = await getPlanningIntervalsClient().getTeams(id)

    return piTeamsDtos.map((team: PlanningIntervalTeamResponse) => ({
      key: team.id,
      name: team.name,
      code: team.code,
      disabled: false,
      teamOfTeams: team.teamOfTeams?.name,
    }))
  }, [])

  const { isOpen, isSaving, handleOk, handleCancel } = useConfirmModal({
    onSubmit: async () => {
      try {
        const request: ManagePlanningIntervalTeamsRequest = {
          id: id,
          teamIds: targetKeys as string[],
        }
        await getPlanningIntervalsClient().manageTeams(id, request)

        messageApi.success(`Successfully updated PI teams.`)
        return true
      } catch (error) {
        messageApi.error(`An unexpected error occurred while saving.`)
        console.error(error)
        return false
      }
    },
    onComplete: onFormSave,
    onCancel: onFormCancel,
    errorMessage:
      'An error occurred while updating PI teams. Please try again.',
    permission: 'Permissions.PlanningIntervals.Update',
  })

  useEffect(() => {
    if (!isOpen) return
    let cancelled = false

    const loadData = async () => {
      try {
        setIsLoading(true)
        const [teamsData, piTeamsData] = await Promise.all([
          getTeams(),
          getPlanningIntervalTeams(id),
        ])
        if (!cancelled) {
          setTeams(teamsData)
          setTargetKeys(piTeamsData.map((team) => team.key))
        }
      } catch (error) {
        if (!cancelled) {
          messageApi.error(
            `An unexpected error occurred while retrieving PI teams.`,
          )
          console.error(error)
        }
      }
      if (!cancelled) {
        setIsLoading(false)
      }
    }

    loadData()
    return () => {
      cancelled = true
    }
  }, [isOpen, id, getTeams, getPlanningIntervalTeams, messageApi])

  const onChange: TransferProps<PlanningIntervalTeamModel>['onChange'] = (nextTargetKeys) => {
    setTargetKeys(nextTargetKeys)
  }

  return (
    <Modal
      title="Manage Planning Interval Teams"
      open={isOpen}
      width={900}
      onOk={handleOk}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
    >
      {
        <Spin spinning={isLoading} size="large">
          <TableTransfer
            dataSource={teams}
            targetKeys={targetKeys}
            showSearch={true}
            onChange={onChange}
            filterOption={(inputValue, item: PlanningIntervalTeamModel) =>
              item.name!.toLowerCase().indexOf(inputValue.toLowerCase()) !==
                -1 ||
              item.code!.toLowerCase().indexOf(inputValue.toLowerCase()) !==
                -1 ||
              !!(
                item.teamOfTeams &&
                item.teamOfTeams
                  .toLowerCase()
                  .indexOf(inputValue.toLowerCase()) !== -1
              )
            }
            leftColumns={tableColumns}
            rightColumns={tableColumns}
          />
        </Spin>
      }
    </Modal>
  )
}

export default ManagePlanningIntervalTeamsForm
