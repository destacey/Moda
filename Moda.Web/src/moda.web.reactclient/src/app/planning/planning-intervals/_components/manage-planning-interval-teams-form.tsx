'use client'

import useAuth from '@/src/components/contexts/auth'
import { TeamListItem } from '@/src/app/organizations/types'
import {
  getPlanningIntervalsClient,
  getTeamsClient,
  getTeamsOfTeamsClient,
} from '@/src/services/clients'
import { Modal, Spin, Table, Transfer, TransferProps } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import difference from 'lodash/difference'
import {
  ManagePlanningIntervalTeamsRequest,
  PlanningIntervalTeamResponse,
} from '@/src/services/moda-api'
import type { ColumnsType, TableRowSelection } from 'antd/es/table/interface'
import { useMessage } from '@/src/components/contexts/messaging'

export interface ManagePlanningIntervalTeamsFormProps {
  showForm: boolean
  id: string
  onFormSave: () => void
  onFormCancel: () => void
}

interface PlanningIntervalTeamModel {
  key: string
  name: string
  code: string
  disabled: boolean
  teamOfTeams: string
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
  showForm,
  id,
  onFormSave,
  onFormCancel,
}: ManagePlanningIntervalTeamsFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [teams, setTeams] = useState<PlanningIntervalTeamModel[]>([])
  const [targetKeys, setTargetKeys] = useState<string[]>([])
  const messageApi = useMessage()

  const { hasClaim } = useAuth()
  const canUpdatePI = hasClaim(
    'Permission',
    'Permissions.PlanningIntervals.Update',
  )

  // TODO: should this be in a custom hook? The teams index page has a similar call.
  const getTeams = useCallback(async () => {
    const teamsClient = await getTeamsClient()
    const teamsDtos = await teamsClient.getList(false)

    const teamOfTeamsClient = await getTeamsOfTeamsClient()
    const teamOfTeamsDtos = await teamOfTeamsClient.getList(false)

    return [...teamsDtos, ...teamOfTeamsDtos].map((team: TeamListItem) => ({
      key: team.id,
      name: team.name,
      code: team.code,
      disabled: false,
      teamOfTeams: team.teamOfTeams?.name,
    }))
  }, [])

  const getPlanningIntervalTeams = useCallback(async (id: string) => {
    const piClient = await getPlanningIntervalsClient()
    const piTeamsDtos = await piClient.getTeams(id)

    return piTeamsDtos.map((team: PlanningIntervalTeamResponse) => ({
      key: team.id,
      name: team.name,
      code: team.code,
      disabled: false,
      teamOfTeams: team.teamOfTeams?.name,
    }))
  }, [])

  const loadData = useCallback(
    async (id: string) => {
      try {
        setIsLoading(true)
        setTeams(await getTeams())
        const piTeamsData = await getPlanningIntervalTeams(id)
        setTargetKeys(piTeamsData.map((team) => team.key))
      } catch (error) {
        messageApi.error(
          `An unexpected error occurred while retrieving PI teams.`,
        )
        console.error(error)
      }
      setIsLoading(false)
    },
    [getPlanningIntervalTeams, getTeams, messageApi],
  )

  useEffect(() => {
    if (canUpdatePI) {
      setIsOpen(showForm)
      if (showForm) {
        loadData(id)
      }
    } else {
      onFormCancel()
      messageApi.error(
        'You do not have permission to update the Planning Interval.',
      )
    }
  }, [canUpdatePI, id, loadData, messageApi, onFormCancel, showForm])

  const savePITeams = async (): Promise<boolean> => {
    try {
      const piClient = await getPlanningIntervalsClient()
      const request: ManagePlanningIntervalTeamsRequest = {
        id: id,
        teamIds: targetKeys,
      }
      await piClient.manageTeams(id, request)

      return true
    } catch (error) {
      messageApi.error(`An unexpected error occurred while saving.`)
      console.error(error)

      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await savePITeams()) {
        setIsOpen(false)
        onFormSave()
        messageApi.success(`Successfully updated PI teams.`)
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while updating PI teams. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
  }

  const onChange = (nextTargetKeys: string[]) => {
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
      keyboard={false} // disable esc key to close modal
      destroyOnHidden={true}
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
              (item.teamOfTeams &&
                item.teamOfTeams
                  .toLowerCase()
                  .indexOf(inputValue.toLowerCase()) !== -1)
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
