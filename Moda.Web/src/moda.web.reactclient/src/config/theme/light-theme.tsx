import { ThemeConfig, theme } from 'antd'
const { defaultAlgorithm } = theme

const lightTheme: ThemeConfig = {
  algorithm: defaultAlgorithm,
  token: {
    colorPrimary: '#2196f3',
    borderRadius: 4,
    wireframe: false,
  },
  components: {
    Layout: {
      headerBg: '#2196f3',
    },
  },
}

export default lightTheme
