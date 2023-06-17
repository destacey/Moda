import { createContext, useEffect, useState } from "react";
import { useLocalStorageState } from '../../hooks/use-local-storage-state';
import { ConfigProvider } from "antd";
import lightTheme from "@/src/config/theme/light-theme";
import darkTheme from "@/src/config/theme/dark-theme";

interface ThemeContextType {
    currentThemeName: string;
    setCurrentThemeName: (themeName: string) => void;
    appBarColor: string;
    agGridTheme: string;
}

export const ThemeContext = createContext<ThemeContextType | null>(null)

export const ThemeProvider = ({ children }) => {

    const [currentThemeName, setCurrentThemeName] = useLocalStorageState('modaTheme', 'light')
    const [currentTheme, setCurrentTheme] = useState(undefined)
    const [appBarColor, setAppBarColor] = useState('')
    const [agGridTheme, setAgGridTheme] = useState('')

    useEffect(() => {
        setCurrentTheme(currentThemeName === 'light' ? lightTheme : darkTheme)
        setAppBarColor(currentThemeName === 'light' ? '#2196f3' : '#262a2c')
        setAgGridTheme(currentThemeName === 'light' ? 'ag-theme-balham' : 'ag-theme-balham-dark')
    }, [currentThemeName])

    return (
        <ThemeContext.Provider value={{currentThemeName, setCurrentThemeName, appBarColor, agGridTheme}}>
            <ConfigProvider theme={currentTheme}>
                {children}
            </ConfigProvider>
        </ThemeContext.Provider>
    );
};