import { TeamOutlined, HomeOutlined, SettingOutlined, ProjectOutlined, ScheduleOutlined, ShopOutlined } from "@ant-design/icons";
import { Menu, MenuProps } from "antd";
import Sider from "antd/es/layout/Sider";
import { useState } from "react";

type MenuItem = Required<MenuProps>['items'][number];

function getItem(
  label: React.ReactNode,
  key: React.Key,
  icon?: React.ReactNode,
  children?: MenuItem[],
  type?: 'group',
): MenuItem {
  return {
    key,
    icon,
    children,
    label,
    type,
  } as MenuItem;
}

const items: MenuProps['items'] = [
    getItem('Home', 'home1', <HomeOutlined />),
    getItem('Organization', 'org1', <TeamOutlined />, [
        getItem('Teams', 'org1.1', null),
        getItem('Employees', 'org1.2', null),
    ]),
    getItem('Planning', 'plan1', <ScheduleOutlined />, [
        getItem('Program Increments', 'plan1.1', null),
        getItem('Iterations', 'plan1.2', null),
        getItem('Sprints', 'plan1.3', null),
    ]),
    getItem('Products', 'pdc1', <ShopOutlined />, [
        getItem('Product Lines', 'pdc1.1', null),
        getItem('Product Types', 'pdc1.2', null),
        getItem('Productss', 'pdc1.3', null),
        { type: 'divider' },
        getItem('Releases', 'pdc1.4', null),
        getItem('Roadmaps', 'pdc1.5', null),
    ]),
    getItem('Projects', 'ppm1', <ProjectOutlined />, [
        getItem('Portfolios', 'ppm1.1', null),
        getItem('Programs', 'ppm1.2', null),
        getItem('Projects', 'ppm1.3', null),
    ]),
    { type: 'divider' },
    getItem('Admin', 'adm1', <SettingOutlined />, [
        getItem('Users', 'adm1.1', null),
        getItem('Roles', 'adm1.2', null),
        getItem('Background Jobs', 'adm1.3', null),
    ]),
];

export default function AppMenu() {
    const [collapsed, setCollapsed] = useState(false);
    return (
        <Sider width={200} collapsedWidth={50} collapsible collapsed={collapsed} onCollapse={(value) => setCollapsed(value)}>
            <Menu
                mode="inline"
                defaultSelectedKeys={['1']}
                defaultOpenKeys={['sub1']}
                style={{ height: '100%', borderRight: 0 }}
                items={items}
            />
        </Sider>
    )
}