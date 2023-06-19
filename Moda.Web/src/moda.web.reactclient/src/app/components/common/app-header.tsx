import { Header } from "antd/es/layout/layout";
import Profile from "../Profile";
import useTheme from "../contexts/theme";


export default function AppHeader() {
    const { appBarColor } = useTheme()
    return (
        <Header style={{
            height: 50, display: 'flex', justifyContent: 'space-between', alignItems: 'center',
            backgroundColor: appBarColor
        }}>
            <h1 style={{ fontSize: 24, fontWeight: 400 }}>Moda</h1>
            <Profile />
        </Header>
    )
}