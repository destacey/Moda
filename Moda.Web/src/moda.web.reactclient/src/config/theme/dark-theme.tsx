import { ThemeConfig, theme } from 'antd'

const darkTheme: ThemeConfig = {
  algorithm: theme.darkAlgorithm,
  token: {
    colorPrimary: '#1f83d2',
    borderRadius: 4,
    wireframe: false,
  },
  components: {
    Layout: {
      // controls the background color for all layout components
      colorBgHeader: '#313131',
      colorBgTrigger: '#313131',
    },
  },
}

export default darkTheme
