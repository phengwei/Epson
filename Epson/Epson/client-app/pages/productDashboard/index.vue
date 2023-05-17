<template>
  <main class="dashboard-header"  v-if="loggedInUser.roles.includes('Product')">
    <div class="grid-container">
      <div class="row">
        <div class="grid-item">
          <v-app>
            <ItemsPendingFulfilmentTable />
          </v-app>
        </div>
        <div class="grid-item">
          <v-app>
            <FulfilledRequestAsFulfiller />
          </v-app>
        </div>
      </div>
      <div class="row">
        <div class="grid-item full-width request-table">
          <v-app>
            <FulfillmentRequestSummary />
          </v-app>
        </div>
      </div>
    </div>
  </main>
</template>

<script>
  import { mapGetters } from 'vuex';
  import ItemsPendingFulfilmentTable from '~/components/ItemsPendingFulfilmentTable.vue';
  import FulfillmentRequestSummary from '~/components/FulfillmentRequestSummary.vue';
  import FulfilledRequestAsFulfiller from '~/components/FulfilledRequestAsFulfiller.vue';

  export default {
    name: 'ProductDashboard',
    middleware: 'auth',
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {};
    },
    components: {
      ItemsPendingFulfilmentTable,
      FulfillmentRequestSummary,
      FulfilledRequestAsFulfiller
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
