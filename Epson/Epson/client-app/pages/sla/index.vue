<template>
  <div class="sla-management" v-if="loggedInUser.roles.includes('Admin')">
    <sla-settings />
    <sla-holiday />
    <sla-staffLeaves />
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  import slaStaffLeaves from '~/components/sla-staffleave.vue';
  import slaSettings from '~/components/sla-settings.vue';
  import slaHoliday from '~/components/sla-holiday.vue';
  export default {
    name: 'SLA',
    middleware: "auth",
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    components: {
      slaSettings,
      slaHoliday,
      slaStaffLeaves
    }
  }
</script>

<style scoped>
  .sla-management-container {
    display: flex;
    justify-content: center;
    flex-direction: column;
    align-items: center;
    height: 100vh;
  }
  .sla-management {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
  }
</style>
