'use client'

import React, {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react'
import { usePathname } from 'next/navigation'
import { useMsal, useIsAuthenticated } from '@azure/msal-react'
import {
  AccountInfo,
  InteractionRequiredAuthError,
  SilentRequest,
} from '@azure/msal-browser'
import { LoadingAccount } from '@/src/components/common'
import { AuthContextType, Claim, User } from './types'
import { tokenRequest } from '@/auth-config'
import { useGetUserPermissionsQuery } from '@/src/store/features/user-management/profile-api'
import UnauthorizedPage from '@/src/app/unauthorized/page'
import ServiceUnavailablePage from '@/src/app/service-unavailable/page'

export const AuthContext = createContext<AuthContextType | null>(null)

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const { instance, accounts, inProgress } = useMsal()
  const isAuthenticated = useIsAuthenticated()
  const pathname = usePathname()

  // Bypass loading/error gates on logout route so logout always executes promptly
  const isLogoutRoute = pathname === '/logout'

  // Note: We no longer use useMsalAuthentication for auto-login
  // Users will see the LoginPage and click "Sign in with Microsoft" manually

  const [isLoading, setIsLoading] = useState(true)
  const [isUnauthorized, setIsUnauthorized] = useState(false)
  const [isServiceUnavailable, setIsServiceUnavailable] = useState(false)
  const [authStatus, setAuthStatus] = useState('Initializing authentication...')
  const [activeAccount, setActiveAccountState] = useState<AccountInfo | null>(
    null,
  )

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
   * Uses useState to ensure React properly tracks the active account
   * -------------------------------------------------------------
   */
  useEffect(() => {
    if (!isReady) {
      setActiveAccountState(null)
      return
    }

    let account = instance.getActiveAccount()
    if (!account && accounts.length > 0) {
      instance.setActiveAccount(accounts[0])
      account = accounts[0]
    }
    setActiveAccountState(account)
  }, [accounts, instance, isReady])

  /**
   * -------------------------------------------------------------
   * Permissions (RTK Query)
   * -------------------------------------------------------------
   */
  const {
    data: permissions,
    isLoading: permissionsLoading,
    error: permissionsError,
    refetch: refetchPermissions,
  } = useGetUserPermissionsQuery(undefined, {
    skip: !isReady || !isAuthenticated || !activeAccount,
  })

  /**
   * -------------------------------------------------------------
   * Retry handler for service unavailable
   * -------------------------------------------------------------
   */
  const handleRetry = useCallback(() => {
    setIsServiceUnavailable(false)
    setIsLoading(true)
    refetchPermissions()
  }, [refetchPermissions])

  /**
   * -------------------------------------------------------------
   * Token helper
   * -------------------------------------------------------------
   */
  const acquireToken = useCallback(
    async (requestOverrides?: Partial<SilentRequest>): Promise<string> => {
      const request = { ...tokenRequest, ...requestOverrides }

      try {
        // MSAL v5 requires account parameter for silent token acquisition
        const accounts = instance.getAllAccounts()
        if (accounts.length === 0) {
          throw new Error('No authenticated accounts found')
        }

        const response = await instance.acquireTokenSilent({
          ...request,
          account: request.account || accounts[0], // Use provided account or first available
        })
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
        // Check if it's a 403 Forbidden error - user is authenticated but not authorized
        const errorStatus = (permissionsError as any)?.status
        if (errorStatus === 403) {
          setIsUnauthorized(true)
          setIsLoading(false)
          return
        }
        // For other errors (timeout, network issues, 5xx), show service unavailable
        setIsServiceUnavailable(true)
        setIsLoading(false)
        return
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
      // Show loading state during MSAL operations (redirect handling, etc.)
      setIsLoading(true)
      setAuthStatus('Authenticating...')
      return
    }

    // For unauthenticated users, don't show loading - they'll see login page
    if (!isAuthenticated) {
      setIsLoading(false)
      return
    }

    // Wait for activeAccount to be set (happens in separate useEffect)
    // Don't set isLoading to false here - keep showing loading state
    if (!activeAccount) {
      setAuthStatus('Loading account...')
      return
    }

    if (permissionsLoading) {
      setAuthStatus('Loading user permissions...')
      return
    }

    // Only proceed when permissions have loaded or errored
    refreshUser()
  }, [isReady, isAuthenticated, activeAccount, permissionsLoading, refreshUser])

  /**
   * -------------------------------------------------------------
   * Auth actions
   * -------------------------------------------------------------
   */
  const login = useCallback(async () => {
    await instance.loginRedirect()
  }, [instance])

  const logout = useCallback(async () => {
    // Navigate to logout page which handles the Microsoft logout redirect
    window.location.href = '/logout'
  }, [])

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

  // Bypass all loading/error gates on logout route so logout executes promptly
  // This ensures users can always sign out, even if permissions call is slow/failed
  if (!isLogoutRoute) {
    // Only show loading state for authenticated users loading permissions
    // Unauthenticated users should see the login page immediately via UnauthenticatedTemplate
    if (isLoading && isAuthenticated) {
      return <LoadingAccount message={authStatus} />
    }

    if (isUnauthorized) {
      return <UnauthorizedPage />
    }

    if (isServiceUnavailable) {
      return <ServiceUnavailablePage onRetry={handleRetry} />
    }
  }

  return (
    <AuthContext.Provider value={authContext}>{children}</AuthContext.Provider>
  )
}

export default AuthProvider
