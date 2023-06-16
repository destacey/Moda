import { createContext, useEffect, useState } from "react";
import { useLocalStorageState } from '../../hooks/use-local-storage-state';
import lightTheme from "@/app/config/theme/light-theme";
import darkTheme from "@/app/config/theme/dark-theme";
import { ConfigProvider } from "antd";

export const ThemeContext = createContext([]);

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
        <ThemeContext.Provider value={[currentThemeName, setCurrentThemeName, appBarColor, agGridTheme]}>
            <ConfigProvider theme={currentTheme}>
                {children}
            </ConfigProvider>
        </ThemeContext.Provider>
    );
};