import { LinkDto } from '@/src/services/moda-api'
import { DeleteOutlined, EditOutlined } from '@ant-design/icons'
import { Button, Space } from 'antd'
import Link from 'next/link'
import { useState } from 'react'
import EditLinkForm from './edit-link-form'

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
  if (!link) return null

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
        <Space
          style={{
            width: '100%',
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
              <Button
                type="text"
                icon={<DeleteOutlined style={{ width: '14px' }} />}
                onClick={() => setOpenEditLinkForm(true)}
              />
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
