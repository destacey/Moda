/**
 * @type {import('next').NextConfig}
 */
const nextConfig = {
  output: 'standalone',
  experimental: {
    staleTimes: {
      dynamic: 30,
      static: 180,
    },
  },
}

export default nextConfig
