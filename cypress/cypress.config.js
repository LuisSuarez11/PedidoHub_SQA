const { defineConfig } = require('cypress');

module.exports = defineConfig({
  e2e: {
    baseUrl: process.env.CYPRESS_baseUrl || 'http://localhost:5158',
    specPattern: 'e2e/**/*.cy.js',
    supportFile: false,
    video: false,
    screenshotsFolder: 'screenshots',
    viewportWidth: 1440,
    viewportHeight: 900,
    reporter: 'junit',
    reporterOptions: {
      mochaFile: '../Docs/evidencias/resultados/cypress-[hash].xml',
      toConsole: false
    }
  }
});
