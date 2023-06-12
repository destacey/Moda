import { UserOutlined, LaptopOutlined, NotificationOutlined } from "@ant-design/icons";
import { Menu, MenuProps } from "antd";
import Sider from "antd/es/layout/Sider";
import React from "react";
import { useState } from "react";

const items2: MenuProps['items'] = [UserOutlined, LaptopOutlined, NotificationOutlined].map(
    (icon, index) => {
        const key = String(index + 1);

        return {
            key: `sub${key}`,
            icon: React.createElement(icon),
            label: `subnav ${key}`,

            children: new Array(4).fill(null).map((_, j) => {
                const subKey = index * 4 + j + 1;
                return {
                    key: subKey,
                    label: `option${subKey}`,
                };
            }),
        };
    },
);

export default function AppMenu() {
    const [collapsed, setCollapsed] = useState(false);
    return (
        <Sider width={200} collapsedWidth={50} collapsible collapsed={collapsed} onCollapse={(value) => setCollapsed(value)}>
            <Menu
                mode="inline"
                defaultSelectedKeys={['1']}
                defaultOpenKeys={['sub1']}
                style={{ height: '100%', borderRight: 0 }}
                items={items2}
            />
        </Sider>
    )
}