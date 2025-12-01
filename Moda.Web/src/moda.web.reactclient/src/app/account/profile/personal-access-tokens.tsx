'use client'

import React, { useState, useMemo, useCallback, FC } from 'react'
import {
  Button,
  Space,
  Tag,
  Popconfirm,
  Alert,
  message as antdMessage,
  Flex,
} from 'antd'
import { PlusOutlined, DeleteOutlined, StopOutlined } from '@ant-design/icons'
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
  PersonalAccessTokenCreatedModal,
} from './_components'
import { useMessage } from '@/src/components/contexts/messaging'

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

interface ActionsCellRendererProps
  extends CustomCellRendererProps<PersonalAccessTokenDto> {
  onRevoke: (id: string, name: string) => void
  onDelete: (id: string, name: string) => void
}

const ActionsCellRenderer = (props: ActionsCellRendererProps) => {
  const { data, onRevoke, onDelete } = props
  if (!data) return null

  return (
    <Space size="small">
      {data.isActive && (
        <Popconfirm
          title="Revoke Token"
          description="Are you sure you want to revoke this token? It will no longer work."
          onConfirm={() => onRevoke(data.id!, data.name!)}
          okText="Revoke"
          cancelText="Cancel"
        >
          <Button type="link" icon={<StopOutlined />} size="small" danger>
            Revoke
          </Button>
        </Popconfirm>
      )}
      <Popconfirm
        title="Delete Token"
        description="Are you sure you want to permanently delete this token? This cannot be undone."
        onConfirm={() => onDelete(data.id!, data.name!)}
        okText="Delete"
        cancelText="Cancel"
      >
        <Button type="link" icon={<DeleteOutlined />} size="small" danger>
          Delete
        </Button>
      </Popconfirm>
    </Space>
  )
}

const PersonalAccessTokens: FC = () => {
  const [isCreateFormVisible, setIsCreateFormVisible] = useState(false)
  const [newToken, setNewToken] = useState<string | null>(null)
  const messageApi = useMessage()

  const {
    data: tokens,
    isLoading,
    error,
    refetch,
  } = useGetMyPersonalAccessTokensQuery()
  const [revokeToken] = useRevokePersonalAccessTokenMutation()
  const [deleteToken] = useDeletePersonalAccessTokenMutation()

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
      },
      {
        headerName: 'Actions',
        cellRenderer: ActionsCellRenderer,
        cellRendererParams: {
          onRevoke: handleRevoke,
          onDelete: handleDelete,
        },
        width: 200,
        sortable: false,
        filter: false,
      },
    ],
    [handleRevoke, handleDelete],
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
      <Space direction="vertical">
        <Alert
          message="Personal Access Tokens (PATs)"
          description="Personal access tokens function like passwords for API authentication. Keep them secure and never share them."
          type="info"
          showIcon
        />
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
