import { Space, Typography } from "antd";
import { Header } from "antd/es/layout/layout";
import Profile from "../Profile";

const { Title } = Typography;

export interface AppHeaderProps {
    currentTheme: string;
    setTheme: (theme: string) => void;
}

export default function AppHeader({ currentTheme, setTheme }: AppHeaderProps) {
    return (
        <Header style={{
            height: 50, display: 'flex', justifyContent: 'space-between', alignItems: 'center',
            backgroundColor: currentTheme === 'dark' ? '#262a2c' : '#2196f3'
        }}>
            <Title level={3} className="pb-2">Moda</Title>
            <Profile currentTheme={currentTheme} setTheme={setTheme} />
        </Header>
    )
}