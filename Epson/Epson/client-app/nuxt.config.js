import axios from 'axios'

export default {
    modules: [
        '@nuxtjs/axios',
        '@nuxtjs/pwa',
        '@nuxtjs/auth-next'
    ],
    auth: {
        strategies: {
            local: {
                user: {
                    property: false,
                    autoFetch: true
                },
                endpoints: {
                    login: { url: 'api/customer/login', method: 'post' },
                    user: { url: 'api/customer/getcurrentuser', method: 'get' },
                    logout: { url: 'api/customer/logout', method: 'post' },
                }
            }
        },
        redirect: {
            login: '/login',
            logout: '/login',
            callback: '/login',
            home: '/dashboard'
        }
    },
    axios: {
        // baseURL: 'https://localhost:7223',
        proxy: true
    },
    proxy: {
        '/api': {
            target: 'https://localhost:7223',
            pathRewrite: { '^/api': '' },
            changeOrigin: true
        }
    }
}