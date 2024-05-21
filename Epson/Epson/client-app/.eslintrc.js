module.exports = {
  root: true,
  env: {
    browser: true,
    node: true,
  },
  parserOptions: {
    parser: '@babel/eslint-parser',
    requireConfigFile: false,
  },
  extends: ['@nuxtjs', 'plugin:nuxt/recommended', 'prettier'],
  plugins: [],
  // add your custom rules here
  rules: {
    "yoda": "off", // Disable yoda condition checks
    "eqeqeq": "error", // Enforce strict equality
    "no-undef": "warn", // Warn on undefined variables
    "no-unused-expressions": "error", // Disallow unused expressions
    "no-sequences": "error", // Disallow comma operators
    "no-var": "error", // Enforce let/const instead of var
    "one-var": ["error", "never"], // Enforce one variable per declaration
    'vue/valid-v-slot': ['error', {
      allowModifiers: true,
    }],
  },
  
}
