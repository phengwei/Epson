<template>
  <main class="dashboard-header" v-if="loggedInUser.roles.includes('Sales Section Head') || loggedInUser.roles.includes('Admin')">
    <div class="grid-container">
      <div class="row sales-head-table">
        <div class="grid-item">
          <v-app>
            <ItemsPendingSalesSectionHeadAction />
          </v-app>
        </div>
        <div class="grid-item">
          <v-app>
            <ItemsPendingSalesSectionHeadDepartment />
          </v-app>
        </div>
      </div>
      <div class="row">
        <div class="grid-item">
          <v-app>
            <PendingRequestSummary />
          </v-app>
        </div>
        <div class="grid-item">
          <v-app>
            <CompletedRequestSummary />
          </v-app>
        </div>
      </div>
    </div>
  </main>
</template>

<script>
  import { mapGetters } from 'vuex';
  import ItemsPendingSalesSectionHeadAction from '~/components/ItemsPendingSalesSectionHeadAction.vue';
  import ItemsPendingSalesSectionHeadDepartment from '~/components/ItemsPendingSalesSectionHeadDepartment.vue';
  import PendingRequestSummary from '~/components/PendingRequestSummary.vue';
  import CompletedRequestSummary from '~/components/CompletedRequestSummary.vue';

  export default {
    name: 'SHDashboard',
    middleware: 'auth',
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {};
    },
    components: {
      ItemsPendingSalesSectionHeadAction,
      ItemsPendingSalesSectionHeadDepartment,
      PendingRequestSummary,
      CompletedRequestSummary
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

  .dashboard-header .sales-head-table {
    margin-bottom: 1rem;
  }
</style>
