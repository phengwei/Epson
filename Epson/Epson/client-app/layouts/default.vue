<template>
  <div>
    <MobileNav v-if="isAuthenticated" />
    <NavigationBar v-if="isAuthenticated" />
      <Nuxt class="mt-24"/>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  import NavigationBar from '~/components/NavigationBar.vue';
  import MobileNav from '~/components/MobileNav.vue';

  export default {
    auth: false,
    name: 'DefaultLayout',
    computed: {
      ...mapGetters(['isAuthenticated'])
    },
    components: {
      NavigationBar,
      MobileNav
    }
  }
</script>

