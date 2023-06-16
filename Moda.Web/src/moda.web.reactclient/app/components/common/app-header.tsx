import { Typography } from "antd";
import { Header } from "antd/es/layout/layout";
import Profile from "../Profile";
import { useContext } from "react";
import { ThemeContext } from "@/app/layout";

const { Title } = Typography;

export default function AppHeader() {
    const [currentThemeName, _] = useContext(ThemeContext)
    return (
        <Header style={{
            height: 50, display: 'flex', justifyContent: 'space-between', alignItems: 'center',
            backgroundColor: currentThemeName === 'dark' ? '#262a2c' : '#2196f3'
        }}>
            <Title level={3}>Moda</Title>
            <Profile />
        </Header>
    )
}