var winston = require('winston');
const { format } = require('winston');
require('winston-daily-rotate-file');

var transport = new (winston.transports.DailyRotateFile)({
  filename: './logs/log-%DATE%.log',
  datePattern: 'YYYY-MM-DD',
  zippedArchive: true,
  maxSize: '20m',
  maxFiles: '14d'
});

var tConsole = new (winston.transports.Console)({
  timestamp: true
});

transport.on('rotate', function (oldFilename, newFilename) {
  // do something fun
});

var logger = winston.createLogger({
  format: format.combine(
    format.timestamp({
      format: 'YYYY-MM-DD HH:mm:ss'
    }),
    format.printf(info => `${info.timestamp} ${info.level}: ${info.message}` + (info.splat !== undefined ? `${info.splat}` : " "))
  ),
  transports: [
    transport, tConsole
  ]
});


// /**
//  * Configurations of logger.
//  */
// const winston = require('winston');
// const winstonRotator = require('winston-daily-rotate-file');

// const consoleConfig = [
//   new winston.transports.Console({
//     'colorize': true
//   })
// ];

// const createLogger = new winston.createLogger({
//   'transports': consoleConfig
// });

// const logger = createLogger;
// logger.add(winstonRotator, {
//   'name': 'access-file',
//   'level': 'info',
//   'filename': './logs/access.log',
//   'json': false,
//   'datePattern': 'yyyy-MM-dd-',
//   'prepend': true
// });

// const errorLogger = createLogger;
// errorLogger.add(winstonRotator, {
//   'name': 'error-file',
//   'level': 'error',
//   'filename': './logs/error.log',
//   'json': false,
//   'datePattern': 'yyyy-MM-dd-',
//   'prepend': true
// });

// module.exports = {
//   'logger': logger,
//   'errorlog': errorLogger
// };