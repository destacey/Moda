import { LinkDto } from '@/src/services/moda-api'
import { DeleteOutlined, EditOutlined } from '@ant-design/icons'
import { Button, Popconfirm, Space, message } from 'antd'
import Link from 'next/link'
import { useState } from 'react'
import EditLinkForm from './edit-link-form'
import {
  StoreDeleteLinkRequest,
  useDeleteLinkMutation,
} from '@/src/store/features/common/links-api'

export interface LinkItemProps {
  link: LinkDto
  editModeEnabled: boolean
  canUpdateLinks: boolean
  canDeleteLinks: boolean
}

const LinkItem = ({
  link,
  editModeEnabled,
  canUpdateLinks,
  canDeleteLinks,
}: LinkItemProps) => {
  const [openEditLinkForm, setOpenEditLinkForm] = useState<boolean>(false)
  const [showDeleteConfirm, setShowDeleteConfirm] = useState<boolean>(false)
  const [deleteConfirmLoading, setDeleteConfirmLoading] =
    useState<boolean>(false)
  const [messageApi, contextHolder] = message.useMessage()

  const [deleteLinkMutation, { error: deleteLinkError }] =
    useDeleteLinkMutation()

  if (!link) return null

  const deleteLink = async (id: string, objectId: string) => {
    try {
      const storeRequest: StoreDeleteLinkRequest = {
        id: link.id,
        objectId: link.objectId,
      }
      const response = await deleteLinkMutation(storeRequest)
      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      messageApi.error('An unexpected error occurred while deleting the Link.')
      console.log(error)
      return false
    }
  }

  const handleDeleteConfirm = async () => {
    setDeleteConfirmLoading(true)
    try {
      if (await deleteLink(link.id, link.objectId)) {
        messageApi.success('Successfully deleted Link.')
      }
    } catch (errorInfo) {
      messageApi.error('An unexpected error occurred while deleting the Link.')
      console.log('handleOk error', errorInfo)
    } finally {
      setDeleteConfirmLoading(false)
    }
  }

  const handleDeleteCancel = () => {
    setShowDeleteConfirm(false)
  }

  // without this, pages without http or https will open as a relative path
  const url = link.url.match(/^.{3,5}\:\/\//) ? link.url : `//${link.url}`

  const ExternalLink = () => {
    return (
      <Link href={url} target="_blank" prefetch={false}>
        {link.name}
      </Link>
    )
  }

  if (!editModeEnabled) {
    return <ExternalLink />
  } else {
    return (
      <>
        {contextHolder}
        <Space
          style={{
            display: 'flex',
            justifyContent: 'space-between',
          }}
        >
          <ExternalLink />
          <Space>
            {canUpdateLinks && (
              <Button
                type="text"
                icon={<EditOutlined style={{ width: '14px' }} />}
                onClick={() => setOpenEditLinkForm(true)}
              />
            )}
            {canDeleteLinks && (
              <Popconfirm
                title="Delete the Link"
                description="Are you sure you want to delete this Link?"
                open={showDeleteConfirm}
                onConfirm={handleDeleteConfirm}
                okButtonProps={{ loading: deleteConfirmLoading }}
                onCancel={handleDeleteCancel}
                okText="Yes"
                cancelText="No"
              >
                <Button
                  type="text"
                  icon={<DeleteOutlined style={{ width: '14px' }} />}
                  onClick={() => setShowDeleteConfirm(true)}
                />
              </Popconfirm>
            )}
          </Space>
        </Space>
        {openEditLinkForm && (
          <EditLinkForm
            id={link.id}
            showForm={openEditLinkForm}
            onFormUpdate={() => setOpenEditLinkForm(false)}
            onFormCancel={() => setOpenEditLinkForm(false)}
          />
        )}
      </>
    )
  }
}

export default LinkItem
