import { Button, Card, Space, Spin } from 'antd'
import useAuth from '../../contexts/auth'
import { EditOutlined, EditTwoTone, PlusOutlined } from '@ant-design/icons'
import { useState } from 'react'
import CreateLinkForm from './create-link-form'
import LinkItem from './link-item'
import ModaEmpty from '../moda-empty'
import { useGetLinksQuery } from '@/src/store/features/common/links-api'

export interface LinksCardProps {
  objectId: string
}

const EditModeButton = ({
  editModeEnabled,
  setEditModeEnabled,
}: {
  editModeEnabled: boolean
  setEditModeEnabled: (enabled: boolean) => void
}) => {
  if (editModeEnabled) {
    return (
      <Button
        type="text"
        title="Click to turn off Edit mode"
        icon={<EditTwoTone />}
        onClick={() => setEditModeEnabled(false)}
      />
    )
  } else {
    return (
      <Button
        type="text"
        title="Click to turn on Edit mode"
        icon={<EditOutlined />}
        onClick={() => setEditModeEnabled(true)}
      />
    )
  }
}

const LinksContent = ({
  isLoading,
  hasLinks,
  linksData,
  editModeEnabled,
  canUpdateLinks,
  canDeleteLinks,
}: {
  isLoading: boolean
  hasLinks: boolean
  linksData: any[]
  editModeEnabled: boolean
  canUpdateLinks: boolean
  canDeleteLinks: boolean
}) => {
  if (isLoading) {
    return <Spin size="small" />
  } else if (!hasLinks) {
    return <ModaEmpty message="No links found" />
  } else {
    return (
      <Space
        direction="vertical"
        style={{
          width: '100%',
        }}
      >
        {linksData
          .slice()
          .sort((a, b) => a.name.localeCompare(b.name))
          .map((item) => (
            <LinkItem
              key={item.id}
              link={item}
              editModeEnabled={editModeEnabled}
              canUpdateLinks={canUpdateLinks}
              canDeleteLinks={canDeleteLinks}
            />
          ))}
      </Space>
    )
  }
}

const LinksCard = ({ objectId }: LinksCardProps) => {
  const [openCreateLinkForm, setOpenCreateLinkForm] = useState<boolean>(false)
  const [editModeEnabled, setEditModeEnabled] = useState<boolean>(false)

  const {
    data: linksData,
    isLoading,
    error,
    refetch,
  } = useGetLinksQuery(objectId, { skip: !objectId })

  const hasLinks = linksData && linksData.length > 0

  const { hasPermissionClaim } = useAuth()
  const canCreateLinks = hasPermissionClaim('Permissions.Links.Create')
  const canUpdateLinks = hasPermissionClaim('Permissions.Links.Update')
  const canDeleteLinks = hasPermissionClaim('Permissions.Links.Delete')

  return (
    <>
      <Card
        size="small"
        title="Links"
        style={{ width: 300 }}
        extra={
          <>
            {canCreateLinks && (
              <Button
                type="text"
                icon={<PlusOutlined />}
                onClick={() => setOpenCreateLinkForm(true)}
              />
            )}
            {hasLinks && (canUpdateLinks || canDeleteLinks) && (
              <EditModeButton
                editModeEnabled={editModeEnabled}
                setEditModeEnabled={setEditModeEnabled}
              />
            )}
          </>
        }
      >
        <LinksContent
          isLoading={isLoading}
          hasLinks={hasLinks}
          linksData={linksData || []}
          editModeEnabled={editModeEnabled}
          canUpdateLinks={canUpdateLinks}
          canDeleteLinks={canDeleteLinks}
        />
      </Card>
      {openCreateLinkForm && (
        <CreateLinkForm
          objectId={objectId}
          showForm={openCreateLinkForm}
          onFormCreate={() => setOpenCreateLinkForm(false)}
          onFormCancel={() => setOpenCreateLinkForm(false)}
        />
      )}
    </>
  )
}

export default LinksCard
