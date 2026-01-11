export async function register() {
  if (process.env.NEXT_RUNTIME === 'nodejs') {
    // Import and initialize NodeSDK for proper trace context propagation
    await import('./instrumentation.node');
  }
}
