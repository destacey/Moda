import { ThemeConfig, theme } from 'antd'

const lightTheme: ThemeConfig = {
  algorithm: theme.defaultAlgorithm,
  token: {
    colorPrimary: '#2196f3',
    borderRadius: 4,
    wireframe: false,
  },
  components: {
    Layout: {
      // controls the background color for all layout components
      colorBgHeader: '#2196f3',
    },
  },
}

export default lightTheme
