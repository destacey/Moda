'use client'

import { useMsal, useIsAuthenticated } from '@azure/msal-react'
import { InteractionStatus } from '@azure/msal-browser'
import { useCallback, useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import styles from './page.module.css'

const pulseAnimation = `
@keyframes pulse {
  0%, 100% { r: 4; opacity: 1; }
  50% { r: 6; opacity: 0.7; }
}
@keyframes fadeFlow {
  0% { stroke-dashoffset: 200; }
  100% { stroke-dashoffset: 0; }
}
@keyframes chartDraw {
  0% { stroke-dashoffset: 150; }
  100% { stroke-dashoffset: 0; }
}
@keyframes glowPulse {
  0%, 100% { opacity: 0.3; }
  50% { opacity: 0.7; }
}
`

function AnalyticsIllustration() {
  const [, setTick] = useState(0)
  useEffect(() => {
    const interval = setInterval(() => setTick((t) => t + 1), 2000)
    return () => clearInterval(interval)
  }, [])

  const nodes = [
    { x: 140, y: 80, r: 10, color: '#fff', label: 'Teams' },
    { x: 60, y: 160, r: 7, color: '#90caf9', label: 'Sprints' },
    { x: 240, y: 140, r: 8, color: '#bbdefb', label: 'Dependencies' },
    { x: 180, y: 220, r: 6, color: '#fff', label: 'Work Items' },
    { x: 80, y: 280, r: 9, color: '#90caf9', label: 'PIs' },
    { x: 260, y: 260, r: 7, color: '#bbdefb', label: 'Projects' },
    { x: 140, y: 320, r: 5, color: '#fff', label: 'Objectives' },
    { x: 30, y: 100, r: 5, color: '#bbdefb', label: '' },
    { x: 300, y: 60, r: 4, color: '#90caf9', label: '' },
    { x: 320, y: 200, r: 5, color: '#fff', label: '' },
    { x: 20, y: 220, r: 4, color: '#90caf9', label: '' },
  ]

  const edges = [
    [0, 1], // Teams -> Sprints
    [0, 2], // Teams -> Dependencies
    [1, 3], // Sprints -> Work Items
    [2, 3], // Dependencies -> Work Items
    [1, 4], // Sprints -> PIs
    [4, 6], // PIs -> Objectives (solid)
    [3, 6], // Work Items -> Objectives
    [3, 5], // Work Items -> Projects
    [0, 7],
    [0, 8],
    [2, 5],
    [5, 9],
    [1, 10],
  ]

  return (
    <svg
      viewBox="0 0 340 400"
      style={{ width: '100%', height: '100%', maxHeight: 400 }}
    >
      <style>
        {pulseAnimation}
        {`
        .edge { stroke: rgba(100,181,246,0.35); stroke-width: 1.2; fill: none; stroke-dasharray: 4 6; }
        .edge-anim { stroke: rgba(144,202,249,0.6); stroke-width: 1.5; fill: none; stroke-dasharray: 200; stroke-dashoffset: 200; animation: fadeFlow 2.5s ease forwards; }
        .node-glow { animation: glowPulse 3s ease-in-out infinite; }
      `}
      </style>

      {/* Background gradients and filters */}
      <defs>
        <radialGradient id="glow1" cx="50%" cy="50%" r="50%">
          <stop offset="0%" stopColor="#2196f3" stopOpacity="0.25" />
          <stop offset="100%" stopColor="#2196f3" stopOpacity="0" />
        </radialGradient>
        <radialGradient id="glow2" cx="50%" cy="50%" r="50%">
          <stop offset="0%" stopColor="#64b5f6" stopOpacity="0.2" />
          <stop offset="100%" stopColor="#64b5f6" stopOpacity="0" />
        </radialGradient>
        <filter id="shadow">
          <feDropShadow
            dx="0"
            dy="2"
            stdDeviation="3"
            floodColor="#1565c0"
            floodOpacity="0.4"
          />
        </filter>
        <linearGradient id="bgGrad" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#42a5f5" />
          <stop offset="25%" stopColor="#2196f3" />
          <stop offset="50%" stopColor="#1976d2" />
          <stop offset="75%" stopColor="#1565c0" />
          <stop offset="100%" stopColor="#0d47a1" />
        </linearGradient>
        <pattern
          id="gridPattern"
          width="36"
          height="36"
          patternUnits="userSpaceOnUse"
        >
          <path
            d="M 36 0 L 0 0 0 36"
            fill="none"
            stroke="rgba(255,255,255,0.07)"
            strokeWidth="1"
          />
        </pattern>
      </defs>

      <rect x="4" y="4" width="332" height="392" rx="20" fill="url(#bgGrad)" />
      <rect
        x="4"
        y="4"
        width="332"
        height="392"
        rx="20"
        fill="url(#gridPattern)"
      />

      <ellipse cx="100" cy="150" rx="90" ry="80" fill="url(#glow1)" />
      <ellipse cx="240" cy="250" rx="70" ry="70" fill="url(#glow2)" />

      {/* Edges */}
      {edges.map(([a, b], i) => (
        <line
          key={i}
          className={i < 6 ? 'edge-anim' : 'edge'}
          x1={nodes[a].x}
          y1={nodes[a].y}
          x2={nodes[b].x}
          y2={nodes[b].y}
          style={i < 6 ? { animationDelay: `${i * 0.3}s` } : {}}
        />
      ))}

      {/* Mini Bar Chart - top right */}
      <g transform="translate(248, 24)" filter="url(#shadow)">
        <rect
          x="0"
          y="0"
          width="52"
          height="42"
          rx="6"
          fill="rgba(13,71,161,0.7)"
          stroke="rgba(100,181,246,0.3)"
          strokeWidth="1"
        />
        {[
          { x: 6, h: 14, color: '#1976d2' },
          { x: 14, h: 22, color: '#42a5f5' },
          { x: 22, h: 10, color: '#90caf9' },
          { x: 30, h: 28, color: '#1976d2' },
          { x: 38, h: 18, color: '#42a5f5' },
        ].map((b, i) => (
          <rect
            key={i}
            x={b.x}
            y={38 - b.h}
            width="6"
            height={b.h}
            rx="2"
            fill={b.color}
            opacity="0.9"
          />
        ))}
      </g>

      {/* Mini Line Chart - left side */}
      <g transform="translate(10, 240)" filter="url(#shadow)">
        <rect
          x="0"
          y="0"
          width="56"
          height="40"
          rx="6"
          fill="rgba(13,71,161,0.7)"
          stroke="rgba(100,181,246,0.3)"
          strokeWidth="1"
        />
        <polyline
          points="4,32 12,24 20,28 30,14 40,18 50,8"
          fill="none"
          stroke="#42a5f5"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          style={{
            strokeDasharray: 150,
            strokeDashoffset: 150,
            animation: 'chartDraw 2s 1s ease forwards',
          }}
        />
        <circle cx="50" cy="8" r="2.5" fill="#bbdefb" />
      </g>

      {/* Mini Donut - bottom right */}
      <g transform="translate(272, 290)" filter="url(#shadow)">
        <rect
          x="-24"
          y="-24"
          width="48"
          height="48"
          rx="8"
          fill="rgba(13,71,161,0.7)"
          stroke="rgba(100,181,246,0.3)"
          strokeWidth="1"
        />
        <circle
          cx="0"
          cy="0"
          r="14"
          fill="none"
          stroke="#0d47a1"
          strokeWidth="5"
        />
        <circle
          cx="0"
          cy="0"
          r="14"
          fill="none"
          stroke="#1976d2"
          strokeWidth="5"
          strokeDasharray="52 36"
          strokeLinecap="round"
          transform="rotate(-90)"
        />
        <circle
          cx="0"
          cy="0"
          r="14"
          fill="none"
          stroke="#64b5f6"
          strokeWidth="5"
          strokeDasharray="22 66"
          strokeLinecap="round"
          transform="rotate(48)"
        />
      </g>

      {/* Nodes */}
      {nodes.map((n, i) => (
        <g key={i}>
          <circle
            cx={n.x}
            cy={n.y}
            r={n.r + 5}
            fill={n.color}
            opacity="0.08"
            className="node-glow"
            style={{ animationDelay: `${i * 0.4}s` }}
          />
          <circle
            cx={n.x}
            cy={n.y}
            r={n.r}
            fill={n.color}
            filter="url(#shadow)"
          />
          <circle cx={n.x} cy={n.y} r={n.r * 0.4} fill="rgba(13,71,161,0.6)" />
          {n.label && (
            <text
              x={n.x}
              y={n.y + n.r + 14}
              textAnchor="middle"
              fill="rgba(187,222,251,0.85)"
              fontSize="9"
              fontFamily="'Segoe UI', system-ui, sans-serif"
              fontWeight="500"
            >
              {n.label}
            </text>
          )}
        </g>
      ))}

      {/* Center "MODA" badge */}
      <g filter="url(#shadow)">
        <rect
          x="108"
          y="185"
          width="64"
          height="30"
          rx="8"
          fill="rgba(25,118,210,0.85)"
          stroke="rgba(100,181,246,0.4)"
          strokeWidth="1"
        />
        <text
          x="140"
          y="205"
          textAnchor="middle"
          fill="#fff"
          fontSize="13"
          fontFamily="'Segoe UI', system-ui, sans-serif"
          fontWeight="700"
          letterSpacing="2"
        >
          MODA
        </text>
      </g>
    </svg>
  )
}

function MicrosoftLogo() {
  return (
    <svg width="22" height="22" viewBox="0 0 21 21" fill="none">
      <rect x="1" y="1" width="9" height="9" fill="#f25022" rx="1" />
      <rect x="11" y="1" width="9" height="9" fill="#7fba00" rx="1" />
      <rect x="1" y="11" width="9" height="9" fill="#00a4ef" rx="1" />
      <rect x="11" y="11" width="9" height="9" fill="#ffb900" rx="1" />
    </svg>
  )
}

function LoadingSpinner() {
  return (
    <svg
      className={styles.spinner}
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
    >
      <circle
        cx="12"
        cy="12"
        r="10"
        stroke="currentColor"
        strokeWidth="3"
        strokeLinecap="round"
        strokeDasharray="31.4 31.4"
      />
    </svg>
  )
}

export default function LoginPage() {
  const { instance, inProgress } = useMsal()
  const isAuthenticated = useIsAuthenticated()
  const router = useRouter()

  const isInitializing = inProgress === InteractionStatus.Startup
  const isLoggingIn = inProgress === InteractionStatus.HandleRedirect

  useEffect(() => {
    if (isAuthenticated) {
      router.replace('/')
    }
  }, [isAuthenticated, router])

  const handleLogin = useCallback(async () => {
    await instance.loginRedirect()
  }, [instance])

  if (isAuthenticated) {
    return null
  }

  return (
    <div className={styles.pageBackground}>
      {/* Background decoration circles */}
      <div className={`${styles.bgCircle} ${styles.bgCircle1}`} />
      <div className={`${styles.bgCircle} ${styles.bgCircle2}`} />
      <div className={`${styles.bgCircle} ${styles.bgCircle3}`} />

      {/* Main card */}
      <div className={styles.card}>
        {/* Left Panel */}
        <div className={styles.leftPanel}>
          <div className={styles.illustrationWrapper}>
            <AnalyticsIllustration />
          </div>

          <div className={styles.leftPanelText}>
            <h2 className={styles.leftPanelTitle}>Connect the dots.</h2>
            <p className={styles.leftPanelDescription}>
              Unify your teams, sprints, and planning intervals into one
              intelligent delivery platform.
            </p>
          </div>

          {/* Bottom decorative dots */}
          <div className={styles.decorativeDots}>
            {[0, 1, 2, 3, 4].map((i) => (
              <div
                key={i}
                className={`${styles.dot} ${i === 2 ? styles.dotActive : ''}`}
              />
            ))}
          </div>
        </div>

        {/* Right Panel */}
        <div className={styles.rightPanel}>
          <div className={styles.rightPanelContent}>
            {/* Logo */}
            <div className={styles.logo}>
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src="/moda-icon.png"
                alt="Moda"
                className={styles.logoIcon}
              />
              <div className={styles.logoDivider} />
              <span className={styles.logoText}>moda</span>
            </div>

            {/* Header */}
            <h1 className={styles.title}>Welcome</h1>
            <p className={styles.subtitle}>
              Sign in with your organizational account to access your delivery
              platform.
            </p>

            {/* Microsoft Sign In Button */}
            <button
              type="button"
              onClick={handleLogin}
              disabled={isInitializing || isLoggingIn}
              className={styles.loginButton}
            >
              {isInitializing || isLoggingIn ? (
                <>
                  <LoadingSpinner />
                  {isLoggingIn ? 'Signing in...' : 'Loading...'}
                </>
              ) : (
                <>
                  <MicrosoftLogo />
                  Sign in with Microsoft
                </>
              )}
            </button>

            {/* Divider */}
            <div className={styles.divider}>
              <div className={styles.dividerLine} />
              <span className={styles.dividerText}>
                organizational accounts only
              </span>
              <div className={styles.dividerLine} />
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
