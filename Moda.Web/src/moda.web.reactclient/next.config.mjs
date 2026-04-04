import { withSerwist } from '@serwist/turbopack'

/**
 * @type {import('next').NextConfig}
 */
const nextConfig = {
  output: 'standalone',
  experimental: {
    optimizePackageImports: ['antd', '@ant-design/icons', '@ant-design/charts'],
  },
}

export default withSerwist(nextConfig)
