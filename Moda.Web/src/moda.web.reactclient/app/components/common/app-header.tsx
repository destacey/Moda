import { Space, Typography } from "antd";
import { Header } from "antd/es/layout/layout";
import Profile from "../Profile";

const { Title } = Typography;

export interface AppHeaderProps {
    currentTheme: string;
    setTheme: (theme: string) => void;
}

export default function AppHeader({currentTheme, setTheme}: AppHeaderProps) {
    return (
        <Header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', backgroundColor: currentTheme === 'dark' ? '#262a2c' : '#2196f3' }}>
            <Title>Moda</Title>
            <Space>
                <Profile currentTheme={currentTheme} setTheme={setTheme} />
            </Space>
        </Header>
    )
}