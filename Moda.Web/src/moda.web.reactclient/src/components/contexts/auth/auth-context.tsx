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
  EventType,
  InteractionRequiredAuthError,
  SilentRequest,
} from '@azure/msal-browser'
import { LoadingAccount } from '@/src/components/common'
import { AuthContextType, AuthMethod, Claim, User } from './types'
import { tokenRequest } from '@/auth-config'
import { useGetUserPermissionsQuery } from '@/src/store/features/user-management/profile-api'
import UnauthorizedPage from '@/src/app/unauthorized/page'
import ServiceUnavailablePage from '@/src/app/service-unavailable/page'
import ChangePasswordForm from '@/src/app/account/profile/change-password-form'
import useTheme from '@/src/components/contexts/theme/use-theme'
import styles from './auth-provider.module.css'
import {
  getAuthClient,
  getAuthStorage,
  isLocalAuthActive,
  clearLocalAuth,
  getLocalAuthToken,
  LOCAL_AUTH_TOKEN_KEY,
  LOCAL_AUTH_REFRESH_TOKEN_KEY,
  LOCAL_AUTH_TOKEN_EXPIRY_KEY,
  LOCAL_AUTH_MUST_CHANGE_PASSWORD_KEY,
} from '@/src/services/clients'

export const AuthContext = createContext<AuthContextType | null>(null)

// Global flag to prevent concurrent redirect attempts across multiple 401 errors.
// This prevents MSAL interaction_in_progress errors when multiple API calls
// fail simultaneously (e.g., when a token expires).
let isRedirectInProgress = false

/** @internal Reset redirect flag between tests */
export const _resetRedirectFlag = () => {
  isRedirectInProgress = false
}

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const { instance, accounts, inProgress } = useMsal()
  const isAuthenticated = useIsAuthenticated()
  const pathname = usePathname()
  const { token } = useTheme()

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

  const [authMethod, setAuthMethod] = useState<AuthMethod>(
    typeof window !== 'undefined' && isLocalAuthActive() ? 'local' : null,
  )

  const [mustChangePassword, setMustChangePassword] = useState(
    typeof window !== 'undefined' &&
      getAuthStorage().getItem(LOCAL_AUTH_MUST_CHANGE_PASSWORD_KEY) === 'true',
  )

  const [user, setUser] = useState<User>({
    name: '',
    username: '',
    isAuthenticated: false,
    employeeId: null,
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
   * Cross-tab account synchronization.
   * MSAL event callbacks handle same-tab events (login, logout,
   * token refresh). The storage event listener detects changes
   * from other tabs via localStorage.
   * -------------------------------------------------------------
   */
  useEffect(() => {
    // Same-tab: react to MSAL lifecycle events
    const callbackId = instance.addEventCallback((event) => {
      switch (event.eventType) {
        case EventType.LOGIN_SUCCESS:
        case EventType.ACQUIRE_TOKEN_SUCCESS: {
          const payload = event.payload as { account?: AccountInfo } | null
          if (payload?.account) {
            // ACQUIRE_TOKEN_SUCCESS fires on every silent token acquisition,
            // so only update if the account has actually changed to avoid
            // unnecessary re-renders.
            const currentActiveAccount = instance.getActiveAccount()
            const isDifferentAccount =
              !currentActiveAccount ||
              currentActiveAccount.homeAccountId !==
                payload.account.homeAccountId

            if (isDifferentAccount) {
              instance.setActiveAccount(payload.account)
              setActiveAccountState(payload.account)
            }
          }
          break
        }
        case EventType.LOGOUT_SUCCESS: {
          setActiveAccountState(null)
          break
        }
        case EventType.ACTIVE_ACCOUNT_CHANGED: {
          setActiveAccountState(instance.getActiveAccount())
          break
        }
      }
    })

    // Cross-tab: detect account changes via localStorage storage events.
    // MSAL stores auth state in localStorage with keys prefixed by 'msal.'.
    // A null key means localStorage.clear() was called.
    const handleStorageChange = (event: StorageEvent) => {
      if (!event.key?.startsWith('msal.') && event.key !== null) return

      const currentAccounts = instance.getAllAccounts()
      if (currentAccounts.length === 0) {
        // Accounts were cleared (e.g., logout in another tab).
        // Clear active account so React naturally transitions to the
        // unauthenticated view without a disruptive full-page reload.
        instance.setActiveAccount(null)
        setActiveAccountState(null)
      } else if (!instance.getActiveAccount()) {
        // Accounts exist but no active account (e.g., login in another tab)
        instance.setActiveAccount(currentAccounts[0])
        setActiveAccountState(currentAccounts[0])
      }
    }

    window.addEventListener('storage', handleStorageChange, { passive: true })

    return () => {
      if (callbackId) {
        instance.removeEventCallback(callbackId)
      }
      window.removeEventListener('storage', handleStorageChange)
    }
  }, [instance])

  /**
   * -------------------------------------------------------------
   * Permissions (RTK Query)
   * -------------------------------------------------------------
   */
  const {
    data: permissionsData,
    isLoading: permissionsLoading,
    error: permissionsError,
    refetch: refetchPermissions,
  } = useGetUserPermissionsQuery(undefined, {
    skip: authMethod === 'local'
      ? false // Local auth: always fetch (token is in localStorage)
      : !isReady || !isAuthenticated || !activeAccount,
    pollingInterval: 5 * 60 * 1000, // Re-fetch permissions every 5 minutes
  })

  /**
   * -------------------------------------------------------------
   * Retry handler for service unavailable
   * -------------------------------------------------------------
   */
  const handleRetry = () => {
    setIsServiceUnavailable(false)
    setIsLoading(true)
    refetchPermissions()
  }

  /**
   * -------------------------------------------------------------
   * Token helper
   * -------------------------------------------------------------
   */
  const acquireToken = useCallback(async (requestOverrides?: Partial<SilentRequest>): Promise<string> => {
    // For local auth, return the stored token
    if (authMethod === 'local') {
      const token = getLocalAuthToken()
      if (token) return token
      throw new Error('No local auth token found')
    }

    const request = { ...tokenRequest, ...requestOverrides }

    try {
      // MSAL v5 requires account parameter for silent token acquisition
      const accounts = instance.getAllAccounts()
      if (accounts.length === 0) {
        throw new Error('No authenticated accounts found')
      }

      const response = await instance.acquireTokenSilent({
        ...request,
        account: request.account || accounts[0],
      })
      return response.accessToken
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        const response = await instance.acquireTokenPopup(request)
        return response.accessToken
      }
      throw error
    }
  }, [authMethod, instance])

  /**
   * -------------------------------------------------------------
   * Build user
   * -------------------------------------------------------------
   */
  const refreshUser = useCallback(async () => {
    // Handle local auth users
    if (authMethod === 'local') {
      if (permissionsLoading) return

      try {
        setAuthStatus('Loading user profile...')
        const claims: Claim[] = []

        if (permissionsData) {
          if (permissionsData.employeeId) {
            claims.push({ type: 'EmployeeId', value: permissionsData.employeeId })
          }
          claims.push(
            ...permissionsData.permissions.map((p) => ({
              type: 'Permission',
              value: p,
            })),
          )

          // Decode JWT to get user info
          const token = getLocalAuthToken()
          let name = ''
          let username = ''
          if (token) {
            try {
              const payload = JSON.parse(atob(token.split('.')[1]))
              name = [
                payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
                payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname'],
              ].filter(Boolean).join(' ')
              username = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ?? ''
            } catch {
              // Token decode failed, use empty values
            }
          }

          setUser({
            name,
            username,
            isAuthenticated: true,
            employeeId: permissionsData.employeeId ?? null,
            claims,
          })
        } else if (permissionsError) {
          const errorStatus = (permissionsError as any)?.status
          if (errorStatus === 401) {
            // Local token expired and refresh failed
            clearLocalAuth()
            setAuthMethod(null)
            setIsLoading(false)
            return
          }
          if (errorStatus === 403) {
            setIsUnauthorized(true)
            setIsLoading(false)
            return
          }
          setIsServiceUnavailable(true)
          setIsLoading(false)
          return
        }
      } catch (error) {
        console.error('[Auth] Error building local user profile', error)
      } finally {
        setIsLoading(false)
      }
      return
    }

    // Handle MSAL auth users
    if (!activeAccount) {
      setUser({
        name: '',
        username: '',
        isAuthenticated: false,
        employeeId: null,
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

      if (permissionsData) {
        if (permissionsData.employeeId) {
          claims.push({
            type: 'EmployeeId',
            value: permissionsData.employeeId,
          })
        }
        claims.push(
          ...permissionsData.permissions.map((p) => ({
            type: 'Permission',
            value: p,
          })),
        )
      } else if (permissionsError) {
        const errorStatus = (permissionsError as any)?.status
        if (errorStatus === 403) {
          setIsUnauthorized(true)
          setIsLoading(false)
          return
        }
        if (errorStatus === 401) {
          if (isRedirectInProgress || inProgress !== 'none') {
            console.log(
              '[Auth] Redirect already in progress, skipping duplicate redirect',
            )
            setIsLoading(false)
            return
          }

          setAuthStatus('Session expired, redirecting to login...')
          isRedirectInProgress = true

          try {
            await instance.loginRedirect()
          } catch (loginError) {
            console.error(
              '[Auth] Re-authentication redirect failed',
              loginError,
            )
            isRedirectInProgress = false
            setIsLoading(false)
          }
          return
        }
        setIsServiceUnavailable(true)
        setIsLoading(false)
        return
      }

      setUser({
        name: activeAccount.name ?? '',
        username: activeAccount.username,
        isAuthenticated: true,
        employeeId: permissionsData?.employeeId ?? null,
        claims,
      })
    } catch (error) {
      console.error('[Auth] Error building user profile', error)
      setUser({
        name: '',
        username: '',
        isAuthenticated: false,
        employeeId: null,
        claims: [],
      })
    } finally {
      setIsLoading(false)
    }
  }, [
    authMethod,
    activeAccount,
    permissionsLoading,
    permissionsData,
    permissionsError,
    inProgress,
    instance,
  ])

  /**
   * -------------------------------------------------------------
   * React to auth readiness
   * -------------------------------------------------------------
   */
  useEffect(() => {
    // Handle local auth separately
    if (authMethod === 'local') {
      if (permissionsLoading) {
        setAuthStatus('Loading user permissions...')
        return
      }
      refreshUser()
      return
    }

    if (!isReady) {
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
  }, [authMethod, isReady, isAuthenticated, activeAccount, permissionsLoading, refreshUser])

  /**
   * -------------------------------------------------------------
   * Auth actions
   * -------------------------------------------------------------
   */
  const login = useCallback(async () => {
    await instance.loginRedirect()
  }, [instance])

  const localLogin = useCallback(async (username: string, password: string) => {
    setIsLoading(true)
    setAuthStatus('Signing in...')
    try {
      const authClient = getAuthClient()
      const tokenResponse = await authClient.login({ userName: username, password })
      const storage = getAuthStorage()
      storage.setItem(LOCAL_AUTH_TOKEN_KEY, tokenResponse.token)
      storage.setItem(LOCAL_AUTH_REFRESH_TOKEN_KEY, tokenResponse.refreshToken)
      storage.setItem(LOCAL_AUTH_TOKEN_EXPIRY_KEY, new Date(tokenResponse.tokenExpiresAt).toISOString())
      if (tokenResponse.mustChangePassword) {
        storage.setItem(LOCAL_AUTH_MUST_CHANGE_PASSWORD_KEY, 'true')
        setMustChangePassword(true)
      }
      setAuthMethod('local')
      refetchPermissions()
    } catch (error) {
      setIsLoading(false)
      throw error
    }
  }, [refetchPermissions])

  const logout = useCallback(async () => {
    if (authMethod === 'local') {
      // Clear tokens and redirect immediately without updating React state.
      // Updating state before redirect causes a re-render that briefly shows
      // the app behind the forced change password gate.
      clearLocalAuth()
      window.location.href = '/login'
      return
    }
    // Navigate to logout page which handles the Microsoft logout redirect
    window.location.href = '/logout'
  }, [authMethod])

  /**
   * -------------------------------------------------------------
   * Context value
   * -------------------------------------------------------------
   */
  const authContext: AuthContextType = useMemo(
    () => {
      const permissionsSet = new Set(permissionsData?.permissions ?? [])
      return {
        user,
        isLoading,
        authMethod,
        mustChangePassword,
        acquireToken,
        refreshUser,
        hasClaim: (type: string, value: string) =>
          user.claims.some((c) => c.type === type && c.value === value),
        hasPermissionClaim: (value: string) => permissionsSet.has(value),
        login,
        localLogin,
        logout,
      }
    },
    [
      user,
      isLoading,
      authMethod,
      mustChangePassword,
      acquireToken,
      refreshUser,
      permissionsData,
      login,
      localLogin,
      logout,
    ],
  )

  // Bypass all loading/error gates on logout route so logout executes promptly
  // This ensures users can always sign out, even if permissions call is slow/failed
  if (!isLogoutRoute) {
    // Show loading state for authenticated users (MSAL or local)
    if (isLoading && (isAuthenticated || authMethod === 'local')) {
      return <LoadingAccount message={authStatus} />
    }

    if (isUnauthorized) {
      return <UnauthorizedPage />
    }

    if (isServiceUnavailable) {
      return <ServiceUnavailablePage onRetry={handleRetry} onLogout={logout} />
    }
  }

  // Force password change for users who must change their password
  if (mustChangePassword && authMethod === 'local' && user.isAuthenticated) {
    return (
      <AuthContext.Provider value={authContext}>
        <div
          className={styles.changePasswordBackground}
          style={{ '--auth-bg-color': token.colorBgContainer } as React.CSSProperties}
        >
          <ChangePasswordForm
            required
            onFormComplete={() => {
              // Password changed successfully — logout happens in the form
            }}
            onFormCancel={() => logout()}
          />
        </div>
      </AuthContext.Provider>
    )
  }

  return (
    <AuthContext.Provider value={authContext}>{children}</AuthContext.Provider>
  )
}

export default AuthProvider
