import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";
import { Avatar, Button, Dropdown, Menu, Space } from "antd";
import { acquireToken, msalInstance } from "../services/auth";
import { HighlightFilled, HighlightOutlined, LogoutOutlined, UserOutlined } from "@ant-design/icons";
import { createElement, useEffect, useState } from "react";


export interface ProfileProps {
  currentTheme: string;
  setTheme: (theme: string) => void;
}

export default function Profile(
  {currentTheme, setTheme}: ProfileProps
) {
  const [themeIcon, setThemeIcon] = useState(createElement(HighlightOutlined));

  const handleLogout = () => {
    msalInstance.logoutRedirect()
      .catch((e) => { console.error(`logoutRedirect failed: ${e}`) });
  }

  const handleLogin = async () => {
    if(!msalInstance.getActiveAccount()){
      await msalInstance.loginRedirect()
        .catch((e) => { console.error(`loginRedirect failed: ${e}`) });
    }

    const token = await acquireToken();
    console.log(token);
  }

  function WelcomeUser() {
    const { accounts } = useMsal();
    const username = accounts[0].name;
    return username && username.trim() ? <p>Welcome, {username}</p> : null;
  }

  const toggleTheme = () => {
    if(currentTheme === 'light'){
      setTheme('dark');
    } else {
      setTheme('light');
    }
  }

  useEffect(() => {
    if(currentTheme === 'light'){
      setThemeIcon(createElement(HighlightOutlined));
    } else {
      setThemeIcon(createElement(HighlightFilled));
    }
  }, [currentTheme]);

  const menuItems = [
      { key: 'profile', label: 'Account', icon: createElement(UserOutlined) },
      { key: 'theme', label: 'Theme', icon: themeIcon },
      { key: 'logout', label: 'Logout', icon: createElement(LogoutOutlined) }
    ];

  const handleMenuItemClicked = (info: any) => {
      if(info.key === "theme") {
        toggleTheme();
      }
      else if(info.key === "logout") {
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