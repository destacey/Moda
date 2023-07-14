import useAuth from '@/src/app/components/contexts/auth'
import { TeamListItem } from '@/src/app/organizations/types'
import {
  getProgramIncrementsClient,
  getTeamsClient,
  getTeamsOfTeamsClient,
} from '@/src/services/clients'
import { Modal, Spin, TransferProps, message } from 'antd'
import Table, { ColumnsType } from 'antd/es/table'
import { TableRowSelection } from 'antd/es/table/interface'
import Transfer, { TransferItem } from 'antd/es/transfer'
import { useCallback, useEffect, useState } from 'react'
import difference from 'lodash/difference'
import {
  ManageProgramIncrementTeamsRequest,
  ProgramIncrementTeamReponse,
} from '@/src/services/moda-api'

export interface ManageProgramIncrementTeamsFormProps {
  showForm: boolean
  id: string
  onFormSave: () => void
  onFormCancel: () => void
}

interface ProgramIncrementTeamModel {
  key: string
  name: string
  code: string
  disabled: boolean
  teamOfTeams: string
}

interface TableTransferProps extends TransferProps<TransferItem> {
  dataSource: ProgramIncrementTeamModel[]
  leftColumns: ColumnsType<ProgramIncrementTeamModel>
  rightColumns: ColumnsType<ProgramIncrementTeamModel>
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

      const rowSelection: TableRowSelection<TransferItem> = {
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
                !listSelectedKeys.includes(key as string)
              )
            },
          })}
        />
      )
    }}
  </Transfer>
)

const tableColumns: ColumnsType<ProgramIncrementTeamModel> = [
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

const ManageProgramIncrementTeamsForm = ({
  showForm,
  id,
  onFormSave,
  onFormCancel,
}: ManageProgramIncrementTeamsFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [teams, setTeams] = useState<ProgramIncrementTeamModel[]>([])
  const [targetKeys, setTargetKeys] = useState<string[]>([])
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canUpdatePI = hasClaim(
    'Permission',
    'Permissions.ProgramIncrements.Update'
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

  const getProgramIncrementTeams = useCallback(async (id: string) => {
    const piClient = await getProgramIncrementsClient()
    const piTeamsDtos = await piClient.getTeams(id)

    return piTeamsDtos.map((team: ProgramIncrementTeamReponse) => ({
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
        const piTeamsData = await getProgramIncrementTeams(id)
        setTargetKeys(piTeamsData.map((team) => team.key))
      } catch (error) {
        messageApi.error(
          `An unexpected error occurred while retrieving PI teams.`
        )
        console.error(error)
      }
      setIsLoading(false)
    },
    [getProgramIncrementTeams, getTeams, messageApi]
  )

  useEffect(() => {
    if (canUpdatePI) {
      setIsOpen(showForm)
      if (showForm) {
        loadData(id)
      }
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to update teams.')
    }
  }, [canUpdatePI, id, loadData, messageApi, onFormCancel, showForm])

  const savePITeams = async (): Promise<boolean> => {
    try {
      const piClient = await getProgramIncrementsClient()
      const request: ManageProgramIncrementTeamsRequest = {
        id: id,
        teamIds: targetKeys,
      }
      console.log('team count', request.teamIds.length)
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
        setIsSaving(false)
        onFormSave()
        messageApi.success(`Successfully updated PI teams.`)
      } else {
        setIsSaving(false)
      }
    } catch (errorInfo) {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
  }

  const onChange = (nextTargetKeys: string[]) => {
    console.log('onChange', nextTargetKeys)
    setTargetKeys(nextTargetKeys)
  }

  return (
    <>
      {contextHolder}
      <Modal
        title="Manage Program Increment Teams"
        open={isOpen}
        width={900}
        onOk={handleOk}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        closable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        {
          <Spin spinning={isLoading} size="large">
            <TableTransfer
              dataSource={teams}
              targetKeys={targetKeys}
              showSearch={true}
              onChange={onChange}
              filterOption={(inputValue, item: ProgramIncrementTeamModel) =>
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
    </>
  )
}

export default ManageProgramIncrementTeamsForm
