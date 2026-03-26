import { useEffect, useRef, useCallback } from 'react'
import {
  HubConnectionBuilder,
  HubConnection,
  LogLevel,
} from '@microsoft/signalr'
import { tokenRequest } from '@/auth-config'
import { msalInstance } from '@/src/components/contexts/auth/msal-instance'
import { store } from '@/src/store/store'
import { pokerSessionsApi } from '@/src/store/features/planning/poker-sessions-api'
import { QueryTags } from '@/src/store/features/query-tags'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? ''

export interface PresenceParticipant {
  id: string
  name: string
}

/**
 * Establishes a SignalR connection to the Planning Poker hub and
 * automatically invalidates RTK Query cache tags when events arrive.
 * Also tracks participant presence via hub events.
 *
 * Falls back gracefully if the hub is not available (e.g., during local dev
 * without SignalR configured). The detail page also uses 5s polling as a
 * secondary fallback.
 */
export function usePokerSessionConnection(
  sessionId: string | undefined,
  sessionKey: number | undefined,
  onPresenceChange?: (participants: PresenceParticipant[]) => void,
) {
  const connectionRef = useRef<HubConnection | null>(null)
  const presenceMapRef = useRef(new Map<string, PresenceParticipant>())
  const onPresenceChangeRef = useRef(onPresenceChange)

  useEffect(() => {
    onPresenceChangeRef.current = onPresenceChange
  }, [onPresenceChange])

  const emitPresence = useCallback(() => {
    onPresenceChangeRef.current?.(
      Array.from(presenceMapRef.current.values()),
    )
  }, [])

  const invalidateSession = useCallback(() => {
    if (sessionKey === undefined) return
    store.dispatch(
      pokerSessionsApi.util.invalidateTags([
        { type: QueryTags.PokerSessionRound, id: sessionKey },
      ]),
    )
  }, [sessionKey])

  useEffect(() => {
    if (!sessionId) return

    let cancelled = false

    const connect = async () => {
      try {
        const connection = new HubConnectionBuilder()
          .withUrl(`${API_BASE_URL}/hubs/planning-poker`, {
            accessTokenFactory: async () => {
              const accounts = msalInstance.getAllAccounts()
              if (accounts.length === 0) return ''
              const tokenResponse = await msalInstance.acquireTokenSilent({
                ...tokenRequest,
                account: accounts[0],
              })
              return tokenResponse?.accessToken ?? ''
            },
          })
          .withAutomaticReconnect()
          .configureLogging(LogLevel.Warning)
          .build()

        // Game events trigger cache invalidation so the UI refreshes
        const events = [
          'VoteSubmitted',
          'VoteWithdrawn',
          'VotesRevealed',
          'ConsensusSet',
          'RoundReset',
          'SessionUpdated',
          'SessionCompleted',
          'RoundAdded',
          'RoundRemoved',
          'RoundLabelUpdated',
        ]
        for (const event of events) {
          connection.on(event, invalidateSession)
        }

        // Presence events
        connection.on(
          'ParticipantList',
          (participants: { id: string; name: string }[]) => {
            presenceMapRef.current.clear()
            for (const p of participants) {
              presenceMapRef.current.set(p.id, {
                id: p.id,
                name: p.name,
              })
            }
            emitPresence()
          },
        )

        connection.on(
          'ParticipantJoined',
          (participant: { id: string; name: string }) => {
            presenceMapRef.current.set(participant.id, {
              id: participant.id,
              name: participant.name,
            })
            emitPresence()
          },
        )

        connection.on('ParticipantLeft', (data: { id: string }) => {
          presenceMapRef.current.delete(data.id)
          emitPresence()
        })

        await connection.start()

        if (cancelled) {
          await connection.stop()
          return
        }

        // Join the session group — triggers ParticipantList event
        await connection.invoke('JoinSession', sessionId)
        connectionRef.current = connection
      } catch (error) {
        // SignalR is optional — polling is the fallback
        console.warn(
          'Planning Poker SignalR connection failed, falling back to polling:',
          error,
        )
      }
    }

    connect()

    const presenceMap = presenceMapRef.current
    return () => {
      cancelled = true
      presenceMap.clear()
      const conn = connectionRef.current
      if (conn) {
        conn
          .invoke('LeaveSession', sessionId)
          .catch(() => {})
          .finally(() => conn.stop())
        connectionRef.current = null
      }
    }
  }, [sessionId, invalidateSession, emitPresence])
}
