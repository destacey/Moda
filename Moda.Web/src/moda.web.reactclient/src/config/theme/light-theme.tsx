import { ThemeConfig, theme } from 'antd'
import { ThemeConstants } from './theme-constants'
const { defaultAlgorithm } = theme

const lightTheme: ThemeConfig = {
  algorithm: defaultAlgorithm,
  token: {
    colorPrimary: ThemeConstants.COLOR_PRIMARY,
    borderRadius: 4,
    wireframe: false,
  },
  components: {
    Layout: {
      headerBg: ThemeConstants.COLOR_PRIMARY,
    },
    Tabs: {
      colorBorderSecondary: '#d9d9d9',
    },
  },
}

export default lightTheme
