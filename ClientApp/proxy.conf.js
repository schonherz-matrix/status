const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:30001';

const PROXY_CONFIG = [
  {
    context: [
      "/hubs/status"
    ],
    target: target,
    secure: false,
    ws: true,
  }
]

module.exports = PROXY_CONFIG;
