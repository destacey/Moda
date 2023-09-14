import { useGetLinks } from '@/src/services/queries/link-queries'
import { Button, Card, Space } from 'antd'
import useAuth from '../../contexts/auth'
import { PlusOutlined } from '@ant-design/icons'
import { useState } from 'react'
import Link from 'next/link'

export interface LinksCardProps {
  objectId: string
}

const LinksCard = ({ objectId }: LinksCardProps) => {
  const [openCreateLinkForm, setOpenCreateLinkForm] = useState<boolean>(false)
  const [openEditLinkForm, setOpenEditLinkForm] = useState<boolean>(false)

  const { data: linksData, isLoading } = useGetLinks(objectId)

  const { hasClaim } = useAuth()
  const canCreateLinks = hasClaim('Permission', 'Permissions.Links.Create')
  const canUpdateLinks = hasClaim('Permission', 'Permissions.Links.Update')

  // if (!linksData) return null

  return (
    <Card
      size="small"
      title="Links"
      style={{ width: 300 }}
      extra={
        canCreateLinks && (
          <Button
            type="text"
            icon={<PlusOutlined />}
            onClick={() => setOpenCreateLinkForm(true)}
          />
        )
      }
    >
      <Space direction="vertical">
        <Link href={'https://www.google.com'} target="_blank">
          Team Board
        </Link>
        <Link href={'https://www.google.com'} target="_blank">
          Team Docs
        </Link>
        <Link href={'https://www.google.com'} target="_blank">
          Product Docs Product Docs Product Docs Product Docs Product Docs
        </Link>
      </Space>
    </Card>
  )
}

export default LinksCard
