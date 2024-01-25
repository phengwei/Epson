<template>
  <main class="dashboard-header" v-if="loggedInUser.roles.includes('Sales')">
    <div class="grid-container">
      <div class="row">
        <div class="grid-item">
          <v-app>
            <ItemsPendingRequesterAction />
          </v-app>
        </div>
        <div class="grid-item">
          <v-app>
            <ItemsPendingFulfillmentAsRequester />
          </v-app>
        </div>
      </div>
      <div class="row">
        <div class="grid-item full-width request-table">
          <v-app>
            <SalesRequestSummary />
          </v-app>
        </div>
      </div>
    </div>
  </main>
</template>

<script>
  import { mapGetters } from 'vuex';
  import ItemsPendingRequesterAction from '~/components/ItemsPendingRequesterAction.vue';
  import ItemsPendingFulfillmentAsRequester from '~/components/ItemsPendingFulfillmentAsRequester.vue';
  import SalesRequestSummary from '~/components/SalesRequestSummary.vue';

  export default {
    name: 'SalesDashboard',
    middleware: 'auth',
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {};
    },
    components: {
      ItemsPendingRequesterAction,
      ItemsPendingFulfillmentAsRequester,
      SalesRequestSummary
    }
  };
</script>

<style>
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
