// @ts-check

import eslint from '@eslint/js'
import tsEslint from 'typescript-eslint'
import nextPlugin from '@next/eslint-plugin-next'
import reactRecommended from 'eslint-plugin-react/configs/recommended.js'
import reactHook from 'eslint-plugin-react-hooks'
import eslintPluginPrettierRecommended from 'eslint-plugin-prettier/recommended'

export default tsEslint.config(
  eslint.configs.recommended,
  ...tsEslint.configs.recommended,
  {
    plugins: {
      '@next/next': nextPlugin,
    },
    rules: {
      ...nextPlugin.configs.recommended.rules,
      ...nextPlugin.configs['core-web-vitals'].rules,
    },
  },
  {
    ...reactRecommended,
    rules: {
      ...reactRecommended.rules,
      'react/react-in-jsx-scope': 'off', // Not needed in React 18 or Next.js
    },
  },
  eslintPluginPrettierRecommended,
  {
    plugins: {
      'react-hooks': reactHook,
    },
    rules: {
      ...reactHook.configs.recommended.rules,
    },
  },
)
