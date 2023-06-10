import { ThemeProvider } from "styled-components";
import { theme } from "antd";
import lightTheme from "./light-theme";

export default ({ children }: React.PropsWithChildren) => {
  const { token } = theme.useToken();
  return (
    <ThemeProvider theme={{ antd: token, base: lightTheme }}>
      {children}
    </ThemeProvider>
  );
};