<template>
  <div class="sla-management-container" v-if="loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Director') ">
    <v-card class="sla-card">
      <v-row>
        <v-col cols="12">
          <h2 class="blue-text">SLA CONFIGURATION</h2>
        </v-col>
        <v-col cols="12">
          <sla-settings />
        </v-col>
        <v-col cols="12" md="6">
          <sla-holiday />
        </v-col>
        <v-col cols="12" md="6">
          <sla-staffLeaves />
        </v-col>
      </v-row>
    </v-card>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  import slaStaffLeaves from '~/components/sla-staffleave.vue';
  import slaSettings from '~/components/sla-settings.vue';
  import slaHoliday from '~/components/sla-holiday.vue';

  export default {
    name: 'SLA',
    middleware: 'auth',
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    components: {
      slaSettings,
      slaHoliday,
      slaStaffLeaves
    }
  };
</script>

<style scoped>
  .sla-management-container {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 20px;
    box-sizing: border-box;
  }

  .sla-card {
    width: 100%;
    max-width: 1200px;
    padding: 20px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
    border-radius: 8px;
  }

  .blue-text {
    color: #003399;
  }

  .v-row {
    margin: 0;
  }
</style>
