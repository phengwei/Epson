import webpack from 'webpack'
import i18n from './config/i18n'

var path = require('path');
require('dotenv').config()



export default {
  server: {
    port: 3001 // default: 3000
  },
  // Target: https://go.nuxtjs.dev/config-target
  target: 'static',

  // Global page headers: https://go.nuxtjs.dev/config-head
  head: {
    title: 'epson-ums',
    meta: [
      { charset: 'utf-8' },
      { name: 'viewport', content: 'width=device-width, initial-scale=1' },
      { hid: 'description', name: 'description', content: '' },
      { name: 'format-detection', content: 'telephone=no' },
    ],
    link: [{ rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' }],
  },

  // Global CSS: https://go.nuxtjs.dev/config-css
  css: [
    '~/assets/scss/tailwind.scss'
  ],

  
  // Plugins to run before rendering page: https://go.nuxtjs.dev/config-plugins
  plugins: [
    { src: '~/plugins/jquery-ui.js', ssr: false },

    { src: '~/plugins/vue-countdown.js', ssr: false }, 
    { src: '~/plugins/tailable.js', ssr: false }, 
    { src: '~/plugins/dateFilter.js', ssr: false }, 
    { src: '~/plugins/sanitize.js', ssr: false },
    { src: '~/plugins/client-detection.js', ssr: false },
    { src: '~/plugins/vue-chartkick.js', ssr: false },
   ],

  // Auto import components: https://go.nuxtjs.dev/config-components
  components: false,

  // Modules for dev and build (recommended): https://go.nuxtjs.dev/config-modules
  buildModules: [

    // https://go.nuxtjs.dev/eslint
    '@nuxtjs/eslint-module',
    // https://go.nuxtjs.dev/tailwindcss
    '@nuxtjs/tailwindcss',
    '@nuxtjs/vuetify',
    '@nuxt/postcss8',
    [
      'nuxt-i18n',
      {
        vueI18nLoader: true,
        defaultLocale: 'en',
         locales: [
          {
             code: 'en',
             name: 'English'
          },
          {
             code: 'zh',
             name: 'Chinese'
          }
        ],
        vueI18n: i18n
      }
     ],


      
     ['nuxt-fontawesome', {
      component: 'fa', //customize component name
      imports: [{
          set: '@fortawesome/free-solid-svg-icons',
          icons: ['faLightbulb',
                  'faHeart',
                  'faAt',
                  'faChevronRight',
                  'faChevronLeft',
                  'faMapMarkerAlt',
                  'faPhone', 
                  'faUserMd',
                  'faUser',
                  'faPlus',
                  'faMinus',
                  'faAmbulance', 
                  'faUpload',
                  'faDownload',
                  'faHome',
                  'faBars', 
                  'faGlobe',
                  'faTrashAlt',
                  'faArrowLeft', 
                  'faArrowRight', 
                  'faExclamationTriangle',
                 'faTimes','faStethoscope', 'faShareAlt',  'faCaretDown',  'faSearch', 'faTag', 'faQuoteLeft', 'faBuilding','faNewspaper', 'faAngleDoubleRight']
          },
                
          {set: '@fortawesome/free-brands-svg-icons',
          icons: ['faGithub','faFacebookF','faWaze','faInstagram','faYoutube','faLinkedinIn','faTwitter','faWhatsapp']
          },
                
          {set: '@fortawesome/free-regular-svg-icons',
          icons: ['faLightbulb','faEnvelope', 'faCalendarAlt','faEye', 'faBookmark', 'faPaperPlane',
                  'faCheckSquare', 'faClock']
          },
      ]
   }]

  ],

  styleResources: {
    scss: [
      '~assets/scss/mixins.scss',
      '~assets/scss/variables.scss'
    ]
  },

  // Modules: https://go.nuxtjs.dev/config-modules
  modules: [
    // https://go.nuxtjs.dev/axios
    '@nuxtjs/axios',
    // https://go.nuxtjs.dev/pwa
    '@nuxtjs/pwa',
    '@nuxtjs/auth-next',

    // '@nuxtjs/recaptcha',
    ['vue-scrollto/nuxt', { duration: 300 }],
    'vue-sweetalert2/nuxt',

     // With options
     '@nuxtjs/proxy'
   
  ],

  auth: {
    strategies: {
      local: {
        user: {
          property: false,
          autoFetch: true
        },
        endpoints: {
          login: { url: 'api/customer/login', method: 'post'},
          user: { url: 'api/customer/getcurrentuser', method: 'get' },
          logout: { url: 'api/customer/logout', method: 'post' },
        }
      }
    },
    redirect: {
      login: '/login',
      logout: '/login',
      callback: '/login',
      home: '/reporting'
    }
  },

  
  publicRuntimeConfig: {
    restUrl: 'https://epson-stg-dev.eba-8tvp5kuf.ap-southeast-1.elasticbeanstalk.com/',
    baseURL: process.env.BASE_URL,
  },
  privateRuntimeConfig: {
    myPrivateToken: process.env.PRIVATE_TOKEN,
    
  },
  router: {
    middleware: ['auth']
  },

  // Axios module configuration: https://go.nuxtjs.dev/config-axios
  axios: {
    baseURL: 'https://epson-stg-dev.eba-8tvp5kuf.ap-southeast-1.elasticbeanstalk.com/',
    https: true,
  },
  // PWA module configuration: https://go.nuxtjs.dev/pwa
  pwa: {
    manifest: {
      lang: 'en',
    },
  },
  build: {
    extractCSS: true,
    postcss: {
      plugins: { 
        'postcss-import': {},
        tailwindcss: path.resolve(__dirname, './tailwind.config.js'),
        autoprefixer: {},
      }
    },
    
    preset: {
      stage: 1 // see https://tailwindcss.com/docs/using-with-preprocessors#future-css-featuress
    },
    plugins: [
      new webpack.ProvidePlugin({
        $: 'jquery',
        jQuery: 'jquery',
      })
    ]
  },
  vuetify: {
    treeShake: true
  },


  
}
