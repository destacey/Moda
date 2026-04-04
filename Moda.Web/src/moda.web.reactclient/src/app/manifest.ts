import type { MetadataRoute } from 'next'
import { ThemeConstants } from '@/src/config/theme/theme-constants'

export default function manifest(): MetadataRoute.Manifest {
  return {
    name: 'Moda',
    short_name: 'Moda',
    description:
      'Intelligent delivery management platform for engineering teams',
    start_url: '/',
    display: 'standalone',
    background_color: '#ffffff',
    theme_color: ThemeConstants.COLOR_PRIMARY,
    icons: [
      {
        src: '/icons/icon-192x192.png',
        sizes: '192x192',
        type: 'image/png',
      },
      {
        src: '/icons/icon-512x512.png',
        sizes: '512x512',
        type: 'image/png',
      },
      {
        src: '/icons/icon-512x512.png',
        sizes: '512x512',
        type: 'image/png',
        purpose: 'maskable',
      },
    ],
  }
}
