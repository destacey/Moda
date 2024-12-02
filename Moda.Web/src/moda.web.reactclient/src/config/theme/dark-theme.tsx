import { ThemeConfig, theme } from 'antd'
const { darkAlgorithm } = theme

const darkTheme: ThemeConfig = {
  algorithm: darkAlgorithm,
  token: {
    colorPrimary: '#1f83d2',
    borderRadius: 4,
    wireframe: false,
  },
  components: {
    Layout: {
      headerBg: '#313131',
      triggerBg: '#313131',
    },
  },
}

export default darkTheme
