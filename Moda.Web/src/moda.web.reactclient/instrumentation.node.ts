import { NodeSDK } from '@opentelemetry/sdk-node';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-grpc';
import { getNodeAutoInstrumentations } from '@opentelemetry/auto-instrumentations-node';
import { W3CTraceContextPropagator } from '@opentelemetry/core';
import { CompositePropagator } from '@opentelemetry/core';
import { W3CBaggagePropagator } from '@opentelemetry/core';

// Set environment variables to configure metric export intervals
process.env.OTEL_METRIC_EXPORT_INTERVAL = '60000';
process.env.OTEL_METRIC_EXPORT_TIMEOUT = '30000';

const sdk = new NodeSDK({
  serviceName: process.env.OTEL_SERVICE_NAME || 'moda-client',
  traceExporter: new OTLPTraceExporter(),
  textMapPropagator: new CompositePropagator({
    propagators: [
      new W3CTraceContextPropagator(),
      new W3CBaggagePropagator(),
    ],
  }),
  instrumentations: [
    getNodeAutoInstrumentations({
      // Automatically instruments http, https, and many other Node.js libraries
      '@opentelemetry/instrumentation-fs': {
        enabled: false, // Disable fs instrumentation to reduce noise
      },
      '@opentelemetry/instrumentation-http': {
        enabled: true,
        // Ensure outgoing requests include trace context
        requireParentforOutgoingSpans: false,
        requireParentforIncomingSpans: false,
        headersToSpanAttributes: {
          client: {
            requestHeaders: ['traceparent', 'tracestate'],
          },
        },
      },
    }),
  ],
});

sdk.start();

// Graceful shutdown
process.on('SIGTERM', () => {
  sdk.shutdown()
    .then(() => console.log('OpenTelemetry SDK shut down successfully'))
    .catch((error) => console.error('Error shutting down OpenTelemetry SDK', error))
    .finally(() => process.exit(0));
});
