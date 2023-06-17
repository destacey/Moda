import { Header } from "antd/es/layout/layout";
import Profile from "../Profile";
import { useContext } from "react";
import { ThemeContext } from "@/app/components/contexts/theme-context";
import "@/app/globals.css";

export default function AppHeader() {
    const themeContext = useContext(ThemeContext)
    return (
        <Header style={{
            height: 50, display: 'flex', justifyContent: 'space-between', alignItems: 'center',
            backgroundColor: themeContext?.appBarColor
        }}>
            <h1 style={{ fontSize: 24, fontWeight: 400 }}>Moda</h1>
            <Profile />
        </Header>
    )
}