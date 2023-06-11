import { AuthenticatedTemplate, UnauthenticatedTemplate } from "@azure/msal-react";
import { Avatar, Dropdown, Menu, Space } from "antd";
import { acquireToken, msalInstance } from "../services/auth";
import { EditFilled, EditOutlined, HighlightFilled, HighlightOutlined, LogoutOutlined, UserOutlined } from "@ant-design/icons";
import React, { useEffect } from "react";


export interface ProfileProps {
  currentTheme: string;
  setTheme: (theme: string) => void;
}

export default function Profile(
  {currentTheme, setTheme}: ProfileProps
) {
  const [themeIcon, setThemeIcon] =  React.useState(React.createElement(HighlightOutlined));

  const handleLogout = () => {
    msalInstance.logoutPopup();
  }

  const handleLogin = async () => {
    if(!msalInstance.getActiveAccount()){
      const response = await msalInstance.loginPopup();
    }
    const token = await acquireToken();
    console.log(token);
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
      setThemeIcon(React.createElement(HighlightOutlined));
    } else {
      setThemeIcon(React.createElement(HighlightFilled));
    }
  }, [currentTheme]);

  const menuItems = [
      { key: 'profile', label: 'Account', icon: React.createElement(UserOutlined) },
      { key: 'theme', label: 'Theme', icon: themeIcon},
      { key: 'logout', label: 'Logout', icon: React.createElement(LogoutOutlined) }
    ];

  const handleMenuItemClicked = (info: any) => {
      if(info.key === "theme") {
        toggleTheme();
      }
  };

  const authTemplate = () => {
    return (
      <AuthenticatedTemplate>
        <Space>
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
          <button onClick={handleLogin}>Login</button>
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