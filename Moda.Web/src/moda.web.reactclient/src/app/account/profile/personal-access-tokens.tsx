'use client'

import { useState, useMemo, useCallback, FC } from 'react'
import { Button, Tag, Alert, Flex, Typography, App, Space } from 'antd'
import { PlusOutlined } from '@ant-design/icons'
import {
  useGetMyPersonalAccessTokensQuery,
  useRevokePersonalAccessTokenMutation,
  useDeletePersonalAccessTokenMutation,
} from '@/src/store/features/user-management/personal-access-tokens-api'
import { PersonalAccessTokenDto } from '@/src/services/moda-api'
import dayjs from 'dayjs'
import ModaGrid from '@/src/components/common/moda-grid'
import { CustomCellRendererProps } from 'ag-grid-react'
import { ColDef } from 'ag-grid-community'
import {
  CreatePersonalAccessTokenForm,
  EditPersonalAccessTokenForm,
  PersonalAccessTokenCreatedModal,
} from './_components'
import { useMessage } from '@/src/components/contexts/messaging'
import { RowMenuCellRenderer } from '@/src/components/common/moda-grid-cell-renderers'
import { ItemType } from 'antd/es/menu/interface'
import { MenuProps } from 'antd'

const { Text } = Typography

// Custom cell renderers
const StatusCellRenderer = (
  props: CustomCellRendererProps<PersonalAccessTokenDto>,
) => {
  const { data } = props
  if (!data) return null

  if (data.isRevoked) {
    return <Tag color="error">Revoked</Tag>
  }
  if (data.isExpired) {
    return <Tag color="warning">Expired</Tag>
  }
  if (data.isActive) {
    return <Tag color="success">Active</Tag>
  }
  return <Tag>Unknown</Tag>
}

const DateTimeCellRenderer = (props: CustomCellRendererProps) => {
  const { value } = props
  if (!value) return 'Never'
  return dayjs(value).format('YYYY-MM-DD h:mm A')
}

interface RowMenuProps extends MenuProps {
  tokenId: string
  tokenName: string
  isActive: boolean
  token: PersonalAccessTokenDto
  modal: ReturnType<typeof App.useApp>['modal']
  onEditTokenMenuClicked: (token: PersonalAccessTokenDto) => void
  onRevokeTokenMenuClicked: (id: string, name: string) => void
  onDeleteTokenMenuClicked: (id: string, name: string) => void
}

const getRowMenuItems = (props: RowMenuProps) => {
  if (!props.tokenId) return null

  const items: ItemType[] = []

  if (props.isActive) {
    items.push({
      key: 'editToken',
      label: 'Edit',
      onClick: () => props.onEditTokenMenuClicked(props.token),
    })
    items.push({
      key: 'revokeToken',
      label: 'Revoke',
      onClick: () => {
        props.modal.confirm({
          title: `Revoke Token - ${props.tokenName}`,
          content:
            'Are you sure you want to revoke this token? It will no longer work.',
          okText: 'Revoke',
          okType: 'danger',
          onOk: () =>
            props.onRevokeTokenMenuClicked(props.tokenId, props.tokenName),
        })
      },
    })
  }

  items.push({
    key: 'deleteToken',
    label: 'Delete',
    onClick: () => {
      props.modal.confirm({
        title: `Delete Token - ${props.tokenName}`,
        content:
          'Are you sure you want to permanently delete this token? This cannot be undone.',
        okText: 'Delete',
        okType: 'danger',
        onOk: () =>
          props.onDeleteTokenMenuClicked(props.tokenId, props.tokenName),
      })
    },
  })

  return items
}

const PersonalAccessTokens: FC = () => {
  const [isCreateFormVisible, setIsCreateFormVisible] = useState(false)
  const [isEditFormVisible, setIsEditFormVisible] = useState(false)
  const [editingToken, setEditingToken] =
    useState<PersonalAccessTokenDto | null>(null)
  const [newToken, setNewToken] = useState<string | null>(null)
  const messageApi = useMessage()
  const { modal } = App.useApp()

  const {
    data: tokens,
    isLoading,
    error,
    refetch,
  } = useGetMyPersonalAccessTokensQuery()
  const [revokeToken] = useRevokePersonalAccessTokenMutation()
  const [deleteToken] = useDeletePersonalAccessTokenMutation()

  const handleEdit = useCallback((token: PersonalAccessTokenDto) => {
    setEditingToken(token)
    setIsEditFormVisible(true)
  }, [])

  const handleRevoke = useCallback(
    async (id: string, name: string) => {
      try {
        await revokeToken(id).unwrap()
        messageApi.success(`Token "${name}" revoked successfully`)
      } catch (error) {
        console.error('Failed to revoke token:', error)
        messageApi.error('Failed to revoke token')
      }
    },
    [revokeToken, messageApi],
  )

  const handleDelete = useCallback(
    async (id: string, name: string) => {
      try {
        await deleteToken(id).unwrap()
        messageApi.success(`Token "${name}" deleted successfully`)
      } catch (error) {
        console.error('Failed to delete token:', error)
        messageApi.error('Failed to delete token')
      }
    },
    [deleteToken, messageApi],
  )

  const handleFormCreate = (token: string) => {
    setNewToken(token)
  }

  const handleFormCancel = () => {
    setIsCreateFormVisible(false)
  }

  const handleEditFormUpdate = () => {
    setIsEditFormVisible(false)
    setEditingToken(null)
  }

  const handleEditFormCancel = () => {
    setIsEditFormVisible(false)
    setEditingToken(null)
  }

  const handleTokenModalClose = () => {
    setNewToken(null)
    setIsCreateFormVisible(false)
  }

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const columnDefs = useMemo<ColDef<PersonalAccessTokenDto>[]>(
    () => [
      {
        width: 50,
        filter: false,
        sortable: false,
        resizable: false,
        cellRenderer: (params) => {
          const menuItems = getRowMenuItems({
            tokenId: params.data?.id ?? '',
            tokenName: params.data?.name ?? '',
            isActive: params.data?.isActive ?? false,
            token: params.data!,
            modal,
            onEditTokenMenuClicked: handleEdit,
            onRevokeTokenMenuClicked: handleRevoke,
            onDeleteTokenMenuClicked: handleDelete,
          })
          return RowMenuCellRenderer({ ...params, menuItems })
        },
      },
      {
        field: 'name',
        headerName: 'Name',
        flex: 1,
        minWidth: 200,
      },
      {
        headerName: 'Status',
        cellRenderer: StatusCellRenderer,
        width: 120,
      },
      {
        field: 'expiresAt',
        headerName: 'Expires',
        cellRenderer: DateTimeCellRenderer,
        width: 180,
      },
      {
        field: 'lastUsedAt',
        headerName: 'Last Used',
        cellRenderer: DateTimeCellRenderer,
        width: 180,
        headerTooltip:
          'The last time this token was used for authentication. Updates at most once per hour.',
      },
    ],
    [handleEdit, handleRevoke, handleDelete, modal],
  )

  if (error) {
    return (
      <Alert
        message="Error"
        description="Failed to load personal access tokens"
        type="error"
        showIcon
      />
    )
  }

  return (
    <Flex vertical gap={16}>
      <Space direction="vertical" size={16}>
        <Alert
          message="Personal Access Tokens (PATs)"
          description="Personal access tokens function like passwords for API authentication. Keep them secure and never share them. Each user can have up to 10 active tokens."
          type="info"
          showIcon
        />

        <Text type="secondary">
          Use this token in the <code>x-api-key</code> header when making API
          requests.
        </Text>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => setIsCreateFormVisible(true)}
        >
          Create New Token
        </Button>
      </Space>

      <ModaGrid
        height={500}
        columnDefs={columnDefs}
        rowData={tokens}
        loadData={refresh}
        loading={isLoading}
        emptyMessage="No PATs found."
      />

      {isCreateFormVisible && (
        <CreatePersonalAccessTokenForm
          showForm={isCreateFormVisible}
          onFormCreate={handleFormCreate}
          onFormCancel={handleFormCancel}
        />
      )}

      {isEditFormVisible && editingToken && (
        <EditPersonalAccessTokenForm
          token={editingToken}
          showForm={isEditFormVisible}
          onFormUpdate={handleEditFormUpdate}
          onFormCancel={handleEditFormCancel}
        />
      )}

      {newToken && (
        <PersonalAccessTokenCreatedModal
          token={newToken}
          onClose={handleTokenModalClose}
        />
      )}
    </Flex>
  )
}

export default PersonalAccessTokens
