<template>
  <main class="dashboard-header" v-if="loggedInUser.roles.includes('Product') || loggedInUser.roles.includes('Coverplus') || loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Director')">
    <div class="grid-container">
      <div class="row">
        <div class="grid-item full-width request-table">
          <v-app>
            <DemoRequestPendingFulfilment />
          </v-app>
        </div>
      </div>
      <div class="row">
        <div class="grid-item full-width request-table">
          <v-app>
            <FulfilledDemoRequests />
          </v-app>
        </div>
      </div>
      <!--<div class="row">
        <div class="grid-item full-width request-table">
          <v-app>
            <FulfillmentRequestSummary />
          </v-app>
        </div>
      </div>-->
    </div>
  </main>
</template>

<script>
  import { mapGetters } from 'vuex';
  import DemoRequestPendingFulfilment from '~/components/DemoRequestPendingFulfilment.vue';
  import FulfilledDemoRequests from '~/components/FulfilledDemoRequests.vue';

  export default {
    name: 'DirectorDashboard',
    middleware: 'auth',
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {};
    },
    components: {
      DemoRequestPendingFulfilment,
      FulfilledDemoRequests
    }
  };
</script>

<style>
  .v-application {
    font-family: TCCC-UnityText-Regular, TCCC-UnityText !important;
  }

  .grid-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    width: 90%;
    margin: 0 auto;
  }

  .row {
    display: flex;
    justify-content: space-between;
    width: 100%;
  }

  .grid-item {
    width: 45%;
  }

    .grid-item:not(:last-child) {
      margin-right: 20px;
    }

  .full-width {
    width: 100%;
  }

  .v-application--wrap {
    min-height: 20vh !important;
  }

  .dashboard-header {
    margin-top: 7rem;
  }

  .request-table {
    margin-top: 5rem;
    margin-bottom: 5rem;
  }
</style>
