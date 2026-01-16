'use client'

import React, {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react'
import { useMsal, useIsAuthenticated, useMsalAuthentication } from '@azure/msal-react'
import {
  InteractionRequiredAuthError,
  InteractionType,
  SilentRequest,
} from '@azure/msal-browser'
import { LoadingAccount } from '@/src/components/common'
import { AuthContextType, Claim, User } from './types'
import { tokenRequest } from '@/auth-config'
import { useGetUserPermissionsQuery } from '@/src/store/features/user-management/profile-api'

export const AuthContext = createContext<AuthContextType | null>(null)

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const { instance, accounts, inProgress } = useMsal()
  const isAuthenticated = useIsAuthenticated()

  // useMsalAuthentication handles the login flow automatically
  // It will use silent auth if possible, otherwise redirect to login
  const { error: authError } = useMsalAuthentication(InteractionType.Redirect, tokenRequest)

  const [isLoading, setIsLoading] = useState(true)
  const [authStatus, setAuthStatus] = useState('Initializing authentication...')

  const [user, setUser] = useState<User>({
    name: '',
    username: '',
    isAuthenticated: false,
    claims: [],
  })

  // Track when MSAL has finished any in-progress operations
  const isReady = inProgress === 'none'

  /**
   * -------------------------------------------------------------
   * Ensure active account is set when we have accounts
   * -------------------------------------------------------------
   */
  useEffect(() => {
    if (!isReady) return

    const activeAccount = instance.getActiveAccount()
    if (!activeAccount && accounts.length > 0) {
      instance.setActiveAccount(accounts[0])
    }
  }, [accounts, instance, isReady])

  const activeAccount = useMemo(() => {
    if (!isReady) return null
    return instance.getActiveAccount()
  }, [instance, accounts, isReady]) // eslint-disable-line react-hooks/exhaustive-deps

  // Log auth errors
  useEffect(() => {
    if (authError) {
      console.error('[Auth] Authentication error:', authError)
    }
  }, [authError])

  /**
   * -------------------------------------------------------------
   * Permissions (RTK Query)
   * -------------------------------------------------------------
   */
  const {
    data: permissions,
    isLoading: permissionsLoading,
    error: permissionsError,
  } = useGetUserPermissionsQuery(undefined, {
    skip: !isReady || !isAuthenticated || !activeAccount,
  })

  /**
   * -------------------------------------------------------------
   * Token helper
   * -------------------------------------------------------------
   */
  const acquireToken = useCallback(
    async (requestOverrides?: Partial<SilentRequest>): Promise<string> => {
      const request = { ...tokenRequest, ...requestOverrides }

      try {
        const response = await instance.acquireTokenSilent(request)
        return response.accessToken
      } catch (error) {
        if (error instanceof InteractionRequiredAuthError) {
          const response = await instance.acquireTokenPopup(request)
          return response.accessToken
        }
        throw error
      }
    },
    [instance],
  )

  /**
   * -------------------------------------------------------------
   * Build user
   * -------------------------------------------------------------
   */
  const refreshUser = useCallback(async () => {
    if (!activeAccount) {
      setUser({
        name: '',
        username: '',
        isAuthenticated: false,
        claims: [],
      })
      setIsLoading(false)
      return
    }

    if (permissionsLoading) return

    try {
      setAuthStatus('Loading user profile...')

      const idTokenClaims = activeAccount.idTokenClaims ?? {}
      const claims: Claim[] = []

      for (const [key, value] of Object.entries(idTokenClaims)) {
        if (typeof value === 'string') {
          claims.push({ type: key, value })
        }
      }

      if (permissions) {
        claims.push(
          ...permissions.map((p) => ({
            type: 'Permission',
            value: p,
          })),
        )
      } else if (permissionsError) {
        console.error('Failed to load permissions', permissionsError)
      }

      setUser({
        name: activeAccount.name ?? '',
        username: activeAccount.username,
        isAuthenticated: true,
        claims,
      })
    } catch (error) {
      console.error('[Auth] Error building user profile', error)
      setUser({
        name: '',
        username: '',
        isAuthenticated: false,
        claims: [],
      })
    } finally {
      setIsLoading(false)
    }
  }, [activeAccount, permissions, permissionsLoading, permissionsError])

  /**
   * -------------------------------------------------------------
   * React to auth readiness
   * -------------------------------------------------------------
   */
  useEffect(() => {
    if (!isReady) {
      setAuthStatus('Authenticating...')
      return
    }

    if (!isAuthenticated || !activeAccount) {
      setIsLoading(false)
      return
    }

    if (!permissionsLoading) {
      refreshUser()
    }
  }, [
    isReady,
    isAuthenticated,
    activeAccount,
    permissionsLoading,
    refreshUser,
  ])

  /**
   * -------------------------------------------------------------
   * Auth actions
   * -------------------------------------------------------------
   */
  const login = useCallback(async () => {
    await instance.loginRedirect()
  }, [instance])

  const logout = useCallback(async () => {
    instance.setActiveAccount(null)
    setUser({
      name: '',
      username: '',
      isAuthenticated: false,
      claims: [],
    })
    await instance.logoutRedirect()
  }, [instance])

  /**
   * -------------------------------------------------------------
   * Context value
   * -------------------------------------------------------------
   */
  const authContext = useMemo<AuthContextType>(
    () => ({
      user,
      isLoading,
      acquireToken,
      refreshUser,
      hasClaim: (type: string, value: string) =>
        user.claims.some((c) => c.type === type && c.value === value),
      hasPermissionClaim: (value: string) =>
        user.claims.some((c) => c.type === 'Permission' && c.value === value),
      login,
      logout,
    }),
    [user, isLoading, acquireToken, refreshUser, login, logout],
  )

  if (isLoading) {
    return <LoadingAccount message={authStatus} />
  }

  return (
    <AuthContext.Provider value={authContext}>{children}</AuthContext.Provider>
  )
}

export default AuthProvider
