// OpenTelemetry initialization for Node.js runtime
// This file is only imported when OTEL_EXPORTER_OTLP_ENDPOINT is configured

import { NodeSDK } from '@opentelemetry/sdk-node'
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-grpc'
import { getNodeAutoInstrumentations } from '@opentelemetry/auto-instrumentations-node'

const otlpEndpoint = process.env.OTEL_EXPORTER_OTLP_ENDPOINT
console.log(`Initializing OpenTelemetry with endpoint: ${otlpEndpoint}`)

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
