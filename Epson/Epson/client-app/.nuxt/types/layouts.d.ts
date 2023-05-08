import { ComputedRef, Ref } from 'vue'
export type LayoutKey = string
declare module "C:/Users/richa/Documents/GitHub/Epson/Epson/Epson/client-app/node_modules/nuxt/dist/pages/runtime/composables" {
  interface PageMeta {
    layout?: false | LayoutKey | Ref<LayoutKey> | ComputedRef<LayoutKey>
  }
}