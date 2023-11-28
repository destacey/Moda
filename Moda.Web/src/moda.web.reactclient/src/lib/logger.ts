import pino, { Logger } from 'pino'
import pinoToSeq from 'pino-seq'

let logger: Logger
export const getLogger = () => {
    if (!logger) {
        const deploymentEnv = process.env.NODE_ENV || 'development'
        let stream = pinoToSeq.createStream({ serverUrl: 'http://localhost:5341' })
        logger = pino({
            name: 'moda-client',
            level: deploymentEnv === 'production' ? 'info' : 'debug',
        }, stream)
    }
    return logger
}