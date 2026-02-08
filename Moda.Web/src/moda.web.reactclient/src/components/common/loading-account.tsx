import styles from '@/src/app/logout/page.module.css'

interface LoadingAccountProps {
  message?: string
}

function LoadingSpinner() {
  return (
    <svg
      className={styles.spinner}
      width="48"
      height="48"
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

const LoadingAccount: React.FC<LoadingAccountProps> = (props) => {
  const message = props.message || 'Loading...'

  return (
    <div className={styles.pageBackground}>
      {/* Background decoration circles */}
      <div className={`${styles.bgCircle} ${styles.bgCircle1}`} />
      <div className={`${styles.bgCircle} ${styles.bgCircle2}`} />
      <div className={`${styles.bgCircle} ${styles.bgCircle3}`} />

      {/* Main card */}
      <div className={styles.card}>
        <div className={styles.content}>
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

          {/* Loading state */}
          <div className={styles.spinnerWrapper}>
            <LoadingSpinner />
          </div>

          <h1 className={styles.title}>{message}</h1>
          <p className={styles.subtitle}>
            Please wait while we prepare your account.
          </p>
        </div>
      </div>
    </div>
  )
}

export default LoadingAccount
