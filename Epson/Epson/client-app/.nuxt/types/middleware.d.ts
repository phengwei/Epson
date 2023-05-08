import type { NavigationGuard } from 'vue-router'
export type MiddlewareKey = "guest"
declare module "C:/Users/richa/Documents/GitHub/Epson/Epson/Epson/client-app/node_modules/nuxt/dist/pages/runtime/composables" {
  interface PageMeta {
    middleware?: MiddlewareKey | NavigationGuard | Array<MiddlewareKey | NavigationGuard>
  }
}