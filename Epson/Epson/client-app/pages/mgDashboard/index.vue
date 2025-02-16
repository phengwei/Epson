<template>
  <main class="dashboard-header" v-if="loggedInUser.roles.includes('Sales Section Head') || loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Director')">
    <div class="grid-container">
      <div class="row">
        <div class="grid-item full-width sales-head-table">
          <v-app>
            <ItemsPendingSalesSectionHeadAction />
          </v-app>
        </div>
      </div>
      <div class="row">
        <div class="grid-item full-width sales-head-table">
          <v-app>
            <ItemsPendingSalesSectionHeadDepartment />
          </v-app>
        </div>
      </div>
      <!--<div class="row">
        <div class="grid-item full-width sales-head-table">
          <v-app>
            <SalesSectionRequestSummary />
          </v-app>
        </div>
      </div>-->
    </div>
  </main>
</template>

<script>
  import { mapGetters } from 'vuex';
  import ItemsPendingSalesSectionHeadAction from '~/components/ItemsPendingManagerAction.vue';
  import ItemsPendingSalesSectionHeadDepartment from '~/components/ItemsPendingSalesSectionHeadDepartment.vue';

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
      ItemsPendingSalesSectionHeadDepartment
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

  .sales-head-table {
    margin-top: 5rem;
    margin-bottom: 5rem;
  }
</style>
