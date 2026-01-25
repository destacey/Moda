import { Button, Dropdown, Tooltip } from 'antd'
import type { MenuProps } from 'antd'
import { FC, ReactNode, useCallback, useMemo } from 'react'
import useTheme from '../contexts/theme'

export interface IconMenuOption {
  label: string
  value: string | number
  extra?: string
}

export interface IconMenuProps {
  icon: ReactNode
  items: IconMenuOption[]
  tooltip?: string
  selectedKeys?: string[]
  maxHeight?: number
  onChange?: (value: string | number) => void
}

const IconMenu: FC<IconMenuProps> = ({
  icon,
  items,
  tooltip,
  selectedKeys,
  maxHeight = 400,
  onChange,
}: IconMenuProps) => {
  const { token } = useTheme()

  const menuItems: MenuProps['items'] = useMemo(
    () =>
      items?.map((item) => ({
        key: item.value.toString(),
        label: item.label,
        extra: item.extra,
      })) || [],
    [items],
  )

  const handleMenuClick: MenuProps['onClick'] = (info) => {
    const selectedItem = items?.find(
      (item) => item.value.toString() === info.key,
    )
    if (selectedItem && onChange) {
      onChange(selectedItem.value)
    }
  }

  const handleOpenChange = useCallback(
    (open: boolean) => {
      if (open && selectedKeys?.length) {
        // Use setTimeout to wait for the dropdown to render
        setTimeout(() => {
          const selectedElement = document.querySelector(
            `.ant-dropdown-menu-item-selected`,
          )
          selectedElement?.scrollIntoView({ block: 'center' })
        }, 0)
      }
    },
    [selectedKeys],
  )

  if (!icon || !items || items.length === 0) return null

  return (
    <Tooltip title={tooltip}>
      <Dropdown
        menu={{
          items: menuItems,
          onClick: handleMenuClick,
          selectedKeys: selectedKeys,
          style: {
            maxHeight,
            overflowY: 'auto',
            scrollbarWidth: 'thin',
            scrollbarColor: `${token.colorTextQuaternary} ${token.colorBgElevated}`,
          },
        }}
        trigger={['click']}
        onOpenChange={handleOpenChange}
      >
        <Button type="text" shape="circle" icon={icon} />
      </Dropdown>
    </Tooltip>
  )
}

export default IconMenu
