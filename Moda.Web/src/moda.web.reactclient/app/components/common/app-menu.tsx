import { TeamOutlined, HomeOutlined, SettingOutlined, ProjectOutlined, ScheduleOutlined, ShopOutlined, DesktopOutlined } from "@ant-design/icons";
import { Menu, MenuProps } from "antd";
import Sider from "antd/es/layout/Sider";
import Link from "next/link";
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
    getItem(<Link href='/'>Home</Link>, 'home1', <HomeOutlined />),
    getItem('Organizations', 'org1', <TeamOutlined />, [
        getItem(<Link href='/organizations/teams'>Teams</Link>, 'org1.1', null),
        getItem(<Link href='/organizations/employees'>Employees</Link>, 'org1.2', null),
    ]),
    getItem('Planning', 'plan1', <ScheduleOutlined />, [
        getItem(<Link href='/planning/program-increments'>Program Increments</Link>, 'plan1.1', null),
        // getItem('Iterations', 'plan1.2', null),
        // getItem('Sprints', 'plan1.3', null),
    ]),
    // getItem('Products', 'pdc1', <DesktopOutlined />, [
    //     getItem('Product Lines', 'pdc1.1', null),
    //     getItem('Product Types', 'pdc1.2', null),
    //     getItem('Products', 'pdc1.3', null),
    //     { type: 'divider' },
    //     getItem('Releases', 'pdc1.4', null),
    //     getItem('Roadmaps', 'pdc1.5', null),
    //     { type: 'divider' },
    //     getItem('Requirement Management', 'pdc1.6', null),
    // ]),
    // getItem('Projects', 'ppm1', <ProjectOutlined />, [
    //     getItem('Portfolios', 'ppm1.1', null),
    //     getItem('Programs', 'ppm1.2', null),
    //     getItem('Projects', 'ppm1.3', null),
    // ]),
    { type: 'divider' },
    getItem('Settings', 'set1', <SettingOutlined />, [
        getItem(<Link href='/settings/users'>Users</Link>, 'set1.1', null),
        getItem(<Link href='/settings/roles'>Roles</Link>, 'set1.2', null),
        getItem(<Link href='/settings/background-jobs'>Background Jobs</Link>, 'set1.3', null),
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