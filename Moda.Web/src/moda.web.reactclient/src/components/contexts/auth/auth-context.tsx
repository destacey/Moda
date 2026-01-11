'use client'

import React, {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react'
import { useMsal, useIsAuthenticated } from '@azure/msal-react'
import {
  InteractionRequiredAuthError,
  SilentRequest,
} from '@azure/msal-browser'
import { LoadingAccount } from '@/src/components/common'
import { AuthContextType, Claim, User } from './types'
import { tokenRequest } from '@/auth-config'
import { useGetUserPermissionsQuery } from '@/src/store/features/user-management/profile-api'

export const AuthContext = createContext<AuthContextType | null>(null)

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const { instance, accounts } = useMsal()
  const isAuthenticated = useIsAuthenticated()

  const initializeOnce = useRef(false)

  const [redirectHandled, setRedirectHandled] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [authStatus, setAuthStatus] = useState('Initializing authentication...')

  const [user, setUser] = useState<User>({
    name: '',
    username: '',
    isAuthenticated: false,
    claims: [],
  })

  /**
   * -------------------------------------------------------------
   * MSAL initialize + redirect + silent SSO
   * -------------------------------------------------------------
   */
  useEffect(() => {
    const handleAuth = async () => {
      if (initializeOnce.current) return
      initializeOnce.current = true

      try {
        setAuthStatus('Initializing authentication...')
        await instance.initialize()

        setAuthStatus('Processing authentication redirect...')
        const response = await instance.handleRedirectPromise()

        if (response?.account) {
          instance.setActiveAccount(response.account)
          setRedirectHandled(true)
          return
        }

        if (instance.getAllAccounts().length === 0) {
          try {
            setAuthStatus('Attempting silent sign-in...')
            const ssoResponse = await instance.ssoSilent(tokenRequest)

            if (ssoResponse?.account) {
              instance.setActiveAccount(ssoResponse.account)
            }
          } catch (error) {
            console.warn('Silent SSO failed, redirecting to login', error)
            setAuthStatus('Redirecting to sign-in...')
            await instance.loginRedirect()
            return
          }
        }

        setRedirectHandled(true)
      } catch (error) {
        console.error('Authentication initialization failed', error)
        setAuthStatus('Authentication error')
        setRedirectHandled(true)
      }
    }

    handleAuth()
  }, [instance])

  /**
   * -------------------------------------------------------------
   * Ensure active account is set
   * -------------------------------------------------------------
   */
  useEffect(() => {
    if (!instance.getActiveAccount() && accounts.length > 0) {
      instance.setActiveAccount(accounts[0])
    }
  }, [accounts, instance])

  const activeAccount = useMemo(
    () => instance.getActiveAccount(),

    // accounts is needed to auth correctly when changing tabs
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [instance, accounts],
  )

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
    skip: !redirectHandled || !isAuthenticated || !activeAccount,
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
      console.error('Error refreshing user', error)
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
    if (!redirectHandled) return

    if (!isAuthenticated || !activeAccount) {
      setIsLoading(false)
      return
    }

    if (!permissionsLoading) {
      refreshUser()
    }
  }, [
    redirectHandled,
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
