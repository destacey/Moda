import { MenuOutlined } from '@ant-design/icons'
import { Button, Dropdown } from 'antd'
import { ItemType } from 'antd/es/menu/hooks/useItems'

export interface ModaGridRowMenuCellRendererProps {
  menuItems: ItemType[]
}

export const ModaGridRowMenuCellRenderer = (
  props: ModaGridRowMenuCellRendererProps,
) => {
  if (props.menuItems.length === 0) return null

  return (
    <Dropdown menu={{ items: props.menuItems }} trigger={['click']}>
      <Button type="text" size="small" icon={<MenuOutlined />} />
    </Dropdown>
  )
}
