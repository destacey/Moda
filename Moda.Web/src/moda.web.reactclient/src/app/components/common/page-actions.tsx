import { DownOutlined } from '@ant-design/icons'
import { Button, Dropdown, MenuProps, Space } from 'antd'
import { ItemType } from 'antd/es/menu/hooks/useItems'

export interface PageActionsProps {
  actionItems: MenuProps['items'] | ItemType[]
  menuTitle?: string
  placement?:
    | 'bottomLeft'
    | 'bottomCenter'
    | 'bottomRight'
    | 'topLeft'
    | 'topCenter'
    | 'topRight'
}

const PageActions = ({
  actionItems,
  menuTitle = 'Actions',
  placement = 'bottomRight',
}: PageActionsProps) => {
  if (!actionItems || actionItems.length === 0) return null
  return (
    <Dropdown placement={placement} menu={{ items: actionItems }}>
      <Button>
        <Space>
          {menuTitle}
          <DownOutlined />
        </Space>
      </Button>
    </Dropdown>
  )
}

export default PageActions
