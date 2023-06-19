import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";
import { Avatar, Button, Dropdown, Space } from "antd";
import { HighlightFilled, HighlightOutlined, LogoutOutlined, UserOutlined } from "@ant-design/icons";
import { createElement, useContext, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { ThemeContext } from "./contexts/theme-context";
import useAuth from "./contexts/auth";


export default function Profile() {
  const themeContext = useContext(ThemeContext)
  const {login, logout, user} = useAuth()
  const [themeIcon, setThemeIcon] = useState(createElement(HighlightOutlined));
  const router = useRouter();

  const handleLogout = () => {
    logout()
      .catch((e) => { console.error(`logoutRedirect failed: ${e}`) });
  }

  const handleLogin = async () => {
    if(!user.isAuthenticated){
      await login()
        .catch((e) => { console.error(`loginRedirect failed: ${e}`) });
    }
  }

  function WelcomeUser() {
    return user.name && user.name.trim() ? <p>Welcome, {user.name}</p> : null;
  }

  const toggleTheme = () => {
    themeContext?.setCurrentThemeName(themeContext?.currentThemeName === 'light' ? 'dark' : 'light')
  }

  useEffect(() => {
    setThemeIcon(createElement(themeContext?.currentThemeName === 'light' ? HighlightOutlined : HighlightFilled))
  }, [themeContext?.currentThemeName]);

  const menuItems = [
      { key: 'profile', label: 'Account', icon: createElement(UserOutlined) },
      { key: 'theme', label: 'Theme', icon: themeIcon },
      { key: 'logout', label: 'Logout', icon: createElement(LogoutOutlined) }
    ];

  const handleMenuItemClicked = (info: any) => {
    switch(info.key) {
      case "profile":
        router.push('/account/profile');
        break;
      case "theme":
        toggleTheme();
        break;
      case "logout":
        handleLogout();
    }
  };

  const authTemplate = () => {
    return (
      <AuthenticatedTemplate>
        <Space>
          <WelcomeUser />
          <Dropdown menu={{items: menuItems, onClick: handleMenuItemClicked}}>
            <Avatar icon={<UserOutlined />} />
          </Dropdown>
        </Space>
      </AuthenticatedTemplate>
    )
  }

  const noAuthTemplate = () => {
    return (
      <UnauthenticatedTemplate>
        <Space>
          <div>Unauthenticated</div>
          <Button onClick={handleLogin}>Login</Button>
        </Space>
      </UnauthenticatedTemplate>
    )
  }

  return (
    <>
      {authTemplate()}
      {noAuthTemplate()}
    </>
  )
}