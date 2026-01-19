// Empty export to make this a module (enables top-level await)
export {}

// Only initialize OpenTelemetry if OTLP endpoint is configured (e.g., when running with Aspire)
const otlpEndpoint = process.env.OTEL_EXPORTER_OTLP_ENDPOINT

if (otlpEndpoint) {
  console.log(`Initializing OpenTelemetry with endpoint: ${otlpEndpoint}`)

  // Dynamic imports to avoid loading heavy packages when OTel is disabled
  const { NodeSDK } = await import('@opentelemetry/sdk-node')
  const { OTLPTraceExporter } = await import(
    '@opentelemetry/exporter-trace-otlp-grpc'
  )
  const { getNodeAutoInstrumentations } = await import(
    '@opentelemetry/auto-instrumentations-node'
  )

  // Configure metric export intervals via environment variables
  // This prevents timing validation errors in the SDK
  process.env.OTEL_METRIC_EXPORT_INTERVAL = '5000'
  process.env.OTEL_METRIC_EXPORT_TIMEOUT = '3000'

  const sdk = new NodeSDK({
    serviceName: process.env.OTEL_SERVICE_NAME || 'moda-client',
    traceExporter: new OTLPTraceExporter(),
    instrumentations: [
      getNodeAutoInstrumentations({
        // Disable fs instrumentation to reduce noise
        '@opentelemetry/instrumentation-fs': {
          enabled: false,
        },
      }),
    ],
  })

  sdk.start()

  // Graceful shutdown
  process.on('SIGTERM', () => {
    sdk
      .shutdown()
      .then(() => console.log('OpenTelemetry SDK shut down successfully'))
      .catch((error) =>
        console.error('Error shutting down OpenTelemetry SDK', error),
      )
      .finally(() => process.exit(0))
  })
} else {
  console.log(
    'OpenTelemetry disabled: OTEL_EXPORTER_OTLP_ENDPOINT not configured',
  )
}
