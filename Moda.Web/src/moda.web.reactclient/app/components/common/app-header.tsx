import { Typography } from "antd";
import { Header } from "antd/es/layout/layout";
import Profile from "../Profile";
import { useContext } from "react";
import { ThemeContext } from "@/app/components/contexts/theme-context";

const { Title } = Typography;

export default function AppHeader() {
    const themeContext = useContext(ThemeContext)
    return (
        <Header style={{
            height: 50, display: 'flex', justifyContent: 'space-between', alignItems: 'center',
            backgroundColor: themeContext?.appBarColor
        }}>
            <Title level={3}>Moda</Title>
            <Profile />
        </Header>
    )
}