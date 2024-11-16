import { useGetLinks } from '@/src/services/queries/link-queries'
import { Button, Card, Space, Spin } from 'antd'
import useAuth from '../../contexts/auth'
import { EditOutlined, EditTwoTone, PlusOutlined } from '@ant-design/icons'
import { useState } from 'react'
import CreateLinkForm from './create-link-form'
import LinkItem from './link-item'
import ModaEmpty from '../moda-empty'

export interface LinksCardProps {
  objectId: string
}

const LinksCard = ({ objectId }: LinksCardProps) => {
  const [openCreateLinkForm, setOpenCreateLinkForm] = useState<boolean>(false)
  const [editModeEnabled, setEditModeEnabled] = useState<boolean>(false)

  const { data: linksData, isLoading } = useGetLinks(objectId)
  const hasLinks = linksData && linksData.length > 0

  const { hasClaim } = useAuth()
  const canCreateLinks = hasClaim('Permission', 'Permissions.Links.Create')
  const canUpdateLinks = hasClaim('Permission', 'Permissions.Links.Update')
  const canDeleteLinks = hasClaim('Permission', 'Permissions.Links.Delete')

  const EditModeButton = () => {
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

  const LinksContent = () => {
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
              <EditModeButton />
            )}
          </>
        }
      >
        <LinksContent />
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
